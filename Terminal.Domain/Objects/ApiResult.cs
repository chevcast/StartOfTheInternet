using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Terminal.Domain.Objects
{
    public class ApiResult
    {
        public string Command { get; set; }
        public string ContextStatus { get; set; }
        public string ContextText { get; set; }
        public string CurrentUser { get; set; }
        public string EditText { get; set; }
        public string SessionId { get; set; }
        public string TerminalTitle { get; set; }

        public bool ClearScreen { get; set; }
        public bool Exit { get; set; }
        public bool PasswordField { get; set; }
        public bool ScrollToBottom { get; set; }

        public List<ApiDisplayItem> DisplayItems { get; set; }

        public string CommandContext { get; set; }
    }
}