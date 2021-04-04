namespace ScriptCore
{
    using MoonSharp.Interpreter;
    using System;
    internal struct CallbackFunc
    {
        public string Path { get; private set; }
        public Delegate Callback { get; private set; } 
        public string Example { get; private set; }
        public string Documentation { get; private set; }

        public bool IsYieldable { get; private set; }
        public string YieldableString { get; private set; }

        public int NumParams => Callback.Method.GetParameters().Length;
        public CallbackFunc(string path, Delegate callback, string documentation = "", string example = "")
        {

            this.Path = path;
            this.Callback = callback;
            this.Documentation = documentation;
            this.Example = example;
            IsYieldable = typeof(Yielding.Yielder).IsAssignableFrom(callback.Method.ReturnType);
            if (IsYieldable)
            {
                string argString = "";
                int len = Callback.Method.GetParameters().Length;
                for(int j = 0; j < len; j++)
                {
                    if(j == 0)
                    {
                        argString += "a0";
                    }
                    else
                    {
                        argString += $",a{j}";
                    }
                }
                YieldableString = $"function {Path}({argString}) coroutine.yield(COROUTINE_YIELD_{Path}({argString})) end";
                Path = "COROUTINE_YIELD_" + Path;

            }
            else
            {
                YieldableString = "";
            }
        }

        //public DynValue InternalCallback(ScriptExecutionContext context, CallbackArguments args)
        //{
            
        //    //DynValue.NewCallback(
        //    //    )
        //    //context.
        //}

    }
}
