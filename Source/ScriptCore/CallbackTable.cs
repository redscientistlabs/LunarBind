namespace ScriptCore
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using MoonSharp.Interpreter;
    using System.Reflection;
    internal class CallbackTable : CallbackItem
    {
        //internal string
        internal Dictionary<string, CallbackTable> callbackTables = new Dictionary<string, CallbackTable>();
        internal Dictionary<string, CallbackFunc> callbackFunctions = new Dictionary<string, CallbackFunc>();
        
        public CallbackTable(string name)
        {
            this.Name = name;
        }

        //TODO: implement fields
        //public void AddField(string name, FieldInfo info)
        //{
        //}

        public void GenerateYieldableString()
        {
            StringBuilder sb = new StringBuilder();
            GenYieldableString(sb);
            YieldableString = sb.ToString();

            //if (f.IsYieldable) { sb.AppendLine(f.YieldableString); }
        }

        private void GenYieldableString(StringBuilder sb)
        {
            foreach (var f in callbackFunctions.Values)
            {
                if (f.IsYieldable)
                {
                    sb.Append(f.YieldableString);
                }
            }
            foreach (var t in callbackTables.Values)
            {
                t.GenYieldableString(sb);
            }
        }

        internal void AddCallbackFunc(string[] path, int index, CallbackFunc func)
        {
            if (index + 1 >= path.Length)
            {
                //At lowest level, add callback func
                if (callbackTables.ContainsKey(path[index]))
                {
                    throw new Exception($"Cannot add {string.Join(".",path)} ({func.Name}), a Table with that key exists");
                }
                else if (callbackFunctions.ContainsKey(path[index]))
                {
                    throw new Exception($"Cannot add {string.Join(".", path)} ({func.Name}), a Function with that key exists");
                }
                callbackFunctions[path[index]] = func;
            }
            else
            {

                if (callbackFunctions.ContainsKey(path[index]))
                {
                    throw new Exception($"Cannot add {string.Join(".", path)} ({func.Name}), a Function with the key ({path[index]}) exists in the path");
                }

                CallbackTable nextTable;
                if (callbackTables.TryGetValue(path[index], out nextTable))
                {
                    nextTable.AddCallbackFunc(path, index + 1, func);
                }
                else
                {
                    nextTable = new CallbackTable(path[index]);
                    callbackTables.Add(path[index], nextTable);
                    nextTable.AddCallbackFunc(path, index + 1, func);
                }
            }
        }

        private Table GenerateTable(Script script)
        {
            Table table = new Table(script);

            //Tables
            foreach (var t in callbackTables.Values)
            {
                table[t.Name] = t.GenerateTable(script);
            }
            //Names
            foreach (var f in callbackFunctions.Values)
            {
                table[f.Name] = DynValue.FromObject(script, f.Callback);
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
