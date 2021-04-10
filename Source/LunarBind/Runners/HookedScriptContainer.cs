﻿namespace LunarBind
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Holds a <see cref="MoonSharp.Interpreter.Script"/> with lua functions
    /// </summary>
    public class HookedScriptContainer
    {
        public Dictionary<string, ScriptFunction> ScriptFunctions = new Dictionary<string, ScriptFunction>();
        /// <summary>
        /// Currently not in use!
        /// </summary>
        public Dictionary<string, object> ScriptData = new Dictionary<string, object>();

        public HookedScriptContainer()
        {
        }

        public void ResetHooks()
        {
            ScriptFunctions.Clear();
        }

        public ScriptFunction GetHook(string name)
        {
            if(ScriptFunctions.TryGetValue(name, out ScriptFunction value))
            {
                return value;
            }
            else
            {
                return null;
            }
        }

        public void SetHook(string name, ScriptFunction scriptHook)
        {
            ScriptFunctions[name] = scriptHook;
        }

        public bool RemoveHook(string name)
        {
            return ScriptFunctions.Remove(name);
        }
    }
}
