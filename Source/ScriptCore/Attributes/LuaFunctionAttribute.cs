namespace ScriptCore.Attributes
{
    using System;
    [System.AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class LuaFunctionAttribute : Attribute
    {
        readonly string name;

        public string Name
        {
            get { return name; }
        }
        public LuaFunctionAttribute(string name)
        {
            this.name = name;
        }
    }
}
