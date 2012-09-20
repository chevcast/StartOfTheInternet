using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Terminal.Domain.Settings
{
    /// <summary>
    /// Pre-defined role templates for use inside command classes.
    /// </summary>
    public static class RoleTemplates
    {
        /// <summary>
        /// Returns only the visitor role.
        /// </summary>
        public static string[] Visitor
        {
            get { return new string[] { "Visitor" }; }
        }

        /// <summary>
        /// Returns only the user role.
        /// </summary>
        public static string[] OnlyUsers
        {
            get { return new string[] { "User" }; }
        }

        /// <summary>
        /// Returns the moderator and user roles.
        /// </summary>
        public static string[] ModsAndUsers
        {
            get { return new string[] { "User", "Moderator" }; }
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
