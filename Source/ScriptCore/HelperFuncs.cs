using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCore
{
    internal static class HelperFuncs
    {
        //Taken from https://stackoverflow.com/a/40579063
        //Creates a delegate from reflection info
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
    }
}
