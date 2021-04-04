using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScriptCore;
namespace LuaGUI
{
    public static class CSharpClass
    {
        [LuaDocumentation("Peeks bytes, demonstration function")]
        [LuaFunction("PeekByte")]
        public static byte PeekByte(string domain, long address)
        {
            Console.WriteLine("(From C# Method) " + domain);
            return (byte)address;
        }
        public static short PeekShort(string domain, long address)
        {
            Console.WriteLine("From C# " + domain);
            return (short)address;
        }

    }
}
