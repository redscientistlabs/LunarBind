namespace LunarBind
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using MoonSharp.Interpreter;

    //TODO: implement name set in base constructor
    internal abstract class BindItem
    {
        public string Name { get; internal protected set; }
        //Todo: limit to table and function
        public string YieldableString { get; internal protected set; }
        internal abstract void AddToScript(Script script);
    }
}
