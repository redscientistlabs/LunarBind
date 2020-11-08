namespace ScriptCore.Yielders
{
    using ScriptCore.Attributes;
    internal static class Yielders
    {
        //Creating a C# object inside of MoonSharp is strange, using a callback as a workaround

        [LuaCallback(nameof(WaitForFrames))]
        public static WaitFrames WaitForFrames(long frames)
        {
            return new WaitFrames(frames);
        }

    }
}
