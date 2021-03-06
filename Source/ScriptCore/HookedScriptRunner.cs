namespace ScriptCore
{
    using System;
    using System.Collections.Generic;
    using MoonSharp.Interpreter;
    using ScriptCore.Yielders;

    public class HookedScriptRunner
    {
        private Dictionary<string,HookedScriptContainer> GlobalScripts = new Dictionary<string, HookedScriptContainer>();
        private HookedScriptContainer CurrentTempScript = null;

        //Todo: better abort options
        //private volatile bool Cancelled = false;

        private Script lua;

        private HookedScriptContainer runningScript = null;


        public HookedScriptRunner()
        {
            lua = new Script(CoreModules.Preset_HardSandbox | CoreModules.Coroutine | CoreModules.OS_Time);

            //scriptrunner local methods, will have to hardcode documentation for this
            lua.Globals["RegisterHook"] = (Action<DynValue, string>)RegisterHook;
            lua.Globals["RemoveHook"] = (Action<string>)RemoveHook;
            lua.Globals["MakeGlobal"] = (Action<string>)MakeGlobal;
            lua.Globals["RemoveGlobal"] = (Action<string>)RemoveGlobal;
            lua.Globals["ResetGlobals"] = (Action)ResetGlobals;

            //Yielding    
            lua.Globals[ScriptConstants.LUA_YIELD] = null;

            //Default code
            const string defaultCode = @"
            function waitFrames(frames)
                LUA_YIELD = WaitForFrames(frames)
                coroutine.yield()
            end
            function waitFrame()
                LUA_YIELD = WaitForFrames(0)
                coroutine.yield()
            end

            function RegisterCoroutine(co, name)
                local cor = coroutine.create(co)

                local cfunc = function()
                    if coroutine.status(cor) ~= 'dead' then coroutine.resume(cor) end
                end

                RegisterHook(cfunc,name)
            end
            ";

            lua.DoString(defaultCode);


            //Global init
            GlobalScriptBindings.Initialize(lua);
        }

        public HookedScriptRunner(ScriptBindings bindings) : this()
        {
            bindings.Initialize(lua);
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
            lua.DoString(scr.ScriptString);
            runningScript = null;
            //GC.Collect(); <- checked, no memory leaks, but dead objects build up fast without the GC actively collecting
        }

        /// <summary>
        /// Runs the script immediately without hooking
        /// </summary>
        /// <param name="scriptString"></param>
        /// <param name="hook"></param>
        public DynValue RunScriptNow(string scriptString)
        {
            return lua.DoString(scriptString);
        }


        #region callbacks
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

        public void Execute(string hookName, params object[] args)
        {
            try
            {
                //if (!Cancelled)
                //{
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
                //}
            }
            catch(ScriptRuntimeException ex)
            {
                //Todo: error handling
                throw ex;
            }
        }

        private void RunLua(HookedScriptContainer script, string hookName, params object[] args)
        {
            var hook = script.GetHook(hookName);
            if (hook != null && hook.CheckYieldStatus())
            {
                lua.Call(hook.LuaFunc, args);
                var yieldObj = lua.Globals[ScriptConstants.LUA_YIELD];

                if (yieldObj is Yielder yielder)
                {
                    hook.CurYielder = yielder;
                }
                else if(yieldObj != null)
                {
                    //TODO: Throw error
                }
            }
        }

    }
}
