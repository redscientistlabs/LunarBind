﻿

namespace LunarBind
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using MoonSharp.Interpreter;
    internal abstract class BindItem
    {
        public string Name { get; internal protected set; }
        public string YieldableString { get; internal protected set; }
        internal abstract void AddToScript(Script script);
    }
}