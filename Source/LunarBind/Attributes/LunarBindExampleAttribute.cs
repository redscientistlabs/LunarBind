namespace LunarBind
{
    using System;
    [System.AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class LunarBindExampleAttribute : Attribute
    {
        readonly string data;

        public string Data
        {
            get { return data; }
        }
        public LunarBindExampleAttribute(string data)
        {
            this.data = data;
        }
    }
}
