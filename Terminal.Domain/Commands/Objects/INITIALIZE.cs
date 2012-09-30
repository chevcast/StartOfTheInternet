using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terminal.Domain.Enums;
using Terminal.Domain.Objects;
using Terminal.Domain.Commands.Interfaces;
using Terminal.Domain.Entities;
using Terminal.Domain.Settings;
using System.IO;
using Mono.Options;
using Terminal.Domain.Utilities;

namespace Terminal.Domain.Commands.Objects
{
    public class INITIALIZE : ICommand
    {
        public CommandResult CommandResult { get; set; }

        public IEnumerable<ICommand> AvailableCommands { get; set; }

        public string[] Roles
        {
            get { return RoleTemplates.Everyone; }
        }

        public string Name
        {
            get { return "INITIALIZE"; }
        }

        public string Parameters
        {
            get { return "[Option(s)]"; }
        }

        public string Description
        {
            get { return "Displays startup screen."; }
        }

        public bool ShowHelp
        {
            get { return false; }
        }

        public void Invoke(string[] args)
        {
            var options = new OptionSet();
            options.Add(
                "?|help",
                "Show help information.",
                x =>
                {
                    HelpUtility.WriteHelpInformation(
                        this.CommandResult,
                        this.Name,
                        this.Parameters,
                        this.Description,
                        options
                    );
                }
            );

            if (args == null)
            {
                if (this.CommandResult.CurrentUser == null)
                {
                    this.CommandResult.CommandContext.Deactivate();
                    //this.CommandResult.WriteLine(DisplayMode.DontType | DisplayMode.DontWrap, AppSettings.Logo);
                    //this.CommandResult.WriteLine();
                    //this.CommandResult.WriteLine("Type HELP to begin.");
                    var displayMode = DisplayMode.Bold;
                    this.CommandResult.WriteLine(displayMode, "Wilkommen zu dem Anfang des Internets. Bitte melden Sie sich");
                    this.CommandResult.WriteLine(displayMode, "an, um fortzufahren.");
                    this.CommandResult.WriteLine();
                    this.CommandResult.WriteLine(displayMode, "Bienvenue au début de l'Internet. Veuillez soumettre votre");
                    this.CommandResult.WriteLine(displayMode, "identification pour continuer.");
                    this.CommandResult.WriteLine();
                    this.CommandResult.WriteLine(displayMode, "Welcome to the start of the internet. Please type 'HELP' to continue.");
                    this.CommandResult.WriteLine();
                    this.CommandResult.WriteLine(displayMode, "Добро пожаловать в начало интернета. Пожалуйста, войдите,");
                    this.CommandResult.WriteLine(displayMode, "чтобы продолжить.");
                    this.CommandResult.WriteLine();
                    this.CommandResult.WriteLine(displayMode, "Välkommen till början av internet. För att fortsätta, ange dina");
                    this.CommandResult.WriteLine(displayMode, "inloggningsuppgifter.");
                    this.CommandResult.WriteLine();
                    this.CommandResult.WriteLine(displayMode, "インターネットの開始へようこそ。続行するにはログインしてください。");
                    this.CommandResult.WriteLine();
                    this.CommandResult.WriteLine(displayMode, "欢迎到互联网的开始。请登录继续。");
                    if (DateTime.Now.Month == (int)Month.October)
                    {
                        this.CommandResult.WriteLine(DisplayMode.DontType | DisplayMode.DontWrap, @"
                          .,'
                       .'`.'
                      .' .'
          _.ood0Pp._ ,'  `.~ .q?00doo._
      .od00Pd0000Pdb._. . _:db?000b?000bo.
    .?000Pd0000PP?000PdbMb?000P??000b?0000b.
  .d0000Pd0000P'  `?0Pd000b?0'  `?000b?0000b.
 .d0000Pd0000?'     `?d000b?'     `?00b?0000b.
 d00000Pd0000Pd0000Pd00000b?00000b?0000b?0000b
 ?00000b?0000b?0000b?b    dd00000Pd0000Pd0000P
 `?0000b?0000b?0000b?0b  dPd00000Pd0000Pd000P'
  `?0000b?0000b?0000b?0bd0Pd0000Pd0000Pd000P'
    `?000b?00bo.   `?P'  `?P'   .od0Pd000P'
      `~?00b?000bo._  .db.  _.od000Pd0P~'
          `~?0b?0b?000b?0Pd0Pd000PdP~'");
                        this.CommandResult.WriteLine(DisplayMode.Inverted, "                HAPPY HALLOWEEN!                ");
                    }
                    else if (DateTime.Now.Month == (int)Month.December)
                    {
                        this.CommandResult.WriteLine(DisplayMode.DontType | DisplayMode.DontWrap, @"
                            |                         _...._
                         \  _  /                    .::o:::::.
                          (\o/)                    .:::'''':o:.
                      ---  / \  ---                :o:_    _:::
                           >*<                     `:)_>()<_(:'
                          >0<@<                 @    `'//\\'`    @ 
                         >>>@<<*              @ #     //  \\     # @
                        >@>*<0<<<           __#_#____/'____'\____#_#__
                       >*>>@<<<@<<         [__________________________]
                      >@>>0<<<*<<@<         |=_- .-/\ /\ /\ /\--. =_-|
                     >*>>0<<@<<<@<<<        |-_= | \ \\ \\ \\ \ |-_=-|
                    >@>>*<<@<>*<<0<*<       |_=-=| / // // // / |_=-_|
      \*/          >0>>*<<@<>0><<*<@<<      |=_- |`-'`-'`-'`-'  |=_=-|
  ___\\U//___     >*>>@><0<<*>>@><*<0<<     | =_-| o          o |_==_| 
  |\\ | | \\|    >@>>0<*<<0>>@<<0<<<*<@<    |=_- | !     (    ! |=-_=|
  | \\| | _(UU)_ >((*))_>0><*<0><@<<<0<*<  _|-,-=| !    ).    ! |-_-=|_
  |\ \| || / //||.*.*.*.|>>@<<*<<@>><0<<@</=-((=_| ! __(:')__ ! |=_==_-\
  |\\_|_|&&_// ||*.*.*.*|_\\db//__     (\_/)-=))-|/^\=^=^^=^=/^\| _=-_-_\
  ''''|'.'.'.|~~|.*.*.*|     ____|_   =('.')=//   ,------------.      
  jgs |'.'.'.|   ^^^^^^|____|>>>>>>|  ( ~~~ )/   (((((((())))))))   
      ~~~~~~~~         '''''`------'  `w---w`     `------------'");
                        this.CommandResult.WriteLine(DisplayMode.Inverted, "                                HAPPY HOLIDAYS!                                ");
                    }
                    //this.CommandResult.WriteLine();
                    //this.CommandResult.WriteLine("Type 'HELP' to begin.");
                }
                else
                    this.CommandResult.WriteLine("You are currently logged in as {0}.", this.CommandResult.CurrentUser.Username);
            }
            else
                try
                {
                    options.Parse(args);
                }
                catch (OptionException ex)
                {
                    this.CommandResult.WriteLine(ex.Message);
                }
        }
    }
}
