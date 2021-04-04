namespace ScriptCore
{
    using MoonSharp.Interpreter;

    [MoonSharpUserData]
    public abstract class Yielder
    {
        /// <summary>
        /// Returns the state of the yielder. True means complete
        /// </summary>
        /// <returns></returns>
        public abstract bool CheckStatus();
    }
}
