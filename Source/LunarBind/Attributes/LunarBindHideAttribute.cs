namespace LunarBind
{
    using System;

    /// <summary>
    /// Hide constructor from being added
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Constructor, Inherited = false, AllowMultiple = false)]
    public sealed class LunarBindHideAttribute : Attribute
    {
        public LunarBindHideAttribute()
        {
        }
    }
}
