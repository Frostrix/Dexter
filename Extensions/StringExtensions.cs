﻿using System.Text.RegularExpressions;

namespace Dexter.Extensions {
    /// <summary>
    /// The String Extensions class offers a variety of different extensions that can be applied to a string to modify it.
    /// These include the prettify, sanitize and markdown extensions.
    /// </summary>
    public static class StringExtensions {

        private static readonly string[] SensitiveCharacters = { "\\", "*", "_", "~", "`", "|", ">", "[", "(" };

        /// <summary>
        /// The Prettify method removes all the characters before the name of the class and only selects characters from A-Z.
        /// </summary>
        /// <param name="Name">The string you wish to run through the REGEX expression.</param>
        /// <returns>A sanitised string with the characters before the name of the class removed.</returns>
        public static string Prettify(this string Name) => Regex.Replace(Name, @"(?<!^)(?=[A-Z])", " ");

        /// <summary>
        /// The Sanitize method removes the "Commands" string from the name of the class.
        /// </summary>
        /// <param name="Name">The string you wish to run through the replace method.</param>
        /// <returns>The name of a module with the "Commands" string removed.</returns>
        public static string Sanitize(this string Name) => Name.Replace("Commands", string.Empty);

        /// <summary>
        /// The Sanitize Markdown method removes any sensitive characters that may otherwise change the created embed.
        /// It does this by looping through and replacing any sensitive characters that may break the embed.
        /// </summary>
        /// <param name="Text">The string you wish to be run through the command.</param>
        /// <returns>The text which has been sanitized and has had the sensitive characters removed.</returns>
        public static string SanitizeMarkdown(this string Text) {
            foreach (string Unsafe in SensitiveCharacters)
                Text = Text.Replace(Unsafe, $"\\{Unsafe}");
            return Text;
        }

    }
}