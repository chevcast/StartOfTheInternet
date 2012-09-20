using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Terminal.Domain;
using System.Net.Mime;
using Terminal.Domain.Objects;
using Terminal.Domain.Enums;
using Terminal.Domain.ExtensionMethods;
using System.Web.Script.Serialization;
using System.Web.Security;
using Microsoft.Web.Mvc;

namespace Terminal.MvcUI.Controllers
{
    [ValidateInput(false)]
    public class ApiController : Controller
    {
        private TerminalCore _terminalApi;
        private CommandContext _commandContext;

        public ApiController(TerminalCore terminalApi)
        {
            _terminalApi = terminalApi;
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            var response = filterContext.HttpContext.Response;
            response.Write(filterContext.Exception.Message);
            response.ContentType = MediaTypeNames.Text.Plain;
            filterContext.ExceptionHandled = true;
        }


        public JsonResult Index([Deserialize]CommandContext commandContext, string cli, bool parseAsHtml = false)
        {
            _terminalApi.Username = User.Identity.IsAuthenticated ? User.Identity.Name : null;
            _terminalApi.IPAddress = Request.UserHostAddress;
            _terminalApi.CommandContext = commandContext;
            _terminalApi.ParseAsHtml = true;//parseAsHtml;
            var commandResult = _terminalApi.ExecuteCommand(cli);

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

            var displayItems = new List<ApiDisplayItem>();
            commandResult.Display.ForEach(x => displayItems.Add(new ApiDisplayItem
            {
                Text = x.Text,
                Bold = (x.DisplayMode & DisplayMode.Bold) != 0,
                Dim = (x.DisplayMode & DisplayMode.Dim) != 0,
                DontType = (x.DisplayMode & DisplayMode.DontType) != 0,
                Inverted = (x.DisplayMode & DisplayMode.Inverted) != 0,
                Italics = (x.DisplayMode & DisplayMode.Italics) != 0,
                Mute = (x.DisplayMode & DisplayMode.Mute) != 0,
                Parse = (x.DisplayMode & DisplayMode.Parse) != 0
            }));

            var apiResult = new ApiResult
            {
                ClearScreen = commandResult.ClearScreen,
                Command = commandResult.Command,
                ContextStatus = commandResult.CommandContext.Status.ToString(),
                ContextText = commandResult.CommandContext.Command
                + (commandResult.CommandContext.Text.IsNullOrEmpty()
                ? null : string.Format(" {0}", _terminalApi.CommandContext.Text)),
                CurrentUser = commandResult.CurrentUser != null ? commandResult.CurrentUser.Username : null,
                DisplayItems = displayItems,
                EditText = commandResult.EditText,
                Exit = commandResult.Exit,
                PasswordField = commandResult.PasswordField,
                ScrollToBottom = commandResult.ScrollToBottom,
                SessionId = Session.SessionID,
                TerminalTitle = commandResult.TerminalTitle,
                CommandContext = commandResult.CommandContext.Serialize()
            };
            //string json = new JavaScriptSerializer().Serialize(apiResult);
            //return callback != null ? string.Format("{0}({1});", callback, json) : json;
            return Json(apiResult);
        }

        public string GetSessionId()
        {
            return Session.SessionID;
        }

        public JsonResult Metadata()
        {
            var metadata = new
            {
                Command = new { Type = "string", Description = "The command that was processed by the terminal core." },
                ContextStatus = new { Type = "string", Description = "The current command context status. Disabled, Passive, or Forced" },
                ContextText = new { Type = "string", Description = "Text to be displayed next to the CLI bracket representing the current command context." },
                CurrentUser = new { Type = "string", Description = "Displays username of the logged-in user. Null if no user is logged in." },
                EditText = new { Type = "string", Description = "Text to be inserted into the CLI for the user to edit." },
                SessionId = new { Type = "string", Description = "The session ID to be submitted with each subsequent request." },
                TerminalTitle = new { Type = "string", Description = "Text to be displayed in the title bar of the client application." },
                ClearScreen = new { Type = "boolean", Description = "True if the screen should be cleared." },
                Exit = new { Type = "boolean", Description = "True if the client application should exit." },
                PasswordField = new { Type = "boolean", Description = "True if the client application should transform the command line into a password field for the next request." },
                ScrollToBottom = new { Type = "boolean", Description = "True if clients capable of controlling scrolling should scroll to the bottom." },

                DisplayItems = new List<object>()
                {
                    new
                    {
                        Dim = new { Type="boolean", Description = "True if the text should be more subtle than the standard text." },
                        Inverted = new { Type = "boolean", Description = "True if the colors for this line of text should be inverted. (i.e. Foreground Color = Background Color & Background Color = Foreground Color)" },
                        Parse = new { Type = "boolean", Description = "True if the line of text could contain BBCode tags to be parsed. This is true even if the tags have already been parsed as HTML via the parseAsHtml API option." },
                        Italics = new { Type = "boolean", Description = "True if the text should be displayed in italics." },
                        Bold = new { Type = "boolean", Description = "True if the text should be displayed in bold." },
                        DontType = new { Type = "boolean", Description = "True if the text should not be displayed by any character-by-charcater typing script. (e.g. jQuery.coolType)" },
                        Mute = new { Type = "boolean", Description = "True if sound should not be played while the text is being rendered character-by-character. (This will be true for all items if the user has specified the mute option in their settings)" },
                        Text = new { Type = "string", Description = "The text to be displayed. Usually a single line but may contain new-line characters or break tags if the parseAsHtml API option was set to true." }
                    }
                }
            };

            return Json(metadata, JsonRequestBehavior.AllowGet);
        }
    }
}
