namespace ScriptCore.Attributes
{
    using System;
    [System.AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class LuaExampleAttribute : Attribute
    {
        readonly string data;

        public string Data
        {
            get { return data; }
        }
        public LuaExampleAttribute(string data)
        {
            this.data = data;
        }
    }
}
