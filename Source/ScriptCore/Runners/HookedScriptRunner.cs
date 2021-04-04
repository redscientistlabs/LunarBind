namespace ScriptCore
{
    using MoonSharp.Interpreter;
    using ScriptCore.Yielding;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    public sealed class HookedScriptRunner
    {
        public Script Lua { get; private set; }

        private HookedScriptContainer scriptContainer = null;

        public HookedScriptRunner()
        {
            Lua = new Script(CoreModules.Preset_HardSandbox | CoreModules.Coroutine | CoreModules.OS_Time);

            Lua.Globals["RegisterHook"] = (Action<DynValue, string>)RegisterHook;
            Lua.Globals["RegisterCoroutine"] = (Action<DynValue, string>)RegisterCoroutine;
            Lua.Globals["RemoveHook"] = (Action<string>)RemoveHook;
            //Global init
            GlobalScriptBindings.Initialize(Lua);
            //GlobalScriptBindings.InitializeYieldables(Lua);
        }

        public HookedScriptRunner(ScriptBindings bindings) : this()
        {
            bindings.Initialize(Lua);
        }
        public void AddBindings(ScriptBindings bindings)
        {
            bindings.Initialize(Lua);
        }

        public void LoadScript(string scriptString)
        {
            scriptContainer?.ResetHooks();
            scriptContainer = new HookedScriptContainer(scriptString);
            //Initialize the hookable script
            Lua.DoString(scriptContainer.ScriptString);
        }

        public void RegisterHookDoneCallback(string hook, Action callback)
        {
            var h = scriptContainer.GetHook(hook);
            if (h != null) h.Done += callback;
        }

        public void UnRegisterHookDoneCallback(string hook, Action callback)
        {
            var h = scriptContainer.GetHook(hook);
            if(h != null) h.Done -= callback;
        }

        #region callbacks
        void RegisterCoroutine(DynValue del, string name)
        {
            var coroutine = Lua.CreateCoroutine(del);
            scriptContainer.AddHook(name, new ScriptHook(coroutine, true));
        }

        void RegisterHook(DynValue del, string name)
        {
            scriptContainer.AddHook(name, new ScriptHook(del));
        }

        void RemoveHook(string name)
        {
            scriptContainer.RemoveHook(name);
        }
        #endregion

        public void Execute(string hookName, params object[] args)
        {
            try
            {
                RunLua(scriptContainer, hookName, null, args);
            }
            catch (ScriptRuntimeException ex)
            {
                //Todo: error handling
                throw ex;
            }
        }

        public void ExecuteWithCallback(string hookName, Action callback, params object[] args)
        {
            try
            {
                RunLua(scriptContainer, hookName, callback, args);
            }
            catch (ScriptRuntimeException ex)
            {
                //Todo: error handling
                throw ex;
            }
        }


        public T Query<T>(string hookName, params object[] args)
        {
            try
            {
                var ret = RunLua(scriptContainer, hookName, null, args);
                if (ret != null)
                {
                    return ret.ToObject<T>();
                }
                else
                {
                    return default;
                }
            }
            catch (ScriptRuntimeException ex)
            {
                //Todo: error handling
                throw ex;
            }
        }

        private DynValue RunLua(HookedScriptContainer script, string hookName, Action callback, object[] args)
        {
            var hook = script.GetHook(hookName);
            if (hook != null)
            {
                if (hook.IsCoroutine)
                {
                    if (hook.LuaFunc.Coroutine.State == CoroutineState.Dead || !hook.CheckYieldStatus()) //Doesn't run check yield if coroutine is dead
                    {
                        return null;
                    }
                    DynValue ret = hook.LuaFunc.Coroutine.Resume(args);
                    switch (hook.LuaFunc.Coroutine.State)
                    {
                        case CoroutineState.Suspended:

                            if (ret.IsNotNil())
                            {
                                Yielder yielder = ret.ToObject<Yielder>();
                                hook.CurYielder = yielder;
                            }
                            else
                            {
                                hook.CurYielder = null;
                            }
                            break;
                        case CoroutineState.Dead:
                            hook.CurYielder = null;
                            callback?.Invoke();
                            hook.OnDone();
                            break;
                        default:
                            break;
                    }
                    return ret;
                }
                else
                {
                    var ret = Lua.Call(hook.LuaFunc, args);
                    callback?.Invoke();
                    hook.OnDone();
                    return ret;
                }
            }
            else
            {
                return null;
            }
        }

        public object this[string id]
        {
            get
            {
                return Lua.Globals.Get(id);
            }
            set
            {
                Lua.Globals.Set(id, DynValue.FromObject(Lua, value));
            }
        }

    }
}
