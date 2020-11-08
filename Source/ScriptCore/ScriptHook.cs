namespace ScriptCore
{
    using MoonSharp.Interpreter;
    using ScriptCore.Yielders;

    /// <summary>
    /// Can be used for caching run strings for performance? if performance gain is negligible, just replace with longs
    /// </summary>
    internal class ScriptHook
    {
        public Yielder CurYielder { get; set; } = null;
        public DynValue LuaFunc { get; private set; }
        public ScriptHook(DynValue del)
        {
            LuaFunc = del;
        }

        ~ScriptHook()
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
