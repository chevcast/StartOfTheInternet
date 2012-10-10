using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Terminal.Core.Objects
{
    public class TerminalEvents
    {
        public TerminalEvents()
        {
            
        }

        public TerminalEventHandler OnBeforeSetContext;
        public TerminalEventHandler OnSetContext;
        public TerminalEventHandler OnBeforeDeactivateContext;
        public TerminalEventHandler OnDeactivateContext;
        public TerminalEventHandler OnBeforeRestoreContext;
        public TerminalEventHandler OnRestoreContext;
        public TerminalEventHandler OnBeforeBackupContext;
        public TerminalEventHandler OnBackupContext;
    }
}
