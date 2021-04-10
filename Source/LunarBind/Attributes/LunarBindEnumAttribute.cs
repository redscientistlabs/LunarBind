namespace LunarBind
{
    using System;
    [System.AttributeUsage(AttributeTargets.Enum, Inherited = false, AllowMultiple = false)]
    public sealed class LunarBindEnumAttribute : Attribute
    {
        readonly string name;
        public string Name => name;
        public LunarBindEnumAttribute(string name = null)
        {

        }
    }
}
