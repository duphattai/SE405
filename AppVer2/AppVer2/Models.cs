using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppVer2
{
    public class TranslateResponse
    {
        public int code { get; set; }
        public string lang { get; set; }
        public string[] text { get; set; }
    }
}
