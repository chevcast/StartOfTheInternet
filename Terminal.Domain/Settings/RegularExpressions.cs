using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Terminal.Domain.Settings
{
    /// <summary>
    /// Static class containing common regular expression strings.
    /// </summary>
    public static class RegularExpressions
    {
        /// <summary>
        /// Expression to find all root-level BBCode tags. Use this expression recursively to obtain nested tags.
        /// </summary>
        public static string BBCodeTags
        {
            get
            {
                return @"
                        (?>
                        \[ (?<tag>[^][/=\s]+) \s*
                        (?: = \s* (?<val>[^][]*) \s*)?
                        \]
                        )
                          (?<content>
                            (?>
                               \[(?:unsuccessful)\]  # self closing
                               |
                               \[(?<innertag>[^][/=\s]+)[^][]*]
                               |
                               \[/(?<-innertag>\k<innertag>)]
                               |
                               .
                            )*
                            (?(innertag)(?!))
                          )
                        \[/\k<tag>\]
                        ";
            }
        }
    }
}
