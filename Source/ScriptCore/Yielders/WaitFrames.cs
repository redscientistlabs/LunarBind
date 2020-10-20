using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCore.Yielders
{
    /// <summary>
    /// Waits X amount of calls
    /// </summary>
    [MoonSharpUserData]
    public class WaitFrames : Yielder
    {
        long framesLeft;

        public WaitFrames(long frames)
        {
            framesLeft = frames;
        }
        public override bool CheckStatus()
        {
            return (framesLeft-- <= 0);
        }
    }
}
