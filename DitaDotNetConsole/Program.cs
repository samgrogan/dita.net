using System.Collections.Generic;

namespace Dita.Net.Console
{
    class Program
    {
        static int Main(string[] args)
        {
            // Help object, for displaying help to the users
            Help help = new Help();

            // Parse the input and see if it is valid
            Parameters parameters = new Parameters();
            Dictionary<string, string> config = parameters.ParseArgs(args, out bool isValid);


            // Did we get any arguments?
            if (isValid) {
                // What command were we asked to perform
                switch (config[Parameters.ARG_COMMAND]) {
                    case Parameters.CMD_VERIFY:
                        System.Console.WriteLine($"Verifying {config[Parameters.ARG_INPUT]}...");
                        return 0;
                    case Parameters.CMD_CONVERT:
                        System.Console.WriteLine($"Converting {config[Parameters.ARG_INPUT]}...");
                        return 0;
                    default:
                        break;
                }
            }

            // If we get here then something went wrong, so exit with an error
            help.WriteHelpToConsole();
            return -1;
        }
    }
}
