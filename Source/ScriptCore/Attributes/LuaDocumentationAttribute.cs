

namespace ScriptCore.Attributes
{
    using System;
    [System.AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class LuaDocumentationAttribute : Attribute
    {
        readonly string data;

        public string Data
        {
            get { return data; }
        }
        public LuaDocumentationAttribute(string data)
        {
            this.data = data;
        }
    }
}
