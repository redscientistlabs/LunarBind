namespace ScriptCore
{
    using System;
    [System.AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class ScriptCoreExampleAttribute : Attribute
    {
        readonly string data;

        public string Data
        {
            get { return data; }
        }
        public ScriptCoreExampleAttribute(string data)
        {
            this.data = data;
        }
    }
}
