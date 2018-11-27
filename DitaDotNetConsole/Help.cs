using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Dita.Net.Console {
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