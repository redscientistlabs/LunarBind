using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCore.Yielders
{
    [MoonSharpUserData]
    internal abstract class Yielder
    {
        /// <summary>
        /// Returns the state of the yielder
        /// </summary>
        /// <returns></returns>
        public abstract bool CheckStatus();
    }
}
