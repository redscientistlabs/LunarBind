namespace ScriptCore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using MoonSharp.Interpreter;
    using ScriptCore.Yielders;

    /// <summary>
    /// Can be used for caching run strings for performance? if performance gain is negligible, just replace with longs
    /// </summary>
    internal class NamedScriptHook
    {
        //TODO: remove?
        //public string Name { get; private set; } = "";
        public Yielder CurYielder { get; set; } = null;
        public DynValue LuaFunc { get; private set; }
        public NamedScriptHook(DynValue del)
        {
            LuaFunc = del;
        }

        ~NamedScriptHook()
        {
            CurYielder = null;
            LuaFunc = null;
        }

        public bool CheckYieldStatus()
        {
            if(CurYielder != null)
            {
                if (CurYielder.CheckStatus())
                {
                    CurYielder = null;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        public void SetYielder(Yielder yielder)
        {
            CurYielder = yielder;
        }

    }

}
