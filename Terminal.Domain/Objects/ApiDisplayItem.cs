using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Terminal.Domain.Objects
{
    public class ApiDisplayItem
    {
        public bool Dim { get; set; }
        public bool Inverted { get; set; }
        public bool Parse { get; set; }
        public bool Italics { get; set; }
        public bool Bold { get; set; }
        public bool DontType { get; set; }
        public bool Mute { get; set; }

        public string Text { get; set; }
    }
}