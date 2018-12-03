using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Dita.Net {

    public enum PageMapping {
        Unknown = 0,
        TopicToPage = 1,
        MapToPage = 2
    }

    public class DitaToJsonConverter : DitaConverter {

        private const string SearchFileName = "search.json";

        public new bool Convert(string input, string output, bool rename = false, PageMapping pageMapping = PageMapping.TopicToPage) {
            if (base.Convert(input, output, rename)) {

                try {
                    // Write out the json table of contents
                    DitaCollectionJson collectionJson = new DitaCollectionJson(Collection);
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