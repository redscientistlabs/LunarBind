namespace ScriptCore
{
    using MoonSharp.Interpreter;
    using System;
    internal class CallbackFunc : CallbackItem
    {
        //public override string Name { get; private set; }
        public Delegate Callback { get; private set; } 
        public string Example { get; private set; }
        public string Documentation { get; private set; }

        public bool IsYieldable { get; private set; }

        public int NumParams => Callback.Method.GetParameters().Length;

        const string COROUTINE_YIELD_ = "COROUTINE_YIELD_";

        public CallbackFunc(string name, Delegate callback, string documentation = "", string example = "")
        {
            this.Callback = callback;
            this.Documentation = documentation;
            this.Example = example;
            IsYieldable = typeof(Yielder).IsAssignableFrom(callback.Method.ReturnType);
            if (IsYieldable && GlobalScriptBindings.AutoYield) { Name = COROUTINE_YIELD_ + name; }
            else { Name = name; }
            YieldableString = "";
        }

        public void GenerateYieldableString(string path = null)
        {
            string argString = "";

            string[] pathSplit = path.Split('.');
            pathSplit[pathSplit.Length - 1] = Name;
            string adjustedPath = string.Join(".", pathSplit);

            int len = Callback.Method.GetParameters().Length;
            for (int j = 0; j < len; j++)
            {
                if (j == 0)
                {
                    argString += "a0";
                }
                else
                {
                    argString += $",a{j}";
                }
            }

            YieldableString = $"{path} = function({argString}) return coroutine.yield({adjustedPath}({argString})) end \r\n";

            //if (path == null)
            //{
            //    YieldableString = $"{path} = function({argString}) return coroutine.yield(COROUTINE_YIELD_{adjustedPath}({argString})) end";
            //}
            //else
            //{
            //    YieldableString = $"{path} = function({argString}) return coroutine.yield(COROUTINE_YIELD_{Name}({argString})) end";
            //}

            //Name = "COROUTINE_YIELD_" + Name;
        }

        //Do not call from CallbackTable
        internal override void AddToScript(Script script)
        {
            script.Globals[Name] = Callback;
            if (IsYieldable && GlobalScriptBindings.AutoYield)
            {
                script.DoString(YieldableString);
            }
        }
    }
}
