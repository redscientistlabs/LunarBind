namespace ScriptCore
{
    using System.Collections.Generic;

    /// <summary>
    /// Holds a script to run in stepactions
    /// </summary>
    internal class ScriptContainer
    {
        public string ScriptString { get; protected set; }

        internal Dictionary<string, ScriptHook> Hooks = new Dictionary<string, ScriptHook>();
        
        public ScriptContainer(string script)
        {
            ScriptString = script;
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


    }
}
