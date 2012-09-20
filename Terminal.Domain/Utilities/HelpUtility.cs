using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terminal.Domain.Commands.Interfaces;
using Terminal.Domain.Objects;
using Terminal.Domain.Enums;
using Mono.Options;
using System.IO;

namespace Terminal.Domain.Utilities
{
    public static class HelpUtility
    {
        public static void WriteHelpInformation(
            CommandResult commandResult,
            string name,
            string parameters,
            string description,
            OptionSet options
        )
        {
            var displayMode = DisplayMode.DontWrap | DisplayMode.DontType;
            commandResult.WriteLine(displayMode, description);
            commandResult.WriteLine();
            commandResult.WriteLine(displayMode, "Usage: {0} {1}", name, parameters);
            commandResult.WriteLine();
            commandResult.WriteLine(displayMode, "Options:");
            commandResult.WriteLine();
            var stringWriter = new StringWriter();
            options.WriteOptionDescriptions(stringWriter);
            commandResult.WriteLine(displayMode, stringWriter.ToString());
        }
    }
}
