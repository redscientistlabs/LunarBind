using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Text;

namespace LunarBind
{

    /// <summary>
    /// Used for transporting a script reference without adding Script to userdata
    /// </summary>
    [MoonSharpUserData]
    public class ScriptReference
    {
        [MoonSharpHidden]
        public Script Script;
        [MoonSharpHidden]
        public ScriptReference(Script s)
        {
            Script = s;
        }

        public static implicit operator Script(ScriptReference r)
        {
            return r.Script;
        }
    }
}
