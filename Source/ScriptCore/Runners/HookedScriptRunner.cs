namespace ScriptCore
{
    using MoonSharp.Interpreter;
    using ScriptCore.Yielding;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    public sealed class HookedScriptRunner
    {
        public Script Lua { get; private set; }

        private HookedScriptContainer scriptContainer = new HookedScriptContainer();
        public HookStandard HookStandard { get; private set; } = null;
        public bool AutoResetCoroutines { get; set; } = false;

        int autoRefreshInterval = 0;
        int curAutoRefreshCoundown = 0;

        /// <summary>
        /// Set the auto refresh rate, to clear moonsharp's bulky storage of source code. 0 means never.
        /// </summary>
        public int AutoRefreshInterval{ get => autoRefreshInterval; set { autoRefreshInterval = value; curAutoRefreshCoundown = value; } }

        private object luaSources;
        private System.Reflection.MethodInfo luaSourceClear;

        public List<string> GetHookNames()
        {
            return new List<string>(scriptContainer.Hooks.Keys);
        }

        public HookedScriptRunner()
        {
            Lua = new Script(CoreModules.Preset_HardSandbox | CoreModules.Coroutine | CoreModules.OS_Time);
            luaSources = typeof(Script).GetField("m_Sources", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(Lua);
            luaSourceClear = luaSources.GetType().GetMethod("Clear");
            Lua.Globals["RegisterHook"] = (Action<DynValue, string>)RegisterHook;
            Lua.Globals["RegisterCoroutine"] = (Action<DynValue, string, bool>)RegisterCoroutine;
            Lua.Globals["RemoveHook"] = (Action<string>)RemoveHook;
            //Global init
            GlobalScriptBindings.Initialize(Lua);
            //GlobalScriptBindings.InitializeYieldables(Lua);
        }

        public HookedScriptRunner(ScriptBindings bindings, HookStandard standard = null) : this()
        {
            bindings.Initialize(Lua);
            HookStandard = standard;
        }

        public HookedScriptRunner(HookStandard standard, ScriptBindings bindings = null) : this()
        {
            bindings?.Initialize(Lua);
            HookStandard = standard;
        }

        public void AddBindings(ScriptBindings bindings)
        {
            bindings.Initialize(Lua);
        }

        public void SetStandard(HookStandard standard)
        {
            HookStandard = standard;
        }

        /// <summary>
        /// This method exists because moonsharp stores the source code for everything.
        /// </summary>
        public void RefreshLua()
        {
            //var field = typeof(Script).GetField("m_Sources", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(Lua);
            luaSourceClear.Invoke(luaSources,null);
            curAutoRefreshCoundown = autoRefreshInterval;
        }

        public void LoadScript(string scriptString, string scriptName = "User Code")
        {
            scriptContainer.ResetHooks();
            scriptContainer.SetScript(scriptString);
            //Initialize the hookable script

            Lua.Globals.CollectDeadKeys();
            if(curAutoRefreshCoundown > 0 && --curAutoRefreshCoundown == 0)
            {
                RefreshLua();
            }
            
            try
            {
                Lua.DoString(scriptContainer.ScriptString, null, scriptName);
                
                if (HookStandard != null)
                {
                    List<string> errors = new List<string>();
                    bool res = HookStandard.ApplyStandard(Lua, scriptContainer, errors);
                    if (!res)
                    {
                        //Todo: new type of exception with info
                        throw new Exception($"Script Standard was not met! Standards Not Met: [{string.Join(", ", errors)}]");
                    }
                }
            }
            catch(Exception ex)
            {
                if(ex is InterpreterException e)
                {
                    throw new Exception(e.DecoratedMessage);
                }

                throw ex;
            }
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
        void RegisterCoroutine(DynValue del, string name, bool autoReset = false)
        {
            var coroutine = Lua.CreateCoroutine(del);
            scriptContainer.AddHook(name, new ScriptHook(del, coroutine, autoReset));
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

        public void AutoHookAsCoroutines()
        {

        }

        public void ResetCoroutine(string name)
        {
            var hook = scriptContainer.GetHook(name);
            hook.Coroutine.Assign(Lua.CreateCoroutine(hook.LuaFunc));
        }

        public void Execute(string hookName, params object[] args)
        {
            try
            {
                RunLua(scriptContainer, hookName, null, args);
            }
            catch (Exception ex)
            {
                if (ex is InterpreterException e)
                {
                    throw new Exception(e.DecoratedMessage);
                }

                throw ex;
            }
        }

        public void ExecuteWithCallback(string hookName, Action callback, params object[] args)
        {
            try
            {
                RunLua(scriptContainer, hookName, callback, args);
            }
            catch (Exception ex)
            {
                if (ex is InterpreterException e)
                {
                    throw new Exception(e.DecoratedMessage);
                }

                throw ex;
            }
        }

        public IEnumerator GetIEnumeratorLoop(string hookName, Action callback, params object[] args)
        {
            CoroutineState state = CoroutineState.NotStarted;
            while (state != CoroutineState.Dead)
            {
                try
                {
                    state = RunAsCoroutine(scriptContainer, hookName, callback, args);
                }
                catch (Exception ex)
                {
                    if (ex is InterpreterException e)
                    {
                        throw new Exception(e.DecoratedMessage);
                    }
                    throw ex;
                }
                yield return null;
            }
        }

        public IEnumerator GetIEnumeratorLoopWithCallback(string hookName, Action callback, params object[] args)
        {
            CoroutineState state = CoroutineState.NotStarted;
            while (state != CoroutineState.Dead)
            {
                try
                {
                    state = RunAsCoroutine(scriptContainer, hookName, callback, args);
                }
                catch (Exception ex)
                {
                    if (ex is InterpreterException e)
                    {
                        throw new Exception(e.DecoratedMessage);
                    }
                    throw ex;
                }
                yield return null;
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
            catch (Exception ex)
            {
                if (ex is InterpreterException e)
                {
                    throw new Exception(e.DecoratedMessage);
                }

                throw ex;
            }
        }

        private CoroutineState RunAsCoroutine(HookedScriptContainer script, string hookName, Action callback, object[] args)
        {
            var hook = script.GetHook(hookName);
            if (hook != null)
            {
                if (hook.IsCoroutine)
                {
                    if (hook.Coroutine.Coroutine.State == CoroutineState.Dead)
                    {
                        return CoroutineState.Dead;
                    }

                    if (!hook.CheckYieldStatus())
                    {
                        return CoroutineState.Suspended;
                    }
                    DynValue ret = hook.Coroutine.Coroutine.Resume(args);
                    switch (hook.Coroutine.Coroutine.State)
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
                            return CoroutineState.Suspended;
                        case CoroutineState.Dead:
                            hook.CurYielder = null;
                            callback?.Invoke();
                            hook.OnDone();
                            if (AutoResetCoroutines || hook.AutoResetCoroutine)
                            {
                                hook.Coroutine.Assign(Lua.CreateCoroutine(hook.LuaFunc));
                            }
                            return CoroutineState.Dead;
                        default:
                            break;
                    }
                    return hook.Coroutine.Coroutine.State;
                }
                else
                {
                    throw new Exception("Hook is not coroutine");
                }
            }
            else
            {
                var ret = Lua.Call(hook.LuaFunc, args);
                callback?.Invoke();
                hook.OnDone();
                return CoroutineState.Dead;
            }
        }


        private DynValue RunLua(HookedScriptContainer script, string hookName, Action callback, object[] args)
        {
            var hook = script.GetHook(hookName);
            if (hook != null)
            {
                if (hook.IsCoroutine)
                {
                    if (hook.Coroutine.Coroutine.State == CoroutineState.Dead || !hook.CheckYieldStatus()) //Doesn't run check yield if coroutine is dead
                    {
                        return null;
                    }
                    DynValue ret = hook.Coroutine.Coroutine.Resume(args);
                    switch (hook.Coroutine.Coroutine.State)
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
                            if (AutoResetCoroutines || hook.AutoResetCoroutine)
                            {
                                hook.Coroutine.Assign(Lua.CreateCoroutine(hook.LuaFunc));
                            }
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
