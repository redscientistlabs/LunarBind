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
    internal class StepScript
    {
        public string Original { get; protected set; }
        public string ScriptString { get; protected set; }
        public string Decorator { get; protected set; } = "";

        internal ScriptCoroutineInfo PreExecuteInfo = new ScriptCoroutineInfo();
        internal ScriptCoroutineInfo ExecuteInfo = new ScriptCoroutineInfo();
        internal ScriptCoroutineInfo PostExecuteInfo = new ScriptCoroutineInfo();


        public StepScript(string script)
        {
            Original = script;
            ScriptString = script;
        }

        public void ResetInfos()
        {
            PreExecuteInfo.Finished = false;
            PreExecuteInfo.FramesToResume = 0;

            ExecuteInfo.Finished = false;
            ExecuteInfo.FramesToResume = 0;

            PostExecuteInfo.Finished = false;
            PostExecuteInfo.FramesToResume = 0;
        }

        public void Clean()
        {
            ScriptString = Original;
        }

        public void Decorate(string decorator, params string[] decorateThese)
        {
            Decorator = decorator;

            if(decorator == "")
            {
                ScriptString = Original;
                return;
            }

            string decorated = Original;
            foreach (var str in decorateThese)
            {
                decorated = decorated.Replace(str, str + decorator);
            }
            ScriptString = decorated;
        }


    }
}
