namespace ScriptCore.Runners
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
            Lua.Globals["RegisterCoroutine"] = (Action<DynValue, string>)RegisterCoroutine;
            Lua.Globals["RemoveHook"] = (Action<string>)RemoveHook;
            GlobalScriptBindings.Initialize(Lua);
            //GlobalScriptBindings.InitializeYieldables(Lua);
        }

        public UserScriptRunner(ScriptBindings bindings = null)
        {
            Lua = new Script(CoreModules.Preset_HardSandbox | CoreModules.Coroutine | CoreModules.OS_Time);
            Lua.Globals["RegisterHook"] = (Action<DynValue, string>)RegisterHook;
            Lua.Globals["RegisterCoroutine"] = (Action<DynValue, string>)RegisterCoroutine;
            Lua.Globals["RemoveHook"] = (Action<string>)RemoveHook;
            GlobalScriptBindings.Initialize(Lua);
            //GlobalScriptBindings.InitializeYieldables(Lua);
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
            scriptContainer = new HookedScriptContainer(scriptString);
            Lua.DoString(scriptContainer.ScriptString);
        }

        void RegisterCoroutine(DynValue del, string name)
        {
            var coroutine = Lua.CreateCoroutine(del);
            scriptContainer.AddHook(name, new ScriptHook(Lua, del, coroutine, true));
        }

        void RegisterHook(DynValue del, string name)
        {
            scriptContainer.AddHook(name, new ScriptHook(Lua, del));
        }

        void RemoveHook(string name)
        {
            //scriptContainer.Hooks.Remove(name);
            scriptContainer.RemoveHook(name);
        }
        #endregion

        public virtual T Execute(string hook, params object[] args)
        {
            throw new NotImplementedException();
        }
        //public abstract IEnumerator ExecuteCoroutine(string hook, params object[] args);
    }


    public abstract class UserScriptRunner : ScriptRunnerBase
    {
        //public Script Lua { get; private set; }

        protected HookedScriptContainer scriptContainer = null;

        public UserScriptRunner()
        {
            Lua = new Script(CoreModules.Preset_HardSandbox | CoreModules.Coroutine | CoreModules.OS_Time);
            Lua.Globals["RegisterHook"] = (Action<DynValue, string>)RegisterHook;
            Lua.Globals["RegisterCoroutine"] = (Action<DynValue, string>)RegisterCoroutine;
            Lua.Globals["RemoveHook"] = (Action<string>)RemoveHook;
            GlobalScriptBindings.Initialize(Lua);
            //GlobalScriptBindings.InitializeYieldables(Lua);
        }

        public UserScriptRunner(ScriptBindings bindings = null)
        {
            Lua = new Script(CoreModules.Preset_HardSandbox | CoreModules.Coroutine | CoreModules.OS_Time);
            Lua.Globals["RegisterHook"] = (Action<DynValue, string>)RegisterHook;
            Lua.Globals["RegisterCoroutine"] = (Action<DynValue, string>)RegisterCoroutine;
            Lua.Globals["RemoveHook"] = (Action<string>)RemoveHook;
            GlobalScriptBindings.Initialize(Lua);
            //GlobalScriptBindings.InitializeYieldables(Lua);
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
            scriptContainer = new HookedScriptContainer(scriptString);
            Lua.DoString(scriptContainer.ScriptString);
        }

        void RegisterCoroutine(DynValue del, string name)
        {
            var coroutine = Lua.CreateCoroutine(del);
            scriptContainer.AddHook(name, new ScriptHook(Lua, del, coroutine));
        }

        void RegisterHook(DynValue del, string name)
        {
            scriptContainer.AddHook(name, new ScriptHook(Lua, del));
        }

        void RemoveHook(string name)
        {
            //scriptContainer.Hooks.Remove(name);
            scriptContainer.RemoveHook(name);
        }
        #endregion

        public virtual void Execute(string hook, params object[] args)
        {
            throw new NotImplementedException();
        }
        //public abstract IEnumerator ExecuteCoroutine(string hook, params object[] args);
    }
}
