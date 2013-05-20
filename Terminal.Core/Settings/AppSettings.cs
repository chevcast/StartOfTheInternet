using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.EntityClient;
using System.Data.SqlClient;

namespace Terminal.Core.Settings
{
    /// <summary>
    /// Various static application settings.
    /// </summary>
    public static class AppSettings
    {
        /// <summary>
        /// The logo to display on application initialization.
        /// </summary>
        public static string Logo
        {
            get
            {
                return @"          .@@@@@@@@@@@@@@f          
        @@@@@@@@@808@@@@@@@@t       
      @@@@@@            0@@@@@t     
    .@@@@@                i@@@@@    
   1@@@@@                  :@@@@8   
   @@@@@                    @@@@@f  
  ,@@@@@                    .@@@@@  
  C@@@@0                     @@@@@  
  f@@@@0                     @@@@@  
   @@@@@                    .@@@@@  
   G@@@@                    @@@@@.  
    @@@@8                  ,@@@@f   
     @@@@L                 @@@@C    
      L@@@@              t@@@@      
        @@@@C           @@@@,       
          0@@@8      1@@@@:         
 L@@@@@@@@@@@@@.     @@@@@@@@@@@@@@";
            }
        }

        private static int _maxLineLength = 95;
        public static int MaxLineLength
        {
            get { return _maxLineLength; }
            set { _maxLineLength = value; }
        }

        public static int DividerLength
        {
            get { return _maxLineLength / 2; }
        }

        /// <summary>
        /// The number of topics to display per page.
        /// </summary>
        public static int TopicsPerPage
        {
            get { return 15; }
        }

        /// <summary>
        /// The number of replies to display per page.
        /// </summary>
        public static int RepliesPerPage
        {
            get { return 15; }
        }

        public static int MessagesPerPage
        {
            get { return 15; }
        }
    }
}
