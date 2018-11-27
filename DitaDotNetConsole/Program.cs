using System.Collections.Generic;

namespace Dita.Net.Console {
    class Program {
        static int Main(string[] args) {
            // Help object, for displaying help to the users
            Help help = new Help();

            // Try to read the input 
            if (args.Length >= 1) {
                Configuration config = Configuration.CreateFromJson(args[0]);

                // What command were we asked to perform
                switch (config.Command) {
                    case Parameters.CommandVerify:
                        System.Console.WriteLine($"Verifying {config.Input}...");

                        DitaVerifier verifier = new DitaVerifier();
                        if (verifier.VerifyFileOrDirectory(config.Input)) {
                            return 0;
                        }

                        return -1;

                    case Parameters.CommandConvert:
                        System.Console.WriteLine($"Converting {config.Input}...");

                        switch (config.Format) {
                            case Parameters.FormatJson: // Convert to JSON
                                DitaToJsonConverter converter = new DitaToJsonConverter();
                                if (converter.Convert(config.Input, config.Output, config.Rename)) {
                                    return 0;
                                }
                                break;

                            default:
                                System.Console.WriteLine($"Unknown output format {config.Format}");
                                break;
                        }

                        return -1;

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