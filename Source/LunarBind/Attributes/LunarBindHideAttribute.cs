namespace LunarBind
{
    //TODO: Implement
    using System;
    /// <summary>
    /// Hide from being added
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Struct | AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class LunarBindHideAttribute : Attribute
    {
        public LunarBindHideAttribute()
        {
        }
    }

    /// <summary>
    /// Hide class from being added through assembly
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class LunarBindIgnoreAssemblyAddAttribute : Attribute
    {
        public LunarBindIgnoreAssemblyAddAttribute()
        {
        }
    }

}
