namespace ScriptCore.Attributes
{
    using System;
    [System.AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class LuaCallbackAttribute : Attribute
    {
        readonly string name;

        public string Name
        {
            get { return name; }
        }
        public LuaCallbackAttribute(string name)
        {
            this.name = name;
        }
    }
}
