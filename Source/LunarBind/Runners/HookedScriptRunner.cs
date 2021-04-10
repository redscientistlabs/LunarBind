namespace LunarBind
{
    using MoonSharp.Interpreter;
    using LunarBind.Yielding;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using LunarBind.Standards;
    public sealed class HookedScriptRunner : ScriptRunnerBase
    {
        public string Name { get; set; } = nameof(HookedScriptRunner);

        private readonly HookedScriptContainer scriptContainer = new HookedScriptContainer();
        
        /// <summary>
        /// A standard all loaded scripts must follow
        /// </summary>
        public LuaScriptStandard ScriptStandard { get; private set; } = null;
        public bool AutoResetCoroutines { get; set; } = false;

        /// <summary>
        /// Get a reference to the 
        /// </summary>
        public Dictionary<string, ScriptFunction> Functions => scriptContainer.ScriptFunctions;

        public ScriptFunction GetFunction(string name)
        {
            return scriptContainer.GetHook(name);
        }

        public HookedScriptRunner()
        {
            Lua = new Script(CoreModules.Preset_HardSandbox | CoreModules.Coroutine | CoreModules.OS_Time);
            Lua.Globals["RegisterHook"] = (Action<DynValue, string>)RegisterHook;
            Lua.Globals["RegisterCoroutine"] = (Action<DynValue, string, bool>)RegisterCoroutine;
            Lua.Globals["RemoveHook"] = (Action<string>)RemoveHook;
            //Global init
            GlobalScriptBindings.Initialize(Lua);
        }

        public HookedScriptRunner(ScriptBindings bindings, LuaScriptStandard standard = null) : this()
        {
            bindings.Initialize(Lua);
            ScriptStandard = standard;
        }

        public HookedScriptRunner(LuaScriptStandard standard, ScriptBindings bindings = null) : this()
        {
            bindings?.Initialize(Lua);
            ScriptStandard = standard;
        }

        public void AddBindings(ScriptBindings bindings)
        {
            bindings.Initialize(Lua);
        }

        public void SetStandard(LuaScriptStandard standard)
        {
            ScriptStandard = standard;
        }

        public void LoadScript(string scriptString, string scriptName = "User Code")
        {
            scriptContainer.ResetHooks();
            //scriptContainer.ScriptString = scriptString;
            //Lua.Globals.CollectDeadKeys();

            Lua.DoString(scriptString, null, scriptName);
                
            if (ScriptStandard != null)
            {
                List<string> errors = new List<string>();
                bool res = ScriptStandard.ApplyStandard(Lua, scriptContainer, errors);
                if (!res)
                {
                    //Todo: new type of exception with info
                    throw new Exception($"Script Standard was not met! Standards Not Met: [{string.Join(", ", errors)}]");
                }
            }
        }

        #region callbacks
        void RegisterCoroutine(DynValue del, string name, bool autoReset = false)
        {
            //var coroutine = Lua.CreateCoroutine(del);
            scriptContainer.SetHook(name, new ScriptFunction(Lua, del, true, autoReset));
        }

        void RegisterHook(DynValue del, string name)
        {
            scriptContainer.SetHook(name, new ScriptFunction(Lua, del, false));
        }

        void RemoveHook(string name)
        {
            scriptContainer.RemoveHook(name);
        }
        #endregion

        public void ResetCoroutine(string name)
        {
            var hook = scriptContainer.GetHook(name);
            hook.Coroutine.Assign(Lua.CreateCoroutine(hook.LuaFunc));
        }

        public void Execute(string hookName, params object[] args)
        {
            //try
            //{
                scriptContainer.GetHook(hookName)?.Execute(args);
                //RunLua(scriptContainer, hookName, null, args);
            //}
            //catch (Exception ex)
            //{
            //    if (ex is InterpreterException e)
            //    {
            //        throw new Exception(e.DecoratedMessage);
            //    }

            //    throw ex;
            //}
        }

        public void ExecuteWithCallback(string hookName, Action callback, params object[] args)
        {
            //try
            //{
                scriptContainer.GetHook(hookName)?.ExecuteWithCallback(callback, args);
                //RunLua(scriptContainer, hookName, callback, args);
            //}
            //catch (Exception ex)
            //{
            //    if (ex is InterpreterException e)
            //    {
            //        throw new Exception(e.DecoratedMessage);
            //    }

            //    throw ex;
            //}
        }

        public IEnumerator CreateUnityCoroutine(string hookName, params object[] args)
        {
            CoroutineState state = CoroutineState.NotStarted;
            var hook = scriptContainer.GetHook(hookName);
            if (hook == null)
            {
                throw new Exception($"Hook {hookName} does not exist on script");
            }
            //Create new hook with coroutine
            hook = new ScriptFunction(hook) { AutoResetCoroutine = false };
            while (state != CoroutineState.Dead)
            {
                state = RunAsUnityCoroutine(hook, null, args);
                yield return null;
            }
        }

        public IEnumerator CreateUnityCoroutine(string hookName, Action callback, params object[] args)
        {
            CoroutineState state = CoroutineState.NotStarted;
            var hook = scriptContainer.GetHook(hookName);
            if(hook == null)
            {
                throw new Exception($"Hook {hookName} does not exist on script");
            }
            hook = new ScriptFunction(hook) { AutoResetCoroutine = false };
            while (state != CoroutineState.Dead)
            {
                state = RunAsUnityCoroutine(hook, callback, args);
                yield return null;
            }
        }

        public T Query<T>(string hookName, params object[] args)
        {
            try
            {
                var ret = scriptContainer.GetHook(hookName)?.Execute(args);
                    //RunLua(scriptContainer, hookName, null, args);
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

        private CoroutineState RunAsUnityCoroutine(ScriptFunction hook, Action callback, object[] args)
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
                var co = hook.Coroutine.Coroutine;

                //DynValue ret = hook.Coroutine.Coroutine.Resume(args);

                DynValue ret;// = co.Resume(args);

                if (co.State == CoroutineState.NotStarted)
                {
                    ret = co.Resume(args);
                }
                else
                {
                    ret = co.Resume();
                }

                switch (co.State)
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
                        if (AutoResetCoroutines || hook.AutoResetCoroutine)
                        {
                            hook.Coroutine.Assign(Lua.CreateCoroutine(hook.LuaFunc));
                        }
                        return CoroutineState.Dead;
                    default:
                        break;
                }
                return co.State;
            }
            else
            {
                var ret = Lua.Call(hook.LuaFunc, args);
                callback?.Invoke();
                return CoroutineState.Dead;
            }
        }

        //private DynValue RunLua(HookedScriptContainer script, string hookName, Action callback, object[] args)
        //{
        //    var hook = script.GetHook(hookName);
        //    if (hook != null)
        //    {
        //        if (hook.IsCoroutine)
        //        {
        //            if (hook.Coroutine.Coroutine.State == CoroutineState.Dead || !hook.CheckYieldStatus()) //Doesn't run check yield if coroutine is dead
        //            {
        //                return null;
        //            }
        //            DynValue ret = hook.Coroutine.Coroutine.Resume(args);

        //            switch (hook.Coroutine.Coroutine.State)
        //            {
        //                case CoroutineState.Suspended:

        //                    if (ret.IsNotNil())
        //                    {
        //                        Yielder yielder = ret.ToObject<Yielder>();
        //                        hook.CurYielder = yielder;
        //                    }
        //                    else
        //                    {
        //                        hook.CurYielder = null;
        //                    }
        //                    break;
        //                case CoroutineState.Dead:
        //                    hook.CurYielder = null;
        //                    callback?.Invoke();
        //                    if (AutoResetCoroutines || hook.AutoResetCoroutine)
        //                    {
        //                        hook.Coroutine.Assign(Lua.CreateCoroutine(hook.LuaFunc));
        //                    }
        //                    break;
        //                default:
        //                    break;
        //            }
        //            return ret;
        //        }
        //        else
        //        {
        //            var ret = Lua.Call(hook.LuaFunc, args);
        //            callback?.Invoke();
        //            return ret;
        //        }
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}

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
