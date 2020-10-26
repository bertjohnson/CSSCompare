using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SPBert.CSSCompare
{
    /// <summary>
    /// CSS Compare - naive comparison tool between two stylesheets.
    /// Accepts filenames of two stylesheets.
    /// Compares and outputs differences from v1 to v2.
    /// </summary>
    class Program
    {
        /// <summary>
        /// CSS Compare - naive comparison tool between two stylesheets.
        /// </summary>
        /// <param name="args">Strings specifying filenames of v1file and v2file.</param>
        static void Main(string[] args)
        {
            #region Parse Arguments
            string v1file = "";
            string v2file = "";

            // Parse command line parameters.
            string lastArg = "";
            foreach (string arg in args)
            {
                switch (lastArg)
                {
                    case "-v1":
                    case "-v1file":
                    case "-file1":
                        v1file = arg;
                        break;
                    case "-v2":
                    case "-v2file":
                    case "-file2":
                        v2file = arg;
                        break;
                }
                lastArg = arg.ToLower();
            }
            #endregion Parse Arguments

            #region Error Handling
            // Handle invalid parameters.
            if (string.IsNullOrEmpty(v1file) || string.IsNullOrEmpty(v2file))
            {
                Console.WriteLine("Please specify an original file with the -v1 parameter and the comparison file with the -v2 parameter.");
                return;
            }

            // Handle invalid files
            if (!File.Exists(v1file))
            {
                Console.WriteLine("File \"" + v1file + "\" not found.");
                return;
            }
            if (!File.Exists(v2file))
            {
                Console.WriteLine("File \"" + v2file + "\" not found.");
                return;
            }
            #endregion Error Handling

            #region Read and Normalize Input Files
            // Read and parse both files.
            StreamReader sr = new StreamReader(v1file);
            string v1 = sr.ReadToEnd();
            sr.Dispose();

            sr = new StreamReader(v2file);
            string v2 = sr.ReadToEnd();
            sr.Dispose();

            string normalizedV1 = NormalizeCSS(v1);
            string normalizedV2 = NormalizeCSS(v2);
            #endregion Read and Normalize Input Files

            #region Build Original Objects
            // Object to keep track of elements and their styles.
            Dictionary<string, HashSet<string>> Styles = new Dictionary<string, HashSet<string>>();

            bool inComment = false;
            string currentMediaBlock = "";
            string currentElement = "";
            string lastLine = "";
            string[] lines = normalizedV1.Split('\n');

            // Loop through and track all styles.
            foreach (string line in lines)
            {
                if (line.StartsWith("@"))
                {
                    currentMediaBlock = line.Substring(0, line.Length);
                }
                else if (line == "*/")
                {
                    inComment = false;
                }
                else if (!inComment)
                {
                    switch (line)
                    {
                        case "/*":
                            inComment = true;
                            break;
                        case "{":
                            if (!lastLine.StartsWith("@"))
                            {
                                currentElement = lastLine;

                                if (!Styles.ContainsKey(currentMediaBlock + "|" + currentElement))
                                    Styles.Add(currentMediaBlock + "|" + currentElement, new HashSet<string>());
                            }
                            break;
                        case "}":
                            if (!string.IsNullOrEmpty(currentElement))
                                currentElement = "";
                            else
                                currentMediaBlock = "";
                            break;
                        default:
                            if (!string.IsNullOrEmpty(currentElement))
                                Styles[currentMediaBlock + "|" + currentElement].Add(line.Trim());
                            break;
                    }
                }
                lastLine = line;
            }
            #endregion Build Original Objects

            #region Compare Updated Objects
            currentMediaBlock = "";
            currentElement = "";
            lastLine = "";
            lines = normalizedV2.Split('\n');

            // Loop through and remove duplicate styles.
            foreach (string line in lines)
            {
                if (line.StartsWith("@"))
                    currentMediaBlock = line;
                else if (line == "*/")
                    inComment = false;
                else if (!inComment)
                {
                    switch (line)
                    {
                        case "/*":
                            inComment = true;
                            break;
                        case "{":
                            if (!lastLine.StartsWith("@"))
                            {
                                currentElement = lastLine;

                                if (!Styles.ContainsKey(currentMediaBlock + "|" + currentElement))
                                    Styles.Add(currentMediaBlock + "|" + currentElement, new HashSet<string>());
                            }
                            break;
                        case "}":
                            if (!string.IsNullOrEmpty(currentElement))
                                currentElement = "";
                            else
                                currentMediaBlock = "";
                            break;
                        default:
                            if (!string.IsNullOrEmpty(currentElement))
                            {
                                string normalizedLine = line.Trim();
                                if (Styles[currentMediaBlock + "|" + currentElement].Contains(normalizedLine))
                                    Styles[currentMediaBlock + "|" + currentElement].Remove(normalizedLine);
                            }
                            break;
                    }
                }
                lastLine = line;
            }
            #endregion Compare Updated Objects

            #region Print Remaining Objects
            string lastMediaBlock = "";

            // Loop through and print out styles unique to the first CSS file.
            foreach (string key in Styles.Keys)
            {
                if (Styles[key].Count > 0)
                {
                    string extraIndentation = "";
                    string mediaBlock = key.Substring(0, key.IndexOf("|"));
                    if (mediaBlock.Length > 0)
                    {
                        if (mediaBlock != lastMediaBlock)
                        {
                            if (!string.IsNullOrEmpty(lastMediaBlock))
                                Console.WriteLine("}");
                            Console.WriteLine(mediaBlock + "\n" + "{");
                        }
                        extraIndentation = "\t";
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(lastMediaBlock))
                            Console.WriteLine("}");
                        extraIndentation = "";
                    }
                    string element = key.Substring(key.IndexOf("|") + 1);

                    Console.WriteLine(extraIndentation + element + "{");
                    foreach (string value in Styles[key])
                        Console.WriteLine(extraIndentation + "\t" + value);
                    Console.WriteLine(extraIndentation + "}");

                    lastMediaBlock = mediaBlock;
                }
            }

            if (!string.IsNullOrEmpty(lastMediaBlock))
                Console.WriteLine("}");
            #endregion Print Remaining Objects
        }

        /// <summary>
        /// Function to standardize CSS file spacing and other formatting.
        /// </summary>
        /// <param name="input">The raw input to be normalized.</param>
        /// <returns>Normalized output, with whitespace minimized.</returns>
        private static string NormalizeCSS(string input)
        {
            #region Preliminary Cleanup
            // Collapse whitespace.
            input = input.Replace('\r', '\n');
            input = input.Replace("\t", "");
            
            while (input.IndexOf("  ") > -1)
                input = input.Replace("  ", " ");
            #endregion Preliminary Cleanup

            string[] lines = input.Split('\n');

            StringBuilder output = new StringBuilder();

            #region Normalize Lines
            // Iterate through and normalize each line.
            foreach (string line in lines)
            {
                if (!string.IsNullOrEmpty(line))
                    AddNormalizedLine(line, ref output);
            }
            #endregion Normalize Lines

            return output.ToString().Replace("\n ", "\n");
        }

        /// <summary>
        /// Function to standardize CSS line spacing and other formatting.
        /// </summary>
        /// <param name="line">Line to be normalized.</param>
        /// <param name="output">Normalized line.</param>
        private static void AddNormalizedLine(string line, ref StringBuilder output)
        {
            // Eliminate trailing and leading whitespace.
            string currentLine = line.Trim();

            // Break individual styles into their own lines.
            int semicolon = currentLine.IndexOf(";");
            while (semicolon > -1 && !string.IsNullOrEmpty(currentLine))
            {
                // Handle lines starting with a semicolon.
                if (semicolon == 0) {
                    if (currentLine.Length > 1)
                    {
                        currentLine = currentLine.Substring(1);
                        semicolon = currentLine.IndexOf(";");
                        continue;
                    }
                    else
                    {
                        return;
                    }
                }

                if (semicolon < currentLine.Length - 1)
                {
                    {
                        AddNormalizedLine(currentLine.Substring(0, semicolon + 1), ref output);
                        currentLine = currentLine.Substring(semicolon);
                    }
                }
                else
                {
                    // Remove trailing semicolon.
                    currentLine = currentLine.Substring(0, currentLine.Length - 1);
                }
                semicolon = currentLine.IndexOf(";");
            }

            // Move each of the following characters to its own line.
            string[] specialCharacters = new string[] { "{", "}", "/*", "*/" };
            foreach (string specialCharacter in specialCharacters)
            {
                int openCharacter = currentLine.IndexOf(specialCharacter);
                while (openCharacter > -1 && !string.IsNullOrEmpty(currentLine))
                {
                    if (openCharacter == 0)
                        output.Append(specialCharacter + "\n");
                    else
                    {
                        AddNormalizedLine(currentLine.Substring(0, openCharacter), ref output);
                        output.Append(specialCharacter + "\n");
                    }

                    currentLine = currentLine.Substring(openCharacter + specialCharacter.Length);
                    openCharacter = currentLine.IndexOf(specialCharacter);
                }
            }

            // Append newline.
            if (!string.IsNullOrEmpty(currentLine))
                output.Append(currentLine + "\n");
        }
    }
}
