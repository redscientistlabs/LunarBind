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

        public Dictionary<string, ScriptHook> Hooks = new Dictionary<string, ScriptHook>();
        
        //public string Guid { get; private set; }

        public HookedScriptContainer(string script)
        {
            ScriptString = script;
            //Guid = System.Guid.NewGuid().ToString();
        }

        public void ResetHooks()
        {
            Hooks.Clear();
        }

        public ScriptHook GetHook(string name)
        {
            if(Hooks.TryGetValue(name, out ScriptHook value))
            {
                return value;
            }
            else
            {
                return null;
            }
        }

        public void AddHook(string name, ScriptHook scriptHook)
        {
            Hooks[name] = scriptHook;
        }

        public bool RemoveHook(string name)
        {
            return Hooks.Remove(name);
        }
    }
}
