﻿using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCore
{
    public static class ScriptLibrary
    {
        static Dictionary<string, string> scripts = new Dictionary<string, string>();
        public static string GetScript(string name)
        {
            if(scripts.TryGetValue(name, out string value))
            {
                return value;
            }
            else
            {
                return null;
            }
        }


        public static void LoadScriptsFromDir(string dir, string searchPattern = "*.txt")
        {
            if (Directory.Exists(dir))
            {
                var files = Directory.GetFiles(dir, searchPattern);
                foreach (var fileName in files)
                {
                    var text = File.ReadAllText(fileName);
                    scripts.Add(Path.GetFileNameWithoutExtension(fileName), text);
                }
            }
        }
        public static void InitializeForUnity(params (string name, string text)[] assets)
        {
            foreach (var ta in assets)
            {
                scripts.Add(ta.name, ta.text);
            }
            Script.DefaultOptions.ScriptLoader = new MoonSharp.Interpreter.Loaders.UnityAssetsScriptLoader(scripts);
        }
    }
}