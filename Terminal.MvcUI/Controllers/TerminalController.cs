using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Terminal.Domain;
using Terminal.Domain.Objects;
using Terminal.MvcUI.ViewModels;
using Terminal.Domain.ExtensionMethods;
using System.Web.Security;
using Terminal.Domain.Settings;
using System.Text;
using Terminal.Domain.Enums;
using System.Net;
using System.Configuration;
using Microsoft.Web.Mvc;

namespace Terminal.MvcUI.Controllers
{
    [ValidateInput(false)]
    public class TerminalController : Controller
    {
        private TerminalCore _terminalCore;

        public TerminalController(TerminalCore terminalCore)
        {
            _terminalCore = terminalCore;
        }

        public ViewResult Index(string Cli, string Display, [Deserialize]CommandContext CommandContext)
        {
            ModelState.Clear();
            _terminalCore.Username = User.Identity.IsAuthenticated ? User.Identity.Name : null;
            _terminalCore.IPAddress = Request.UserHostAddress;
            _terminalCore.CommandContext = CommandContext;
            _terminalCore.ParseAsHtml = true;
            var commandResult = _terminalCore.ExecuteCommand(Cli);

            if (commandResult.ClearScreen)
                Display = null;

            if (User.Identity.IsAuthenticated)
            {
                if (commandResult.CurrentUser == null)
                    FormsAuthentication.SignOut();
            }
            else
            {
                if (commandResult.CurrentUser != null)
                    FormsAuthentication.SetAuthCookie(commandResult.CurrentUser.Username, true);
            }

            var display = new StringBuilder();
            foreach (var displayItem in commandResult.Display)
            {
                display.Append(displayItem.Text);
                display.Append("<br />");
            }

            if (Display != null)
                Display += "<br />";

            var viewModel = new TerminalViewModel
            {
                Cli = commandResult.EditText,
                ContextText = commandResult.CommandContext.Command
                + (commandResult.CommandContext.Text.IsNullOrEmpty() 
                ? null : string.Format(" {0}", _terminalCore.CommandContext.Text)),
                Display = Display + display.ToString(),
                PasswordField = commandResult.PasswordField,
                Notifications = string.Empty,
                Title = commandResult.TerminalTitle,
                CommandContext = commandResult.CommandContext
            };

            return View(viewModel);
        }
    }
}
