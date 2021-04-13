namespace LunarBind
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using MoonSharp.Interpreter;
    using System.Reflection;
    internal class BindTable : BindItem
    {
        //internal string
        private readonly Dictionary<string, BindTable> bindTables = new Dictionary<string, BindTable>();
        private readonly Dictionary<string, BindFunc> bindFunctions = new Dictionary<string, BindFunc>();
        private readonly Dictionary<string, BindEnum> bindEnums = new Dictionary<string, BindEnum>();
        //private readonly Dictionary<string, BindField> bindFields = new Dictionary<string, BindField>();

        public BindTable(string name)
        {
            this.Name = name;
        }

        //TODO: implement fields
        //public void AddField(string name, FieldInfo info)
        //{
        //}

        public void GenerateWrappedYieldString()
        {
            StringBuilder sb = new StringBuilder();
            GenYieldableStringRecursive(sb);
            YieldableString = sb.ToString();
        }

        private void GenYieldableStringRecursive(StringBuilder sb)
        {
            foreach (var f in bindFunctions.Values)
            {
                if (f.IsYieldable)
                {
                    sb.Append(f.YieldableString);
                }
            }
            foreach (var t in bindTables.Values)
            {
                t.GenYieldableStringRecursive(sb);
            }
        }

        internal void AddBindFunc(string[] path, int index, BindFunc func)
        {
            if (index + 1 >= path.Length)
            {
                //At lowest level, add callback func
                if (bindTables.ContainsKey(path[index]))
                {
                    throw new Exception($"Cannot add {string.Join(".",path)} ({func.Name}), a Table with that key already exists");
                }
                else if (bindFunctions.ContainsKey(path[index]))
                {
                    throw new Exception($"Cannot add {string.Join(".", path)} ({func.Name}), a Function with that key already exists");
                }
                else if (bindEnums.ContainsKey(path[index]))
                {
                    throw new Exception($"Cannot add {string.Join(".", path)} ({func.Name}), an Enum with that key already exists");
                }
                bindFunctions[path[index]] = func;
            }
            else
            {

                if (bindFunctions.ContainsKey(path[index]))
                {
                    throw new Exception($"Cannot add {string.Join(".", path)} ({func.Name}), a Function with the key ({path[index]}) exists in the path");
                }
                else if (bindEnums.ContainsKey(path[index]))
                {
                    throw new Exception($"Cannot add {string.Join(".", path)} ({func.Name}), an Enum with the key ({path[index]}) exists in the path");
                }

                BindTable nextTable;
                if (bindTables.TryGetValue(path[index], out nextTable))
                {
                    nextTable.AddBindFunc(path, index + 1, func);
                }
                else
                {
                    nextTable = new BindTable(path[index]);
                    bindTables.Add(path[index], nextTable);
                    nextTable.AddBindFunc(path, index + 1, func);
                }
            }
        }

        
        internal void AddBindEnum(string[] path, int index, BindEnum bindEnum)
        {
            if (index + 1 >= path.Length)
            {
                //At lowest level, add enum
                if (bindTables.ContainsKey(path[index]))
                {
                    throw new Exception($"Cannot add {string.Join(".", path)} ({bindEnum.Name}), a Table with that key already exists");
                }
                else if (bindFunctions.ContainsKey(path[index]))
                {
                    throw new Exception($"Cannot add {string.Join(".", path)} ({bindEnum.Name}), a Function with that key already exists");
                }
                else if (bindEnums.ContainsKey(path[index]))
                {
                    throw new Exception($"Cannot add {string.Join(".", path)} ({bindEnum.Name}), an Enum with that key already exists");
                }
                bindEnums[path[index]] = bindEnum;
            }
            else
            {
                if (bindFunctions.ContainsKey(path[index]))
                {
                    throw new Exception($"Cannot add {string.Join(".", path)} ({bindEnum.Name}), a Function with the key ({path[index]}) exists in the path");
                }
                else if (bindEnums.ContainsKey(path[index]))
                {
                    throw new Exception($"Cannot add {string.Join(".", path)} ({bindEnum.Name}), an Enum with the key ({path[index]}) exists in the path");
                }

                BindTable nextTable;
                if (bindTables.TryGetValue(path[index], out nextTable))
                {
                    nextTable.AddBindEnum(path, index + 1, bindEnum);
                }
                else
                {
                    //Create new table
                    nextTable = new BindTable(path[index]);
                    bindTables.Add(path[index], nextTable);
                    nextTable.AddBindEnum(path, index + 1, bindEnum);
                }
            }
        }

        private Table GenerateTable(Script script)
        {
            Table table = new Table(script);

            //Tables
            foreach (var t in bindTables.Values)
            {
                table[t.Name] = t.GenerateTable(script);
            }
            //Functions
            foreach (var f in bindFunctions.Values)
            {
                table[f.Name] = DynValue.FromObject(script, f.Callback);
            }
            //Enums
            foreach (var e in bindEnums.Values)
            {
                table[e.Name] = e.CreateEnumTable(script);
            }

            return table;
        }

        internal override void AddToScript(Script script)
        {
            script.Globals[Name] = GenerateTable(script);
            if (!string.IsNullOrWhiteSpace(YieldableString)) { script.DoString(YieldableString); }
        }
    }
}
