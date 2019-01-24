using System;
using System.Collections.Generic;
using System.IO;

namespace DitaDotNet {
    public class DitaConverter {
        // The collection of DITA content to convert
        protected DitaCollection Collection { get; set; }

        // The bookmap/amp that is the root of the collection
        protected DitaFile RootMap { get; set; }

        // The main conversion action
        public bool Convert(string input, string output, string rootMapFile, bool rename = false, bool deleteExistingOutput = false) {
            try {
                // Make sure the output path exists
                VerifyOutputPath(output);

                // Try to load all of the input files
                Collection = new DitaCollection();
                Collection.LoadDirectory(input);

                // Find the rootmap
                FindRootMap(rootMapFile);

                // Try renaming the files in the collection, if requested
                if (rename) {
                    Collection.RenameFiles();
                }

                // Delete and existing output, if asked
                if (deleteExistingOutput) {
                    DeleteOutputFiles(output);
                }

                return true;
            }
            catch (Exception ex) {
                Trace.TraceError(ex);

                return false;
            }
        }

        // Validate that the output folder exists, and create it if it doesn't
        private void VerifyOutputPath(string output) {
            if (!Directory.Exists(output)) {
                Directory.CreateDirectory(output);
            }
        }

        // Delete existing output files
        protected void DeleteOutputFiles(string output) {
            try {
                if (Directory.Exists(output)) {
                    string[] files = Directory.GetFiles(output);
                    foreach (string file in files) {
                        File.Delete(file);
                        Trace.TraceInformation($"Deleted output file {file}.");
                    }
                }
            }
            catch (Exception ex) {
                Trace.TraceError($"Error deleting output files in {output}");
                Trace.TraceError(ex);
            }
        }

        // Try to find the root map file
        private void FindRootMap(string rootMapFile) {
            // If no file name specified, try to find a bookmap
            if (string.IsNullOrEmpty(rootMapFile)) {
                List<DitaBookMap> bookMaps = Collection.GetBookMaps();
                if (bookMaps.Count == 1) {
                    RootMap = bookMaps[0];
                    return;
                }
                throw new Exception($"Expecting exactly 1 bookmap, but found {bookMaps.Count}");
            }

            // Is there a bookmap or map with the given name?
            DitaFile rootFile = Collection.GetFileByName(rootMapFile);
            if (rootFile == null) {
                throw new Exception($"Specified root map file {rootMapFile} was not found in collection.");
            }

            switch (rootFile) {
                case DitaBookMap bookMap:
                    RootMap = bookMap;
                    break;
                case DitaMap map:
                    RootMap = map;
                    break;
                default:
                    throw new Exception($"{rootMapFile} must be a map or bookmap.");
            }
        }
    }
}