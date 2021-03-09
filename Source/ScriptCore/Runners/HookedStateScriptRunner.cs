﻿namespace ScriptCore
{
    using System;
    using System.Collections.Generic;
    using MoonSharp.Interpreter;
    using ScriptCore.Yielding;
    public class HookedStateScriptRunner
    {
        private Dictionary<string,HookedScriptContainer> GlobalScripts = new Dictionary<string, HookedScriptContainer>();
        private HookedScriptContainer CurrentTempScript = null;

        private Script lua;

        private HookedScriptContainer runningScript = null;
        const string COROUTINE_PREFIX_ = nameof(COROUTINE_PREFIX_);

        public HookedStateScriptRunner()
        {
            lua = new Script(CoreModules.Preset_HardSandbox | CoreModules.Coroutine | CoreModules.OS_Time);

            lua.Globals["RegisterHook"] = (Action<DynValue, string>)RegisterHook;
            lua.Globals["RegisterCoroutine"] = (Action<DynValue, string>)RegisterCoroutine;
            lua.Globals["RemoveHook"] = (Action<string>)RemoveHook;
            lua.Globals["MakeGlobal"] = (Action<string>)MakeGlobal;
            lua.Globals["RemoveGlobal"] = (Action<string>)RemoveGlobal;
            lua.Globals["ResetGlobals"] = (Action)ResetGlobals;


            //Global init
            GlobalScriptBindings.Initialize(lua);
            GlobalScriptBindings.InitializeYieldables(lua);
        }

        public HookedStateScriptRunner(ScriptBindings bindings) : this()
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

        void RegisterCoroutine(DynValue del, string name)
        {
            if (runningScript == null) { return; }
            var coroutine = lua.CreateCoroutine(del);
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
                            Yielder yielder = ret.ToObject<Yielder>();
                            hook.CurYielder = yielder;
                            break;
                        case CoroutineState.Dead:
                            hook.CurYielder = null;
                            hook.IsCoroutineDead = true;
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                     lua.Call(hook.LuaFunc, args);
                }
            }
        }

    }
}