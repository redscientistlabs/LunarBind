namespace ScriptCore.Attributes
{
    using System;
    [System.AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class LuaFunctionAttribute : Attribute
    {
        readonly string name;

        public string Name
        {
            get { return name; }
        }
        public LuaFunctionAttribute(string name = null)
        {
            this.name = name;
        }
    }
}
