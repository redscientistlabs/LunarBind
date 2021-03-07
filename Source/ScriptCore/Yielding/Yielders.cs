namespace ScriptCore
{
    using ScriptCore.Yielding;
    using System;
    using System.Collections.Generic;

    public static class Yielders
    {
        static Dictionary<string, Type> allYielders = new Dictionary<string, Type>();

        public static void RegisterYielder<T>(string name = null) where T : Yielder
        {
            if(name == null)
            {
                name = typeof(T).Name;
            }
            GlobalScriptBindings.RegisterYieldableType(name, typeof(T));
            allYielders[name] = typeof(T);
        }

        internal static void Initialize()
        {
            RegisterYielder<WaitFrames>(ReservedGlobals.WaitForFrames);
        }

    }
}
