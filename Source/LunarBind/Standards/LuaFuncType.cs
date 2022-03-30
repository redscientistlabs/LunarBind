namespace LunarBind.Standards
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    [System.Flags]
    public enum LuaFuncType
    {
        /// <summary>
        /// A non-coroutine function
        /// </summary>
        Function = 1,
        /// <summary>
        /// A coroutine which automatically restarts after completing
        /// </summary>
        AutoCoroutine = 2,
        /// <summary>
        /// An optimized coroutine which automatically restarts after completing
        /// </summary>
        OptimizedAutoCoroutine = 4,
        /// <summary>
        /// A coroutine which does not automatically restart after completing
        /// </summary>
        SingleUseCoroutine = 8,
        /// <summary>
        /// Allow User-Registered hooks, which must be coroutines.<para/> Must also provide either <see cref="AutoCoroutine"/> or <see cref="SingleUseCoroutine"/> for automatic hooking if you use this flag
        /// </summary>
        AllowAnyCoroutine = 16,
        
        /// <summary>
        /// Allow user-registered hooks of any type.<para/> Must provide another type besides <see cref="AllowAnyCoroutine"/> for automatic hooking if you use this flag.
        /// </summary>
        AllowAny = 32

    }
}
