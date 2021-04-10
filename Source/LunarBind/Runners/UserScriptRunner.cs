namespace LunarBind.Runners
{
    using MoonSharp.Interpreter;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Provides a sandboxed lua runner that utilizes GlobalScriptBindings and ScriptBindings
    /// </summary>
    public abstract class UserScriptRunner<T> : ScriptRunnerBase
    {
        //public Script Lua { get; private set; }

        protected HookedScriptContainer scriptContainer = null;

        public UserScriptRunner()
        {
            Lua = new Script(CoreModules.Preset_HardSandbox | CoreModules.Coroutine | CoreModules.OS_Time);
            Lua.Globals["RegisterHook"] = (Action<DynValue, string>)RegisterHook;
            Lua.Globals["RegisterCoroutine"] = (Action<DynValue, string, bool>)RegisterCoroutine;
            Lua.Globals["RemoveHook"] = (Action<string>)RemoveHook;
            GlobalScriptBindings.Initialize(Lua);
            //GlobalScriptBindings.InitializeYieldables(Lua);
        }

        public UserScriptRunner(ScriptBindings bindings = null)
        {
            Lua = new Script(CoreModules.Preset_HardSandbox | CoreModules.Coroutine | CoreModules.OS_Time);
            Lua.Globals["RegisterHook"] = (Action<DynValue, string>)RegisterHook;
            Lua.Globals["RegisterCoroutine"] = (Action<DynValue, string, bool>)RegisterCoroutine;
            Lua.Globals["RemoveHook"] = (Action<string>)RemoveHook;
            GlobalScriptBindings.Initialize(Lua);
            bindings?.Initialize(Lua);
        }

        public void AddBindings(ScriptBindings bindings)
        {
            bindings.Initialize(Lua);
        }

        #region Hooks
        public void LoadScript(string scriptString)
        {
            scriptContainer?.ResetHooks();
            scriptContainer = new HookedScriptContainer();
            Lua.DoString(scriptString);
        }

        void RegisterCoroutine(DynValue del, string name, bool autoReset)
        {
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

        public virtual T Execute(string hook, params object[] args)
        {
            throw new NotImplementedException();
        }
    }


    public abstract class UserScriptRunner : ScriptRunnerBase
    {

        protected HookedScriptContainer scriptContainer = null;

        public UserScriptRunner()
        {
            Lua = new Script(CoreModules.Preset_HardSandbox | CoreModules.Coroutine | CoreModules.OS_Time);
            Lua.Globals["RegisterHook"] = (Action<DynValue, string>)RegisterHook;
            Lua.Globals["RegisterCoroutine"] = (Action<DynValue, string>)RegisterCoroutine;
            Lua.Globals["RemoveHook"] = (Action<string>)RemoveHook;
            GlobalScriptBindings.Initialize(Lua);
        }

        public UserScriptRunner(ScriptBindings bindings = null)
        {
            Lua = new Script(CoreModules.Preset_HardSandbox | CoreModules.Coroutine | CoreModules.OS_Time);
            Lua.Globals["RegisterHook"] = (Action<DynValue, string>)RegisterHook;
            Lua.Globals["RegisterCoroutine"] = (Action<DynValue, string>)RegisterCoroutine;
            Lua.Globals["RemoveHook"] = (Action<string>)RemoveHook;
            GlobalScriptBindings.Initialize(Lua);
            bindings?.Initialize(Lua);
        }

        public void AddBindings(ScriptBindings bindings)
        {
            bindings.Initialize(Lua);
        }

        #region Hooks
        public void LoadScript(string scriptString)
        {
            scriptContainer?.ResetHooks();
            scriptContainer = new HookedScriptContainer();
            Lua.DoString(scriptString);
        }

        void RegisterCoroutine(DynValue del, string name)
        {
            scriptContainer.SetHook(name, new ScriptFunction(Lua, del, true));
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

        public virtual void Execute(string hook, params object[] args)
        {
            throw new NotImplementedException();
        }
    }
}
