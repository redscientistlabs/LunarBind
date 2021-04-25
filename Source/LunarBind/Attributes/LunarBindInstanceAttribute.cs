namespace LunarBind
{
    using System;

    /// <summary>
    /// Use this attribute to mark classes for instantiation
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class LunarBindInstanceAttribute : Attribute
    {
        readonly string name;
        readonly bool usePrivateMembers;
        public string Name => name;
        public bool PrivateMemberAccess => usePrivateMembers;

        /// <summary>
        /// The path this class will be instantiated at. Classes marked by this will be instantiated with their parameterless constructor.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="usePrivateMembers"></param>
        public LunarBindInstanceAttribute(string name = null, bool usePrivateMembers = false)
        {
            this.name = name;
            this.usePrivateMembers = usePrivateMembers;
        }
    }


}
