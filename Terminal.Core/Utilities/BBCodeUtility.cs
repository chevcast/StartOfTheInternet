using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Text.RegularExpressions;
using Terminal.Core.ExtensionMethods;
using Terminal.Core.Settings;
using Terminal.Core.Data.Entities;
using System.Drawing;
using System.Net;
using System.IO;
using CodeKicker.BBCode;
using Terminal.Core.Data.Repositories.Interfaces;

namespace Terminal.Core.Utilities
{
    /// <summary>
    /// Utilities for handling tag conversion. The UI can utilize this utility when parsing text containing BBCode tags.
    /// </summary>
    public static class BBCodeUtility
    {
        #region Public Methods

        /// <summary>
        /// Find formatting tags in the text and transform them into the appropriate HTML.
        /// </summary>
        /// <param name="text">The text to be transformed.</param>
        /// <returns>A formatted string.</returns>
        public static string ConvertTagsToHtml(string text)
        {
            var parser = new BBCodeParser(new[]
                {
                    new BBTag("code", "<pre><code>", "</code></pre>"), 
                    new BBTag("color", "<span style='color: ${color}; border-color: ${color};'>", "</span>", new BBAttribute("color", "")), 
                    new BBTag("img", "", "", true, true, content =>
                        {
                            string imageUrl = ConfirmHttp(content);
                            try
                            {
                                var client = new WebClient();
                                var stream = client.OpenRead(imageUrl);
                                var bitmap = new Bitmap(stream);
                                stream.Flush();
                                stream.Close();
                                var width = Convert.ToDecimal(bitmap.Size.Width);
                                var height = Convert.ToDecimal(bitmap.Size.Height);
                                if (width > 500m)
                                {
                                    var ratio = width / 500m;
                                    height = height / ratio;
                                    width = 500m;
                                }
                                return string.Format("<div style='height: {0}px; width: {1}px;'><a target='_blank' href='{2}'><img style='height: {0}px; width: {1}px;' src='{2}' /></a></div>", height, width, imageUrl);
                            }
                            catch
                            {
                                return string.Format("<div><a target='_blank' href='{0}'><img src='{0}' /></a></div>", imageUrl);
                            }
                        }), 
                    new BBTag("url", "<a target='_blank' href='${href}'>", "</a>", new BBAttribute("href", "", context =>
                            {
                                if (!string.IsNullOrWhiteSpace(context.AttributeValue))
                                    return context.AttributeValue;

                                var tagContent = context.GetAttributeValueByID(BBTag.ContentPlaceholderName);
                                return tagContent;   
                            })), 
                    new BBTag("quote", "<div class='quote'>", "</div>"), 
                    new BBTag("b", "<b>", "</b>"), 
                    new BBTag("i", "<span style=\"font-style:italic;\">", "</span>"),
                    new BBTag("u", "<span style=\"text-decoration:underline;\">", "</span>"),
                    new BBTag("s", "<strike>", "</strike>"),
                    new BBTag("br", "<br />", "", true, false)
                });
            return parser.ToHtml(text);
        }

        public static string ConvertTagsForConsole(string text)
        {
            var parser = new BBCodeParser(new[]
                {
                    new BBTag("code", "{", "}"), 
                    new BBTag("color", "", ""), 
                    new BBTag("img", "", ""), 
                    new BBTag("url", "(${content}) ${href}", "", false, true, new BBAttribute("href", "")), 
                    new BBTag("transmit", "", ""), 
                    new BBTag("quote", "\n\"", "\"\n"), 
                    new BBTag("b", "*", "*"), 
                    new BBTag("i", "'", "'"),
                    new BBTag("u", "_", "_"),
                    new BBTag("s", "-", "-"),
                });
            return parser.ToHtml(text, false);
        }

        /// <summary>
        /// Find formatting tags in the text and transform them into static tags.
        /// </summary>
        /// <param name="text">The text to be transformed.</param>
        /// <param name="replyRepository">An instance of IReplyRepository.</param>
        /// <param name="isModerator">True if the current user is a moderator.</param>
        /// <returns>A formatted string.</returns>
        public static string SimplifyComplexTags(string text, IReplyRepository replyRepository, bool isModerator)
        {
            var parser = new BBCodeParser(new[]
                { 
                    new BBTag("quote", "", "", true, true, content =>
                        {
                            string reformattedQuote = string.Format(@"[quote]{0}[/quote]", content);
                            if (content.IsLong())
                            {
                                Reply reply = replyRepository.GetReply(content.ToLong());
                                if (reply != null)
                                    if (!reply.IsModsOnly() || isModerator)
                                    {
                                        var author = reply.Topic.Board.Anonymous ? "Anon" : reply.Username;
                                        reformattedQuote = string.Format("[quote]Posted by: [transmit=USER]{0}[/transmit] on {1}\n\n{2}[/quote]", author, reply.PostedDate, reply.Body);
                                    }
                            }
                            return reformattedQuote;
                        }),  
                });
            return parser.ToHtml(text, false);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Checks if a string begins with http. If not, it adds it.
        /// </summary>
        /// <param name="url">The URL to check.</param>
        /// <returns>URL complete with http://</returns>
        private static string ConfirmHttp(string url)
        {
            return new string[] { "http://", "https://", "ftp://", "mailto:" }.Any(x => url.StartsWith(x, true, System.Globalization.CultureInfo.CurrentCulture)) ? url : "http://" + url;
        }

        #endregion
    }
}
