namespace ScriptCore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using MoonSharp.Interpreter;
    internal static class BindingHelpers
    {
        //From https://stackoverflow.com/a/40579063
        //Creates a delegate from reflection info. Very helpful for binding
        public static Delegate CreateDelegate(MethodInfo methodInfo, object target = null)
        {
            Func<Type[], Type> getType;
            var isAction = methodInfo.ReturnType.Equals((typeof(void)));
            var types = methodInfo.GetParameters().Select(p => p.ParameterType);

            if (isAction)
            {
                getType = System.Linq.Expressions.Expression.GetActionType;
            }
            else
            {
                getType = System.Linq.Expressions.Expression.GetFuncType;
                types = types.Concat(new[] { methodInfo.ReturnType });
            }

            if (methodInfo.IsStatic)
            {
                return Delegate.CreateDelegate(getType(types.ToArray()), methodInfo);
            }

            return Delegate.CreateDelegate(getType(types.ToArray()), target, methodInfo.Name);
        }

        /// <summary>
        /// Automatically create tables, etc
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="pathString"></param>
        public static void CreateCallbackItem(Dictionary<string, CallbackItem> dict, string pathString, Delegate callback, string documentation, string example)
        {
            if (string.IsNullOrWhiteSpace(pathString))
            {
                throw new Exception($"Path cannot be null, empty, or whitespace for path [{pathString}] MethodInfo: ({callback.Method.Name})");
            }
            var path = pathString.Split('.');
            string root = path[0];
            CallbackFunc func = new CallbackFunc(path[path.Length-1], callback, documentation, example);
            if (func.IsYieldable) { func.GenerateYieldableString(pathString); }

            if (path.Length == 1)
            {
                //Simple global function
                dict[root] = func;
            }
            else
            {
                //Recursion time
                if(dict.TryGetValue(root, out CallbackItem item))
                {
                    if(item is CallbackTable t)
                    {
                        t.AddCallbackFunc(path, 1, func);
                        t.GenerateYieldableString(); //Bake the yieldable string
                    }
                    else
                    {
                        throw new Exception($"Cannot add {pathString} ({callback.Method.Name}), One or more members in the path is assigned to a function");
                    }
                }
                else
                {
                    //Create new
                    CallbackTable t = new CallbackTable(root);
                    dict[root] = t;
                    t.AddCallbackFunc(path, 1, func);
                    t.GenerateYieldableString(); //Bake the yieldable string
                }
            }
        }

        

    }

}
