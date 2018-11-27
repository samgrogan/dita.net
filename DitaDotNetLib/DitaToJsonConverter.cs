using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dita.Net {
    public class DitaToJsonConverter : DitaConverter {

        public new bool Convert(string input, string output, bool rename = false) {

            try {
                // Make sure the output path exists
                VerifyOutputPath(output);

                // Try to load all of the input files
                DitaCollection collection = new DitaCollection();
                collection.LoadDirectory(input);

                return true;
            }
            catch {
                return false;
            }
        }
    }
}