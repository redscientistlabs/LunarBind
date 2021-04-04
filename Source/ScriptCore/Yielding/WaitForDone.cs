
namespace ScriptCore.Yielding
{
    using MoonSharp.Interpreter;

    [MoonSharpUserData]
    public class WaitForDone : Yielder
    {
        public bool Done { get; set; } = false;

        public override bool CheckStatus()
        {
            return Done;
        }
    }
}
