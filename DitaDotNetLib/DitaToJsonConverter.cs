using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Dita.Net {

    public class DitaToJsonConverter : DitaConverter {

        private const string SearchFileName = "search.json";

        public new bool Convert(string input, string output, bool rename = false, PageMapping pageMapping = PageMapping.TopicToPage) {
            if (base.Convert(input, output, rename)) {

                try {
                    // Write out the json table of contents
                    DitaCollectionJson collectionJson = new DitaCollectionJson(Collection, BookMap, pageMapping);
                    collectionJson.SerializeToFile(output);

                    // Write out the pages json
                    List<DitaPageJson> pages = null;
                    switch (pageMapping) {
                        case PageMapping.MapToPage:
                            break;
                        case PageMapping.TopicToPage:
                        default:
                            break;
                    }

                    // Write out the search json

                }
                catch {
                    Console.WriteLine($"Error converting {input} to JSON.");
                    return false;
                }
                return true;
            }

            return false;
        }
    }
}