namespace ScriptCore
{
    using MoonSharp.Interpreter;
    using System;
    using System.Collections.Generic;
    using System.Text;
    public abstract class ScriptRunnerBase
    {
        public Script Lua { get; protected set; }
    }
}
