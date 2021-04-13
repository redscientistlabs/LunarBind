namespace LunarBind
{

    //TODO: Implement
    using System;
    [System.AttributeUsage(AttributeTargets.Enum, Inherited = false, AllowMultiple = false)]
    internal sealed class LunarBindEnumAttribute : Attribute
    {
        readonly string name;
        public string Name => name;
        public LunarBindEnumAttribute(string name = null)
        {
            this.name = name;
        }
    }
}
