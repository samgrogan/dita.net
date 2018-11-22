using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dita.Net.Console {
    public class Parameters {
        public const string ARG_COMMAND = "command";
        public const string ARG_INPUT = "input";
        private const string ARG_INPUT_ABBR = "i";
        public const string ARG_OUTPUT = "output";
        private const string ARG_OUTPUT_ABBR = "o";

        public const string CMD_VERIFY = "verify";
        public const string CMD_CONVERT = "convert";

        // Parses the input into a dictionary
        public Dictionary<string, string> ParseArgs(string[] args, out bool isValid) {
            // The object to return
            Dictionary<string, string> config = new Dictionary<string, string>();
            isValid = true;

            if (args.Length > 1) {
                // First arg is always the command
                string cmdArg = args[0].Trim().ToLower();
                if (!string.IsNullOrWhiteSpace(cmdArg)) {
                    config.Add(ARG_COMMAND, cmdArg);
                }
                else {
                    isValid = false;
                    System.Console.WriteLine("Command argument is missing.");
                }

                // Loop through and parse the additional arguments
                for (int argIndex = 1; argIndex < args.Length; argIndex++) {
                    // Each argument should be of the form key=value
                    string arg = args[argIndex].Trim();
                    if (!string.IsNullOrWhiteSpace(arg)) {
                        string[] argParts = arg.Split('=');
                        if (argParts.Length == 2) {
                            string key = argParts[0].Trim().ToLower();
                            string value = argParts[1].Trim();

                            // Must be a valid key and value
                            if (!string.IsNullOrWhiteSpace(key) && !string.IsNullOrWhiteSpace(value)) {
                                switch (key) {
                                    case ARG_INPUT:
                                    case ARG_INPUT_ABBR:
                                        config.Add(ARG_INPUT, value);
                                        break;
                                    case ARG_OUTPUT:
                                    case ARG_OUTPUT_ABBR:
                                        config.Add(ARG_OUTPUT, value);
                                        break;
                                    default:
                                        isValid = false;
                                        System.Console.WriteLine($"Unknown parameters {key}");
                                        break;
                                }
                            }
                            else {
                                isValid = false;
                                System.Console.WriteLine($"{key}={value} is not properly formatted");
                            }
                        }
                        else {
                            isValid = false;
                            System.Console.WriteLine($"{arg} is not properly formatted");
                        }
                    }
                }
            }
            else {
                isValid = false;
            }

            // Return the dictionary
            return config;
        }
    }
}