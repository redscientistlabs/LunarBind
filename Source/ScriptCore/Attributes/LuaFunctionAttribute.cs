namespace ScriptCore
{
    using System;
    [System.AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class LuaFunctionAttribute : Attribute
    {
        //Todo: Add AutoYield functionality

        readonly string name;
        //readonly bool autoYield;

        public string Name => name;

        //public bool AutoYield => autoYield;

        public LuaFunctionAttribute(string name = null/*, bool autoYield = true*/)
        {
            this.name = name;
            //this.autoYield = autoYield;
        }
    }
}
