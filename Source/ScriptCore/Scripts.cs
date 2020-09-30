using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCore
{
    public static class Scripts
    {

        public static Script Create(string script)
        {
            Script scr = new Script(script);
            ScriptInitializer.Initialize(scr);
            scr.Initialize();
            return scr;
        }

    }


}
