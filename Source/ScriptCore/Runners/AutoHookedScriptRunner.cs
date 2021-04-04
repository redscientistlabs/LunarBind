namespace ScriptCore
{
    using MoonSharp.Interpreter;
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Automatically hooks functions in a script. Coroutines not supported
    /// </summary>
    public class AutoHookedScriptRunner
    {
        protected Script lua;

        private HookedScriptContainer scriptContainer = null;

        public AutoHookedScriptRunner()
        {
            lua = new Script(CoreModules.Preset_HardSandbox | CoreModules.Coroutine | CoreModules.OS_Time);
            //Global init
            GlobalScriptBindings.Initialize(lua);
            GlobalScriptBindings.InitializeYieldables(lua);
        }

        public AutoHookedScriptRunner(ScriptBindings bindings) : this()
        {
            bindings.Initialize(lua);
        }

        public AutoHookedScriptRunner(string script) : this()
        {
            LoadScript(script);
        }

        public AutoHookedScriptRunner(string script, ScriptBindings bindings) : this()
        {
            bindings.Initialize(lua);
            LoadScript(script);
        }

        public void LoadScript(string scriptString)
        {
            scriptContainer?.ResetHooks();
            scriptContainer = new HookedScriptContainer(scriptString);
            lua.DoString(scriptContainer.ScriptString);
            AutoHook();
        }

        private void AutoHook()
        {
            var g = lua.Globals;
            g.CollectDeadKeys();
            foreach (var key in g.Keys)
            {
                var item = g.Get(key);
                if(item.Type == DataType.Function)
                {
                    RegisterHook(item, key.String);
                }
            }
        }

        #region callbacks
        void RegisterHook(DynValue del, string name)
        {
            scriptContainer.Hooks[name] = new ScriptHook(del);
        }
        #endregion

        public void Execute(string hookName, params object[] args)
        {
            try
            {
                RunLua(scriptContainer, hookName, args);
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
                var ret = RunLua(scriptContainer, hookName, args);
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

        private DynValue RunLua(HookedScriptContainer script, string hookName, params object[] args)
        {
            var hook = script.GetHook(hookName);
            if (hook != null)
            {
                return lua.Call(hook.LuaFunc, args);
            }
            else
            {
                return null;
            }
        }
    }
}
