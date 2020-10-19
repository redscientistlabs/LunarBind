namespace ScriptCore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Can be used for caching run strings for performance? if performance gain is negligible, just replace with longs
    /// </summary>
    internal class ScriptCoroutineInfo
    {
        public long FramesToResume { get; set; } = 0;
        public bool Finished { get; set; } = false;

        public bool Enabled { get; set; } = false;
        public ScriptCoroutineInfo()
        {
        }
    }
}
