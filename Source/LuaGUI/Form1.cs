using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ScriptCore;
namespace LuaGUI
{
    public partial class Form1 : Form
    {
        Script testScript = null;

        public Form1()
        {
            InitializeComponent();

            //ADD FULL NAMESPACES
            //ScriptInitializer.AddAssemblyAndNamespaces(GetType().Assembly.FullName, nameof(LuaGUI));
            //ADD INDIVIDUAL FUNCTIONS 
            ScriptInitializer.RegisterFunc("printA", null, GetType().GetMethod("PrintPlusA", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public));
            ScriptInitializer.RegisterFunc("peekByte", null, typeof(CSharpClass).GetMethod("PeekByte", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public));
            //TODO: ADD TYPES
        }

        public static void PrintPlusA(string s)
        {
            Console.WriteLine("(A) " + s);
        }

        private void bTest_Click(object sender, EventArgs e)
        {
           

            Script script = Scripts.Create(tbScript.Text);
            Console.WriteLine("Script created");

            Stopwatch w = new Stopwatch();
            w.Start();
            for (int j = 0; j < 60; j++)
            {
                Console.WriteLine($"LOOP {j} ============================");
                Console.WriteLine($"(C#) PreExecute {j}");
                script.PreExecute();
                Console.WriteLine($"\r\n(C#) Execute {j}");
                script.Execute();
                Console.WriteLine($"\r\n(C#) PostExecute {j}");
                script.PostExecute();
            }
            Console.WriteLine(w.Elapsed);
            w.Stop();

            script.Dispose();
        }

        private void bStart_Click(object sender, EventArgs e)
        {
            testScript?.Dispose();
            testScript = Scripts.Create(tbScript.Text);
            bExecute.Enabled = true;
            bDispose.Enabled = true;
            bAbort.Enabled = true;

            bStart.Enabled = false;
        }

        private void bExecute_Click(object sender, EventArgs e)
        {
            try
            {
                Console.WriteLine($"LOOP ============================");
                Console.WriteLine($"(C#) PreExecute");
                testScript.PreExecute();
                Console.WriteLine($"\r\n(C#) Execute");
                testScript.Execute();
                Console.WriteLine($"\r\n(C#) PostExecute");
                testScript.PostExecute();
            }
            catch 
            {
                Console.WriteLine("ERROR: Script was aborted!");
                bDispose_Click(null, null);
            }
        }

        private void bDispose_Click(object sender, EventArgs e)
        {
            testScript.Dispose();
            testScript = null;
            bExecute.Enabled = false;
            bDispose.Enabled = false;
            bAbort.Enabled = false;
            bStart.Enabled = true;
        }

        private void bAbort_Click(object sender, EventArgs e)
        {
            testScript.Abort();
        }
    }
}
