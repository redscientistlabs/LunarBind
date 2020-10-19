namespace ScriptCore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using NLua;

    public class ScriptRunner : IDisposable
    {
        private Script MainScript { get; set; } = null;
        private List<Script> StashkeyScripts = new List<Script>();
        private Script CurrentStashkeyScript = null;

        private Lua lua;

        private volatile bool Cancelled = false;

        public ScriptRunner()
        {
            lua = new Lua();
            lua.State.Encoding = Encoding.UTF8;
            lua.LoadCLRPackage();
            lua[ScriptConstants.LUA_YIELD] = 0.0;
            Initialize();
            //TODO: Better abort option?
            lua.SetDebugHook(KeraLua.LuaHookMask.Line, 0);
            lua.DebugHook += Lua_DebugHook;
        }

        public void SetCurrentScript(int script)
        {
            CurrentStashkeyScript = StashkeyScripts[script];
            if(CurrentStashkeyScript != null)
            {
                CurrentStashkeyScript.ResetInfos();
                //TODO: Sandbox
                lua.DoString(CurrentStashkeyScript.ScriptString);
            }
        }

        public void LoadScripts(string mainScript, params string[] stashkeyScripts)
        {
            try
            {
                if (mainScript != null)
                {
                    var scr = new Script(mainScript);
                    //TODO: Sandbox
                    lua.DoString(scr.ScriptString);

                    scr.PreExecuteInfo.Enabled = LuaFunctionExists("PreExecute");
                    scr.ExecuteInfo.Enabled = LuaFunctionExists("Execute");
                    scr.PostExecuteInfo.Enabled = LuaFunctionExists("PostExecute");
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
                            var scr = new Script(stashkeyScripts[i]);
                            //Todo: find a better way to decorate, this is janky
                            scr.Decorate(i.ToString(), "Execute");
                            //TODO: Sandbox
                            lua.DoString(scr.ScriptString);
                            scr.PreExecuteInfo.Enabled = LuaFunctionExists($"PreExecute{i}");
                            scr.ExecuteInfo.Enabled = LuaFunctionExists($"Execute{i}");
                            scr.PostExecuteInfo.Enabled = LuaFunctionExists($"PostExecute{i}");

                            StashkeyScripts.Add(scr);
                        }
                    }
                }
            }
            catch(NLua.Exceptions.LuaScriptException ex)
            {
                //Todo: exception handling
                Console.WriteLine(ex);
                throw ex;
            }
        }

        private bool LuaFunctionExists(string hook)
        {
            return (bool)lua.DoString($"return {hook} ~= nil")[0];
        }

        private void Lua_DebugHook(object sender, NLua.Event.DebugHookEventArgs e)
        {
            if (Cancelled)
            {
                lua.State.Error("User Cancelled");
            }
        }

        public void Initialize()
        {
            ScriptInitializer.Initialize(lua);
        }

        public void PreExecute()
        {
            if (!Cancelled)
            {
                if (MainScript != null)
                {
                    RunLua(MainScript.PreExecuteInfo, "PreExecute", "");
                }

                if (CurrentStashkeyScript != null)
                {
                    RunLua(CurrentStashkeyScript.PreExecuteInfo, "PreExecute", CurrentStashkeyScript.Decorator);
                }
            }
        }

        public void Execute()
        {
            if (!Cancelled)
            {
                if (MainScript != null)
                {
                    RunLua(MainScript.ExecuteInfo, "Execute", "");
                }

                if (CurrentStashkeyScript != null)
                {
                    RunLua(CurrentStashkeyScript.ExecuteInfo, "Execute", CurrentStashkeyScript.Decorator);
                }
            }
        }

        public void Abort()
        {
            Cancelled = true;
        }

        public void PostExecute()
        {
            if (!Cancelled)
            {
                if (MainScript != null)
                {
                    RunLua(MainScript.PostExecuteInfo, "PostExecute", "");
                }

                if (CurrentStashkeyScript != null)
                {
                    RunLua(CurrentStashkeyScript.PostExecuteInfo, "PostExecute", CurrentStashkeyScript.Decorator);
                }
            }
        }

        private void RunLua(ScriptCoroutineInfo info, string coroutineName, string decorator)
        {
            //Todo: cache decorator/whole run string in coroutine info to clean up this function
            if (info.Enabled && !info.Finished)
            {
                if (info.FramesToResume <= 0)
                {
                    lua.DoString($"coroutine.resume({coroutineName}{decorator})");
                    // info.FramesToResume = ((long)lua.GetObjectFromPath(ScriptConstants.LUA_YIELD));//Most efficient way I think
                    info.FramesToResume = lua.GetLong(ScriptConstants.LUA_YIELD);
                }
                else
                {
                    info.FramesToResume--;
                }
            }
        }

        public void Dispose()
        {
            lua.Close();
            lua.Dispose();
        }
    }
}
