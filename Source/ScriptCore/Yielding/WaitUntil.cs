namespace ScriptCore.Yielding
{
    using MoonSharp.Interpreter;
    using System;

    /// <summary>
    /// Waits X amount of calls
    /// </summary>
    [MoonSharpUserData]
    public class WaitUntil : Yielder
    {
        Func<bool> waiter;

        public WaitUntil(Func<bool> waiter)
        {
            this.waiter = waiter;
        }
        public override bool CheckStatus()
        {
            return waiter();
        }
    }
}
