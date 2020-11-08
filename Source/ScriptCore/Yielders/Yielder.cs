namespace ScriptCore.Yielders
{
    using MoonSharp.Interpreter;

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
