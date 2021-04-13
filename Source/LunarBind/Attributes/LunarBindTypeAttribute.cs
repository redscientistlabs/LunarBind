namespace LunarBind
{
    //TODO: Implement
    using System;

    [System.AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
    internal sealed class LunarBindTypeAttribute : Attribute
    {
        readonly string name;
        readonly bool newable;
        readonly bool staticAccess;
        public string Name => name;
        public bool Newable => newable;
        public bool StaticAccess => staticAccess;
        public LunarBindTypeAttribute(string name, bool newable = false, bool staticAccess = false)
        {
            this.name = name;
            this.newable = newable;
            this.staticAccess = staticAccess;
        }

        public LunarBindTypeAttribute(bool newable, bool staticAccess, string name = null)
        {
            this.name = name;
            this.newable = newable;
            this.staticAccess = staticAccess;
        }

    }
}
