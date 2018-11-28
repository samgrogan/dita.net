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

                // Is there a bookmap?
                List<DitaBookMap> bookMaps = collection.GetBookMaps();
                if (bookMaps.Count != 1) {
                    throw new Exception($"Expecting exactly 1 bookmap, but found {bookMaps.Count}");
                }

                // Try renaming the files in the collection, if requested
                if (rename) {
                    collection.RenameFiles();
                }

                return true;
            }
            catch {
                return false;
            }
        }
    }
}