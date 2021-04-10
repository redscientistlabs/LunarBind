namespace LunarBind
{
    using System;
    [System.AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class LunarBindDocumentationAttribute : Attribute
    {
        readonly string data;

        public string Data
        {
            get { return data; }
        }
        public LunarBindDocumentationAttribute(string data)
        {
            this.data = data;
        }
    }
}
