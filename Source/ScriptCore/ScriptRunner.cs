namespace ScriptCore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using MoonSharp.Interpreter;
    using ScriptCore.Attributes;
    using ScriptCore.Yielders;

    public class ScriptRunner
    {
        private ScriptContainer MainScript { get; set; } = null;
        private List<ScriptContainer> StashkeyScripts = new List<ScriptContainer>();
        private ScriptContainer CurrentStashkeyScript = null;

        private volatile bool Cancelled = false;

        private Script lua;

        ScriptContainer registrationContext = null;


        public ScriptRunner()
        {
            lua = new Script(CoreModules.Preset_HardSandbox | CoreModules.Coroutine | CoreModules.OS_Time);
            lua.Globals[ScriptConstants.LUA_YIELD] = null;
            lua.Globals["RegisterHook"] = (Action<DynValue, string>)RegisterHook;
            UserData.RegisterType<Yielder>();
            UserData.RegisterType<WaitFrames>();
            ScriptInitializer.Initialize(lua);
        }

        public void SetCurrentScript(int script)
        {
            CurrentStashkeyScript = StashkeyScripts[script];
            if(CurrentStashkeyScript != null)
            {
                CurrentStashkeyScript.ResetHooks();

                registrationContext = CurrentStashkeyScript;
                lua.DoString(CurrentStashkeyScript.ScriptString);
                registrationContext = null;
            }
        }


        public void LoadScripts(string mainScript, params string[] stashkeyScripts)
        {
            try
            {
                if (mainScript != null)
                {
                    var scr = new ScriptContainer(mainScript);
                    registrationContext = scr;
                    try
                    {
                        lua.DoString(scr.ScriptString);
                    }
                    catch(ScriptRuntimeException ex)
                    {
                        throw ex;
                    }
                    registrationContext = null;
                    MainScript = scr;
                }

                if (stashkeyScripts != null && stashkeyScripts.Length > 0)
                {
                    for (int i = 0; i < stashkeyScripts.Length; i++)
                    {
                        if (stashkeyScripts[i] == null)
                        {
                            StashkeyScripts.Add(null);
                        }
                        else
                        {
                            var scr = new ScriptContainer(stashkeyScripts[i]);
                            registrationContext = scr;
                            lua.DoString(scr.ScriptString);
                            registrationContext = null;
                            StashkeyScripts.Add(scr);
                        }
                    }
                }
            }
            catch(ScriptRuntimeException ex)
            {
                //Todo: exception handling
                //Console.WriteLine(ex);
                throw ex;
            }
        }

        //[LuaDocumentation("Registers a function as a callback")]
        //[LuaCallback("RegisterHook")]
        void RegisterHook(DynValue del, string name)
        {
            //TODO: throw error?

            //var type = del.Type;
            if (registrationContext == null) { return; }
            registrationContext.Hooks.Add(name, new NamedScriptHook(del));
        }


        public void Execute(string hookName)
        {
            try
            {
                if (!Cancelled)
                {
                    if (MainScript != null)
                    {
                        RunLua(MainScript, hookName);
                    }

                    if (CurrentStashkeyScript != null)
                    {
                        RunLua(CurrentStashkeyScript, hookName);
                    }
                }
            }
            catch(ScriptRuntimeException ex)
            {
                throw ex;
            }
        }

        private void RunLua(ScriptContainer script, string hookName)
        {
            var hook = script.GetHook(hookName);
            if (hook != null && hook.CheckYieldStatus())
            {
                lua.Call(hook.LuaFunc);
                var yieldObj = lua.Globals[ScriptConstants.LUA_YIELD];


                //Gave me an error if I combined them
                if (yieldObj == null)
                {
                    hook.CurYielder = null;
                }
                if (yieldObj is Yielder yielder)
                {
                    hook.CurYielder = yielder;
                }
                else
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
