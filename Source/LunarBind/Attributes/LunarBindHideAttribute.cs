namespace LunarBind
{
    //TODO: Implement
    using System;
    /// <summary>
    /// Hide constructor from being added
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Constructor, Inherited = false, AllowMultiple = false)]
    internal sealed class LunarBindHideAttribute : Attribute
    {
        public LunarBindHideAttribute()
        {
        }
    }
}
