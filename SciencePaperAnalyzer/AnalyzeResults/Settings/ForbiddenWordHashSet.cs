using System;
using System.Collections.Generic;
using System.Text;

namespace AnalyzeResults.Settings
{
    public class ForbiddenWordHashSet
    {
        public string Name { get; set; }

        public HashSet<string> Words { get; set; }

        public HashSet<string> Errors { get; set; }
    }
}
