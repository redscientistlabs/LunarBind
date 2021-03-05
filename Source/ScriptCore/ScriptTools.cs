using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCore
{
    public static class ScriptTools
    {
        public static DynValue CreateLuaObj(object o)
        {
            return UserData.Create(o);
        }
    }
}
