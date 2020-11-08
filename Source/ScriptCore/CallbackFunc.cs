namespace ScriptCore
{
    using System;

    internal struct CallbackFunc
    {
        public string Path { get; private set; }
        public Delegate Callback { get; private set; } 
        public string Example { get; private set; }
        public string Documentation { get; private set; }

        public CallbackFunc(string path, Delegate callback, string documentation = "", string example = "")
        {
            this.Path = path;
            this.Callback = callback;
            this.Documentation = documentation;
            this.Example = example;
        }
    }
}
