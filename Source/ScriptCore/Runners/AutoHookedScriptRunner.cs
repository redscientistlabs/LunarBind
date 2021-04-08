namespace ScriptCore
{
    using MoonSharp.Interpreter;
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Automatically hooks all lua functions by name. Coroutines not supported
    /// </summary>
    public class AutoHookedScriptRunner : ScriptRunnerBase
    {
        //public Script Lua { get; private set; }

        private readonly HookedScriptContainer scriptContainer = new HookedScriptContainer();

        public AutoHookedScriptRunner()
        {
            Lua = new Script(CoreModules.Preset_HardSandbox | CoreModules.Coroutine | CoreModules.OS_Time);
            GlobalScriptBindings.Initialize(Lua);
        }

        public AutoHookedScriptRunner(ScriptBindings bindings) : this()
        {
            bindings.Initialize(Lua);
        }

        public AutoHookedScriptRunner(string script) 
        {
            Lua = new Script(CoreModules.Preset_HardSandbox | CoreModules.Coroutine | CoreModules.OS_Time);
            LoadScript(script);

            GlobalScriptBindings.Initialize(Lua);
        }

        public AutoHookedScriptRunner(string script, ScriptBindings bindings) : this()
        {
            bindings.Initialize(Lua);
            LoadScript(script);
        }

        public void LoadScript(string scriptString, string scriptName = "User Code")
        {
            scriptContainer.ResetHooks();
            scriptContainer.SetScript(scriptString);
            try
            {
                Lua.DoString(scriptContainer.ScriptString, null, scriptName);
            }
            catch (Exception ex)
            {
                if (ex is InterpreterException e)
                {
                    throw new Exception(e.DecoratedMessage);
                }

                throw ex;
            }
            AutoHook();
        }

        private void AutoHook()
        {
            var g = Lua.Globals;
            g.CollectDeadKeys();
            foreach (var key in g.Keys)
            {
                var item = g.Get(key);
                if(item.Type == DataType.Function)
                {
                    Console.WriteLine(key.ToString());
                    RegisterHook(item, key.String);
                }
            }
        }

        #region callbacks
        void RegisterHook(DynValue del, string name)
        {
            scriptContainer.Hooks[name] = new ScriptFunction(Lua,del,false);
        }
        #endregion

        public void Execute(string hookName, params object[] args)
        {
            try
            {
                RunLua(scriptContainer, hookName, args);
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
            catch (Exception ex)
            {
                if (ex is InterpreterException e)
                {
                    throw new Exception(e.DecoratedMessage);
                }

                throw ex;
            }
        }

        private DynValue RunLua(HookedScriptContainer script, string hookName, params object[] args)
        {
            var hook = script.GetHook(hookName);
            if (hook != null)
            {
                return Lua.Call(hook.LuaFunc, args);
            }
            else
            {
                return null;
            }
        }
    }
}
