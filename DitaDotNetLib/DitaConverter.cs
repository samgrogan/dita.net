using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dita.Net {
    public class DitaConverter {

        // The main conversion action
        public bool Convert(string input, string output, bool rename = false) {
            throw new NotImplementedException();
        }

        // Validate that the output folder exists, and create it if it doesn't
        public void VerifyOutputPath(string output) {
            if (!Directory.Exists(output)) {
                Directory.CreateDirectory(output);
            }
        }
    }
}