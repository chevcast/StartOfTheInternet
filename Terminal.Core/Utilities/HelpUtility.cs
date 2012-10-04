using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terminal.Core.Commands.Interfaces;
using Terminal.Core.Objects;
using Terminal.Core.Enums;
using Mono.Options;
using System.IO;

namespace Terminal.Core.Utilities
{
    public static class HelpUtility
    {
        public static void WriteHelpInformation(ICommand command, OptionSet options)
        {
            var displayMode = DisplayMode.DontWrap | DisplayMode.DontType;
            command.CommandResult.WriteLine(displayMode, command.Description);
            command.CommandResult.WriteLine();
            command.CommandResult.WriteLine(displayMode, "Usage: {0} {1}", command.Name, command.Parameters);
            command.CommandResult.WriteLine();
            command.CommandResult.WriteLine(displayMode, "Options:");
            command.CommandResult.WriteLine();
            var stringWriter = new StringWriter();
            options.WriteOptionDescriptions(stringWriter);
            command.CommandResult.WriteLine(displayMode, stringWriter.ToString());
        }
    }
}
