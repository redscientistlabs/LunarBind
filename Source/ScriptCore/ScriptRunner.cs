namespace ScriptCore
{
    using System;
    using System.Collections.Generic;
    using MoonSharp.Interpreter;
    using ScriptCore.Yielders;

    public class ScriptRunner
    {
        private Dictionary<string,ScriptContainer> GlobalScripts = new Dictionary<string, ScriptContainer>();
        private ScriptContainer CurrentTempScript = null;

        //Todo: better abort options
        private volatile bool Cancelled = false;

        private Script lua;

        private ScriptContainer runningScript = null;


        public ScriptRunner()
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

            //TODO: set up other constant globals

            ScriptInitializer.Initialize(lua);
        }

        public void LoadScript(string scriptString)
        {
            if (string.IsNullOrWhiteSpace(scriptString))
            {
                //No script
                return;
            }
            var scr = new ScriptContainer(scriptString);
            CurrentTempScript?.ResetHooks();
            CurrentTempScript = scr;

            runningScript = scr;
            lua.DoString(scr.ScriptString);
            runningScript = null;
            //TODO: uncomment if memory becomes an issue somehow
            //GC.Collect();
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
                if (!Cancelled)
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
            }
            catch(ScriptRuntimeException ex)
            {
                //Todo: error handling
                throw ex;
            }
        }

        private void RunLua(ScriptContainer script, string hookName, params object[] args)
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

        public void Abort()
        {
            Cancelled = true;
        }
    }
}
