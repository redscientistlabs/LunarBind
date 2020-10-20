namespace ScriptCore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// A script to run in stepactions
    /// </summary>
    internal class ScriptContainer
    {
        public string ScriptString { get; protected set; }

        internal Dictionary<string, NamedScriptHook> Hooks = new Dictionary<string, NamedScriptHook>();
        
        public ScriptContainer(string script)
        {
            ScriptString = script;
        }

        public void ResetHooks()
        {
            Hooks.Clear();
        }

        public NamedScriptHook GetHook(string name)
        {
            if(Hooks.TryGetValue(name, out NamedScriptHook value))
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
