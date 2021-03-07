namespace ScriptCore.Yielding
{
    using MoonSharp.Interpreter;
    using ScriptCore.Attributes;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class Yielders
    {
        static Dictionary<string, Type> allYielders = new Dictionary<string, Type>();

        public static void RegisterYielder<T>(string name = null) where T : Yielder
        {
            if(name == null)
            {
                name = typeof(T).Name;
            }
            //GlobalScriptBindings.RegisterUserDataType(typeof(T));
            GlobalScriptBindings.RegisterYieldableType(name, typeof(T));
            allYielders[name] = typeof(T);
        }

        internal static void Initialize()
        {
            RegisterYielder<WaitFrames>(ReservedGlobals.WaitForFrames);
        }

        //[LuaFunction(nameof(WaitForFrames))]
        //public static WaitFrames WaitForFrames(long frames)
        //{
        //    return new WaitFrames(frames);
        //}

    }
}
