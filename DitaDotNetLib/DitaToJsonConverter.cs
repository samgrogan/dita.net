using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dita.Net {

    internal class DitaCollectionJson {
        public List<Dictionary<string, string>> BookTitle { get; set; }


        public List<DitaTocLinkJson> Chapters;
    }

    internal class DitaTocLinkJson {
        public string Title { get; set; }
        public string Link { get; set; }
        public  List<DitaTocLinkJson> Children { get; set; }
    }


    public class DitaToJsonConverter : DitaConverter {

        public new bool Convert(string input, string output, bool rename = false) {
            if (base.Convert(input, output, rename)) {

                // Write out the json table of contents


                return true;
            }

            return false;
        }

        protected DitaCollectionJson CreateCollectionOutput(string output) {
            DitaCollectionJson collectionOutput = new DitaCollectionJson();

            // Find the bookmap
            List<DitaBookMap> bookMaps = Collection?.GetBookMaps();
            if (bookMaps?.Count == 1) {

                // 


            }
            else {
                Console.WriteLine($"Found {bookMaps?.Count} bookmaps instead of 1.");
            }
            return false;
        }

        // Collapse a series of dita bookmaps / maps to a list of titles and links

    }
}