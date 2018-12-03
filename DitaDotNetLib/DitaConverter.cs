using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dita.Net {

    public enum PageMapping {
        Unknown = 0,
        TopicToPage = 1,
        MapToPage = 2
    }
    public class DitaConverter {

        // The collection of DITA content to convert
        protected DitaCollection Collection { get; set; }

        // The bookmap that is the root of the collection
        protected DitaBookMap BookMap { get; set; }

        // The main conversion action
        public bool Convert(string input, string output, bool rename = false, PageMapping pageMapping = PageMapping.TopicToPage) {
            try {
                // Make sure the output path exists
                VerifyOutputPath(output);

                // Try to load all of the input files
                Collection = new DitaCollection();
                Collection.LoadDirectory(input);

                // Is there a bookmap?
                List<DitaBookMap> bookMaps = Collection.GetBookMaps();
                if (bookMaps.Count == 1)
                {
                    BookMap = bookMaps[0];
                } else { 
                    throw new Exception($"Expecting exactly 1 bookmap, but found {bookMaps.Count}");
                }

                // Try renaming the files in the collection, if requested
                if (rename) {
                    Collection.RenameFiles();
                }

                return true;
            }
            catch (Exception exception) {
                Console.WriteLine(exception.ToString());

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