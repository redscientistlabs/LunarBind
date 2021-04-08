namespace ScriptCore
{
    using System;
    using System.Collections.Generic;
    using ScriptCore.Yielding;

    public static class Yielders
    {
        private static readonly Dictionary<string, Type> allYielders = new Dictionary<string, Type>();

        public static void RegisterYielder<T>(string name = null) where T : Yielder
        {
            if(name == null)
            {
                name = typeof(T).Name;
            }
            GlobalScriptBindings.RegisterYieldableType(name, typeof(T));
            allYielders[name] = typeof(T);
        }

        /// <summary>
        /// Register yielders for internal use
        /// </summary>
        internal static void Initialize()
        {
            RegisterYielder<WaitFrames>();
            //The others are not meant for use in lua, instead for C# functions
        }

    }
}
