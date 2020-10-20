using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoonSharp.Interpreter;
using ScriptCore.Attributes;
namespace ScriptCore.Yielders
{
    internal static class Yielders
    {
        [LuaCallback(nameof(WaitForFrames))]
        public static WaitFrames WaitForFrames(long frames)
        {
            return new WaitFrames(frames);
        }

    }
}
