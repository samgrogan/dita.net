using System;
using System.Collections.Generic;
using System.IO;

namespace DitaDotNet {
    public class DitaConverter {
        // The collection of DITA content to convert
        protected DitaCollection Collection { get; set; }

        // The bookmap that is the root of the collection
        protected DitaBookMap BookMap { get; set; }

        // The main conversion action
        public bool Convert(string input, string output, bool rename = false) {
            try {
                // Make sure the output path exists
                VerifyOutputPath(output);

                // Try to load all of the input files
                Collection = new DitaCollection();
                Collection.LoadDirectory(input);

                // Is there a bookmap?
                List<DitaBookMap> bookMaps = Collection.GetBookMaps();
                if (bookMaps.Count == 1) {
                    BookMap = bookMaps[0];
                }
                else {
                    throw new Exception($"Expecting exactly 1 bookmap, but found {bookMaps.Count}");
                }

                // Try renaming the files in the collection, if requested
                if (rename) {
                    Collection.RenameFiles();
                }

                return true;
            }
            catch (Exception ex) {
                Trace.TraceError(ex);

                return false;
            }
        }

        // Validate that the output folder exists, and create it if it doesn't
        public void VerifyOutputPath(string output) {
            if (!Directory.Exists(output)) {
                Directory.CreateDirectory(output);
            }
        }
    }
}