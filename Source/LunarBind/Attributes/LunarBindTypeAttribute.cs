namespace LunarBind
{
    //TODO: Implement
    using System;

    [System.AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
    internal class LunarBindTypeAttribute : Attribute
    {
        readonly string name;
        readonly bool newable;
        readonly bool staticAccess;
        public string Name => name;
        public bool Newable => newable;
        public bool PrivateMemberAccess => staticAccess;
        public LunarBindTypeAttribute(string name, bool newable = false, bool usePrivateMembers = false)
        {
            this.name = name;
            this.newable = newable;
            this.staticAccess = usePrivateMembers;
        }

        public LunarBindTypeAttribute(bool newable, bool staticAccess, string name = null)
        {
            this.name = name;
            this.newable = newable;
            this.staticAccess = staticAccess;
        }
    }


}
