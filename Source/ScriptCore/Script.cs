using NLua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCore
{
    public class Script : IDisposable
    {
        const int PRE = 0;
        const int EXEC = 1;
        const int POST = 2;

        internal Lua lua;
        string hookableScript;
        
        //public bool Running { get; set; } = false;

        
        long[] yieldTimers = new long[] { 0, 0, 0 };

        bool[] hooks = new bool[] { false, false, false };



        //To satisfy Ceras serialization later
        public Script() { }

        internal Script(string script)
        {
            this.hookableScript = script;
            lua = new Lua();
            lua.State.Encoding = Encoding.UTF8;
            lua.LoadCLRPackage();
        }


        public void Initialize()
        {
            lua.DoString(hookableScript);
            hooks[PRE] =  (bool)lua.DoString("return PreExecute ~= nil")[0];
            hooks[EXEC] = (bool)lua.DoString("return Execute ~= nil")[0];
            hooks[POST] = (bool)lua.DoString("return PostExecute ~= nil")[0];               
        }

        private void RunLua(int timerIndex, string luaStr)
        {
            bool exec = false;

            if (yieldTimers[timerIndex] > 0)
            {
                exec = (--yieldTimers[timerIndex] == 0);
            }
            else
            {
                exec = true;
            }

            if (exec)
            {
                lua.DoString(luaStr);
                object yt = lua[ScriptConstants.LUA_YIELD];
                if (yt != null)
                {
                    yieldTimers[timerIndex] = Convert.ToInt64(yt);
                }
                lua[ScriptConstants.LUA_YIELD] = null;
            }
        }

        public void PreExecute()
        {
            if (hooks[PRE])
            {
                RunLua(PRE, "coroutine.resume(PreExecute)");
            }
        }

        public void Execute()
        {
            if (hooks[EXEC])
            {
                RunLua(EXEC, "coroutine.resume(Execute)");
            }
        }

        public void PostExecute()
        {
            if (hooks[POST])
            {
                RunLua(POST, "coroutine.resume(PostExecute)");
            }
        }

        public void Dispose()
        {
            lua.Dispose();
        }
    }
}
