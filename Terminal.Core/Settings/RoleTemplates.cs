using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Terminal.Core.Settings
{
    /// <summary>
    /// Pre-defined role templates for use inside command classes.
    /// </summary>
    public static class RoleTemplates
    {
        /// <summary>
        /// Available only to logged out users.
        /// </summary>
        public static string[] Visitor
        {
            get { return new string[] { "Visitor" }; }
        }

        /// <summary>
        /// Available only to logged in users.
        /// </summary>
        public static string[] OnlyUsers
        {
            get { return new string[] { "User" }; }
        }

        /// <summary>
        /// Available only to moderators.
        /// </summary>
        public static string[] Moderators
        {
            get { return new string[] { "Moderator" }; }
        }

        /// <summary>
        /// Available only to administrators.
        /// </summary>
        public static string[] Administrators
        {
            get { return new string[] { "Administrator" }; }
        }

        /// <summary>
        /// Available to moderators and administrators.
        /// </summary>
        public static string[] ModsAndAdmins
        {
            get { return new string[] { "Moderator", "Administrator" }; }
        }

        /// <summary>
        /// Returns all roles for logged in users.
        /// </summary>
        public static string[] AllLoggedIn
        {
            get { return new string[] { "User", "Moderator", "Administrator" }; }
        }

        /// <summary>
        /// Returns all roles.
        /// </summary>
        public static string[] Everyone
        {
            get { return new string[] { "Visitor", "User", "Moderator", "Administrator" }; }
        }
    }
}
