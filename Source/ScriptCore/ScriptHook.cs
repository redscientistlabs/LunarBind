namespace ScriptCore
{
    using MoonSharp.Interpreter;
    using ScriptCore.Yielding;

    internal class ScriptHook
    {
        public Yielder CurYielder { get; set; } = null;
        public DynValue LuaFunc { get; private set; }
        public bool IsCoroutine { get; private set; }
        public bool IsCoroutineDead { get; set; } = false;
        public ScriptHook(DynValue del, bool isCoroutine = false)
        {
            IsCoroutine = isCoroutine;
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
