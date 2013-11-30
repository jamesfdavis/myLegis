using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace myLegis.Spider
{

    [Serializable]
    public class Follow : iFollower {
        public Int32 depth { get; set; }
        public Regex pattern { get; set; }
    }

    public interface iFollower
    {
        Int32 depth { get; set; }
        Regex pattern { get; set; }
    }

}
