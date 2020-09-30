using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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
        public Form1()
        {
            InitializeComponent();

            //ADD ASSEMBLIES NEEDED
            ScriptInitializer.AddAssemblyAndNamespaces(GetType().Assembly.FullName, nameof(LuaGUI));
        }

        private void bTest_Click(object sender, EventArgs e)
        {
            string scriptString = @"
            Execute = coroutine.create(function ()
                local tempvar0 = 10
                print('(lua) exec0, yield 1 frame')
                waitFrames(1)

                print('(lua) exec1, yield 3 frames')
                waitFrames(3)

                print('(lua) exec2, final')
            end)
            

            PostExecute = coroutine.create(function ()
                print('(lua) postexec0, yield 2 frames')
                waitFrames(2)

                print('(lua) postexec1, yield 1 frame')
                waitFrames(1)

                print('(lua) postexec2, yield 1 frame')

                waitFrames(1)
                print('(lua) calling static C# method in post execute, final')
                print('(lua) return val from C#', CSharpClass.PeekByte('SRAM', 123))
            end)

            ";

            Script script = Scripts.Create(scriptString);
            Console.WriteLine("Script created");

            Console.WriteLine("(C#) Execute 0");
            script.Execute();

            for (int j = 0; j < 5; j++)
            {
                Console.WriteLine($"(C#) PostExecute {j}");
                script.PostExecute();
            }

            Console.WriteLine("(C#) Execute 1");
            script.Execute();
            Console.WriteLine("(C#) Execute 2");
            script.Execute();
            Console.WriteLine("(C#) Execute 3");
            script.Execute();
            Console.WriteLine("(C#) Execute 4");
            script.Execute();

            script.Dispose();
        }
    }
}
