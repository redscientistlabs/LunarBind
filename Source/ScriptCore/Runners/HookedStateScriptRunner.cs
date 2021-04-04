namespace ScriptCore
{
    using System;
    using System.Collections.Generic;
    using MoonSharp.Interpreter;
    using ScriptCore.Yielding;

    /// <summary>
    /// Can run multiple scripts at once
    /// </summary>
    public sealed class HookedStateScriptRunner
    {
        public Script Lua { get; private set; }
        private Dictionary<string,HookedScriptContainer> GlobalScripts = new Dictionary<string, HookedScriptContainer>();
        private HookedScriptContainer CurrentTempScript = null;


        private HookedScriptContainer runningScript = null;

        public HookedStateScriptRunner()
        {
            Lua = new Script(CoreModules.Preset_HardSandbox | CoreModules.Coroutine | CoreModules.OS_Time);

            Lua.Globals["RegisterHook"] = (Action<DynValue, string>)RegisterHook;
            Lua.Globals["RegisterCoroutine"] = (Action<DynValue, string>)RegisterCoroutine;
            Lua.Globals["RemoveHook"] = (Action<string>)RemoveHook;
            Lua.Globals["MakeGlobal"] = (Action<string>)MakeGlobal;
            Lua.Globals["RemoveGlobal"] = (Action<string>)RemoveGlobal;
            Lua.Globals["ResetGlobals"] = (Action)ResetGlobals;

            //Global init
            GlobalScriptBindings.Initialize(Lua);
            GlobalScriptBindings.InitializeYieldables(Lua);
        }

        public HookedStateScriptRunner(ScriptBindings bindings) : this()
        {
            bindings.Initialize(Lua);
        }

        public void LoadScript(string scriptString)
        {
            CurrentTempScript?.ResetHooks();
            if (string.IsNullOrWhiteSpace(scriptString))
            {
                //No script
                CurrentTempScript = null;
                return;
            }
            var scr = new HookedScriptContainer(scriptString);
            CurrentTempScript = scr;

            runningScript = scr;
            Lua.DoString(scr.ScriptString);
            runningScript = null;
        }

        /// <summary>
        /// Runs the script immediately without hooking
        /// </summary>
        /// <param name="scriptString"></param>
        /// <param name="hook"></param>
        public DynValue RunScriptNow(string scriptString)
        {
            return Lua.DoString(scriptString);
        }


        #region callbacks

        void RegisterCoroutine(DynValue del, string name)
        {
            if (runningScript == null) { return; }
            var coroutine = Lua.CreateCoroutine(del);
            runningScript.Hooks[name] = new ScriptHook(coroutine,true);
        }

        void RegisterHook(DynValue del, string name)
        {
            if (runningScript == null) { return; }
            runningScript.Hooks[name] = new ScriptHook(del);
        }

        void RemoveHook(string name)
        {
            if (runningScript == null) { return; }
            runningScript.Hooks.Remove(name);
        }
        void MakeGlobal(string name)
        {
            if (runningScript == null) { return; }
            if (!GlobalScripts.ContainsKey(name))
            {
                //Unique global scripts
                GlobalScripts[name] = runningScript;
            }
            CurrentTempScript = null; //Remove temp so as to not dupe either way
        }
        void RemoveGlobal(string name)
        {
            if (runningScript == null) { return; }
            if (GlobalScripts.ContainsKey(name))
            {
                GlobalScripts[name].ResetHooks();
                GlobalScripts.Remove(name);
            }
        }
        void ResetGlobals()
        {
            if (runningScript == null) { return; }
            foreach (var scr in GlobalScripts)
            {
                scr.Value.ResetHooks();
            }
            GlobalScripts.Clear();
        }

        #endregion

        /// <summary>
        /// Call a hooked lua function.
        /// <para/>
        /// If you want to pass in a single array and want to access it in a single parameter in lua, convert it to a List first: 
        /// <para/>
        /// C#: runner.Execute("func", List&lt;string&gt;);
        /// <para/>
        /// Lua: function func(list)
        /// </summary>
        /// <param name="hookName"></param>
        /// <param name="args"></param>
        public void Execute(string hookName, params object[] args)
        {
            try
            {
                foreach (var script in GlobalScripts.Values)
                {
                    runningScript = script;
                    RunLua(script, hookName, args);
                    runningScript = null;
                }

                if (CurrentTempScript != null)
                {
                    runningScript = CurrentTempScript;
                    RunLua(CurrentTempScript, hookName, args);
                    runningScript = null;
                }
            }
            catch (ScriptRuntimeException ex)
            {
                //Todo: error handling
                throw ex;
            }
        }

        private void RunLua(HookedScriptContainer script, string hookName, params object[] args)
        {
            var hook = script.GetHook(hookName);
            if (hook != null)
            {
                if (hook.IsCoroutine) 
                {
                    if (hook.LuaFunc.Coroutine.State == CoroutineState.Dead || !hook.CheckYieldStatus()) //Doesn't run check yield if coroutine is dead
                    {
                        return;
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
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                     Lua.Call(hook.LuaFunc, args);
                }
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
