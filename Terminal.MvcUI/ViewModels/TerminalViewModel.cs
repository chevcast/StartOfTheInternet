using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Terminal.Domain.Objects;

namespace Terminal.MvcUI.ViewModels
{
    public class TerminalViewModel
    {
        public string Display { get; set; }
        public string Notifications { get; set; }
        public string Cli { get; set; }
        public string ContextText { get; set; }
        public bool PasswordField { get; set; }
        public string Title { get; set; }
        public CommandContext CommandContext { get; set; }
    }
}