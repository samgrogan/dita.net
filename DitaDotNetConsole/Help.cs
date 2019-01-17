using System.IO;

namespace DitaDotNet.Console {
    class Help {
        // Writes out the command line help
        public void WriteHelpToConsole() {
            try {
                string helpText = File.ReadAllText("Help.txt");
                System.Console.WriteLine(helpText);
            }
            catch {
                System.Console.WriteLine("Error trying to read help file help.txt");
            }
        }
    }
}