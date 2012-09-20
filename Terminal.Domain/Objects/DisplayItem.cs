using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terminal.Domain.Enums;

namespace Terminal.Domain.Objects
{
    public class DisplayItem
    {
        public string Text { get; set; }
        public DisplayMode DisplayMode { get; set; }
    }
}
