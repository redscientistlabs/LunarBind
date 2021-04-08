namespace ScriptCore
{
    using System;
    [System.AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class ScriptCoreFunctionAttribute : Attribute
    {
        //Todo: Add AutoYield functionality

        readonly string name;
        readonly bool autoYield;

        public string Name => name;

        public bool AutoYield => autoYield;

        public ScriptCoreFunctionAttribute(string name = null, bool autoYield = true)
        {
            this.name = name;
            this.autoYield = autoYield;
        }
    }
}
