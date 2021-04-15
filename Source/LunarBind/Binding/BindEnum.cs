namespace LunarBind
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Linq;
    using MoonSharp.Interpreter;

    internal class BindEnum : BindItem
    {
        public string Name { get; private set; }
        private List<KeyValuePair<string, int>> enumVals = new List<KeyValuePair<string, int>>();
        public BindEnum(string name, Type e)
        {
            Name = name;
            var fields = e.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            foreach (var field in fields)
            {
                var attr = (LunarBindHideAttribute)Attribute.GetCustomAttribute(field, typeof(LunarBindHideAttribute));
                if(attr == null)
                {
                    enumVals.Add(new KeyValuePair<string, int>(field.Name, (int)field.GetValue(null)));
                }
            }
        }

        internal Table CreateEnumTable(Script script)
        {
            Table t = new Table(script);
            foreach (var item in enumVals)
            {
                t[item.Key] = item.Value;
            }
            return t;
        }

        internal override void AddToScript(Script script)
        {
            Table t = new Table(script);

            foreach (var item in enumVals)
            {
                t[item.Key] = item.Value;
            }

            script.Globals[Name] = t;
        }
    }

}
