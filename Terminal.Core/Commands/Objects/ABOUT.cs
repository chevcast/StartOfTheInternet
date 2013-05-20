using Mono.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terminal.Core.Commands.Interfaces;
using Terminal.Core.Enums;
using Terminal.Core.Settings;
using Terminal.Core.Utilities;

namespace Terminal.Core.Commands.Objects
{
    public class ABOUT : ICommand
    {
        public Core.Objects.CommandResult CommandResult { get; set; }

        public IEnumerable<ICommand> AvailableCommands { get; set; }

        public string[] Roles
        {
            get { return RoleTemplates.AllLoggedIn; }
        }

        public string Name
        {
            get { return "ABOUT"; }
        }

        public string Parameters
        {
            get { return "[Option(s)]"; }
        }

        public string Description
        {
            get { return "Display information about the Omega Directive."; }
        }

        public bool ShowHelp
        {
            get { return true; }
        }

        public void Invoke(string[] args)
        {
            var options = new OptionSet();
            options.Add(
                "?|help",
                "Show help information.",
                x => HelpUtility.WriteHelpInformation(this, options)
            );

            if (args != null)
            {
                try
                {
                    options.Parse(args);
                }
                catch (OptionException ex)
                {
                    CommandResult.WriteLine(ex.ToString());
                }
            }
            else
            {
                CommandResult.WriteLine(DisplayMode.Inverted, "OMEGA LOCKDOWN ACTIVE");
                CommandResult.WriteLine();
                CommandResult.WriteLine("Welcome agent {0}. I see that you are interested in learning more about the Omega Directive.", CommandResult.CurrentUser.Username);
                CommandResult.WriteLine("We'll begin your briefing from the beginning.");
                CommandResult.WriteLine();
                CommandResult.WriteLine("As an agent of the resistance you already know about exotic matter (XM).");
                CommandResult.WriteLine("What you don't know is that humanity has encountered this matter before the secret French experiments that you've been told about.");
                CommandResult.WriteLine("Exotic matter consists of a group of unknown but powerful particles chemically bonded to form what is known as the Omega Molecule.");
                CommandResult.WriteLine("Strange energetic properties exhibited by Omega resulted in heavy experimentation as scientists believed Omega, if harnessed correctly, could be a virtually unlimited source of energy.");
                CommandResult.WriteLine();
                CommandResult.WriteLine("Unfortunately, Omega proved to be extremely unstable and all attempts to stabalize it were unsuccessful. Not long into the project one of the experimental attempts at stabalization went extremely wrong.");
                CommandResult.WriteLine("In the middle of the experiment several molecules were successfully fused into a stable molecular lattice, but it didn't last long.");
                CommandResult.WriteLine();
                CommandResult.WriteLine(DisplayMode.DontType | DisplayMode.DontWrap | DisplayMode.Parse, "[img]http://omegamolecule.com/content/OmegaLattice.jpg[/img]");
                CommandResult.WriteLine();
                CommandResult.WriteLine("The Omega lattice immediately began to break down causing a chain reaction. The scientists only had a few seconds to enact emergency safety protocols and fill the test chamber with liquid nitrogen, halting the breakdown in mid-cascade.");
                CommandResult.WriteLine("The energy buildup during the last few seconds of the experiment was analyzed vigorously. When the data came back the results were astounding.");
                CommandResult.WriteLine("Just a handful of Omega molecules, if allowed to continue breaking down, would have released an energy equivalent to 1100 times that of an atomic bomb.");
                CommandResult.WriteLine();
                CommandResult.WriteLine("Once the destructive power was revealed it was determined that Omega was too unstable for experimentation and the project was put on permanent hold.");
                CommandResult.WriteLine("The Omega Directive was created to monitor the state of Omega here on Earth and prevent others from experimenting with the molecule.");
                CommandResult.WriteLine("For years nobody else discovered the power of Omega. Several had come close, but never close enough to require intervention, until late last year.");
                CommandResult.WriteLine();
                CommandResult.WriteLine("We have no idea where the first particals of XM began to appear, but it seems the once rare matter is now more abundant on the Earth than we could ever have imagined.");
                CommandResult.WriteLine("XM appears to gather near interesting anomalies known to you as \"portals\". Much to our surprise, these portals appear to be stable structures of Omega Molecules.");
                CommandResult.WriteLine("Needless to say, when we first discovered the properties of these portals we were extremely alarmed and we have since initiated a code blue Omega lockdown as per article 21 subsection D of the Omega Directive.");
                CommandResult.WriteLine("This lockdown forbids us from sharing information about Omega for fear that it could fall into the wrong hands. We must continue to operate in secrecy to protect our race.");
                CommandResult.WriteLine();
                CommandResult.WriteLine("During the lockdown we have continued analyzing these new anomalies to gain a better understanding of the phenomenon. Most members of the Omega Directive have arrived at a consensus that it is highly implausible for these anomalies consisting of stable Omega Molecules to be a natural occurance.");
                CommandResult.WriteLine("Only an intelligence with an extensive understanding of Omega would be able to create so many stable structures. Many have theorized that if enough of these stable anomalies remain within this unkown intelligence's control they could be linked together to form a \"super lattice\". Such a structure would be capable of crossing dimensional barriers that we never imagined existed, let alone that we could cross.");
                CommandResult.WriteLine("As XM particles continue to accumulate around the globe we are becoming increasingly concerned and we must do everything we can to stop it.");
                CommandResult.WriteLine();
                CommandResult.WriteLine("Omega is extremely difficult to erradicate safely. Fortunately for now the stabalization process used on XM particles to form stable Omega structures appears to be practically flawless. Though we are still concerned about the dangers we believe we are relatively safe for the time being.");
                CommandResult.WriteLine("Since we cannot yet safely erradicate the exotic particles from our planet we have taken to doing the next best thing, and that is embracing the Niantic Project. We are still corroborating claims made by Niantic about an alien race known only as \"Shapers\", but the technology developed by Niantic does appear to have the ability to safely manipulate XM particles and stable Omega structures.");
                CommandResult.WriteLine("As an agent of the resistance you already know how to use this technology and we encourage you to continue to do so. However, as part of the Omega Directive we require that you cooperate with fellow Omega agents only for any serious task. Until an agent is a verified member of Omega you have every reason to distrust them, even if they claim to be part of the resistance.");
                CommandResult.WriteLine();
                CommandResult.WriteLine("We are glad to have you as a verified Omega agent, {0}. We hope you will help us keep as many stable structures out of enemy hands as possible. For now, it is our only defense.", CommandResult.CurrentUser.Username);
            }
        }
    }
}
