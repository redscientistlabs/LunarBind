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
        //Indexer constants
        const int PRE = 0;
        const int EXEC = 1;
        const int POST = 2;


        internal Lua lua;
        string userScript;

        volatile bool abort = false;

        //public bool Running { get; set; } = false

        //Todo: maybe just separate these
        long[] yieldTimers = new long[] { 0, 0, 0 };
        bool[] hooks = new bool[] { false, false, false };

        
        //To satisfy Ceras serialization, but these scripts will likely be constructed on the emulator side only
        public Script() { }

        internal Script(string script)
        {
            this.userScript = script;
            lua = new Lua();
            lua.State.Encoding = Encoding.UTF8;
            lua.LoadCLRPackage();

            //TODO: Better abort option?
            lua.SetDebugHook(KeraLua.LuaHookMask.Line, 0);
            lua.DebugHook += Lua_DebugHook;
        }

        private void Lua_DebugHook(object sender, NLua.Event.DebugHookEventArgs e)
        {
            if (abort)
            {
                lua.State.Error("Execution manually aborted");
            }
        }

        public void Initialize()
        {
            lua.DoString(userScript);
            //Detect hooks, find better way?
            hooks[PRE]  = LuaFunctionExists("PreExecute");
            hooks[EXEC] = LuaFunctionExists("Execute");
            hooks[POST] = LuaFunctionExists("PostExecute");
        }

        public bool LuaFunctionExists(string hook)
        {
            return (bool)lua.DoString($"return {hook} ~= nil")[0];
        }

        private void RunLua(int timerIndex, string luaStr)
        {
            bool exec = (yieldTimers[timerIndex] > 0) ? (--yieldTimers[timerIndex] == 0) : true;

            //if (yieldTimers[timerIndex] > 0)
            //{
            //    exec = (--yieldTimers[timerIndex] == 0);
            //}
            //else
            //{
            //    exec = true;
            //}

            if (exec)
            {
                lua.DoString(luaStr, "User Code");
                object yt = lua[ScriptConstants.LUA_YIELD];
                if (yt != null)
                {
                    //Console.WriteLine(yt.GetType().ToString());
                    //TODO: decide if yielding 1 frame means skipping the next frame or waiting until the next frame
                    yieldTimers[timerIndex] = ((long)(double)yt)+1; 
                }
                lua[ScriptConstants.LUA_YIELD] = null;
            }
        }

        public void Abort()
        {
            abort = true;
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
