namespace ScriptCore
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Holds a script with lua hooks
    /// </summary>
    public class HookedScriptContainer
    {
        public string ScriptString { get; protected set; }

        public Dictionary<string, ScriptFunction> Hooks = new Dictionary<string, ScriptFunction>();
        
        //public string Guid { get; private set; }

        public HookedScriptContainer(string script = "")
        {
            ScriptString = script;
            //Guid = System.Guid.NewGuid().ToString();
        }

        public void SetScript(string script)
        {
            ScriptString = script;
        }

        public void ResetHooks()
        {
            Hooks.Clear();
        }

        public ScriptFunction GetHook(string name)
        {
            if(Hooks.TryGetValue(name, out ScriptFunction value))
            {
                return value;
            }
            else
            {
                return null;
            }
        }

        public void AddHook(string name, ScriptFunction scriptHook)
        {
            Hooks[name] = scriptHook;
        }

        public bool RemoveHook(string name)
        {
            return Hooks.Remove(name);
        }
    }
}
