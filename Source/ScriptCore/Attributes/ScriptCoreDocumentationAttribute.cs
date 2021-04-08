namespace ScriptCore
{
    using System;
    [System.AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class ScriptCoreDocumentationAttribute : Attribute
    {
        readonly string data;

        public string Data
        {
            get { return data; }
        }
        public ScriptCoreDocumentationAttribute(string data)
        {
            this.data = data;
        }
    }
}
