namespace ScriptCore
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using ScriptCore.Standards;
    internal static class StandardHelpers
    {
        internal static LuaFuncType GetLuaFuncType(bool coroutine, bool autoreset)
        {
            if (!coroutine) return LuaFuncType.Function;
            else if (autoreset) return LuaFuncType.AutoCoroutine;
            else return LuaFuncType.SingleUseCoroutine;
        }

    }
}
