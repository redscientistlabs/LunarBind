namespace ScriptCore
{
    using MoonSharp.Interpreter;
    using ScriptCore.Yielding;
    using System;

    public class ScriptHook
    {
        public Yielder CurYielder { get; set; } = null;
        public DynValue LuaFunc { get; private set; }
        public DynValue Coroutine { get; private set; }
        public bool IsCoroutine { get; private set; }

        public bool AutoResetCoroutine { get; private set; }
        
        public event Action Done;

        public ScriptHook(DynValue del, DynValue coroutine = null, bool autoResetCoroutine = false)
        {
            IsCoroutine = coroutine != null;
            LuaFunc = del;
            Coroutine = coroutine;
            AutoResetCoroutine = autoResetCoroutine;
        }

        ~ScriptHook()
        {
            CurYielder = null;
            LuaFunc = null;
            Coroutine = null;
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
        //public void SetCoroutine(DynValue coroutine)
        //{
            
        //    this.Coroutine = coroutine;
        //}

        public void OnDone()
        {
            Done?.Invoke();
        }

    }

}
