namespace LunarBind
{
    using MoonSharp.Interpreter;
    using System;
    using System.Collections.Generic;
    using System.Text;

    internal class BindUserObject : BindItem
    {
        public object UserObject { get; private set; }

        public BindUserObject(string name, object obj)
        {
            Name = name;
            UserObject = obj;
        }

        internal override void AddToScript(Script script)
        {
            script.Globals[Name] = UserObject;
        }
    }
}
