using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Dita.Net {

    internal class DitaCollectionJson {
        public Dictionary<string, DitaPropertyJson> Properties { get; set; }
        public List<DitaTocLinkJson> Chapters { get; set; }
    }

    internal class DitaPropertyJson {
        public Dictionary<string, string> Properties { get; set; }
        public List<DitaPropertyJson> Children { get; set; }
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
                try {
                    DitaCollectionJson collectionJson = CreateCollectionJson();

                    // Try to serialize the collection
                    SerializeCollectionJsonToFile(collectionJson, output, "contents.json");
                    Console.WriteLine($"Wrote contents.json");
                }
                catch {
                    Console.WriteLine($"Error converting {input} to JSON.");
                    return false;
                }
                return true;
            }

            return false;
        }

        internal DitaCollectionJson CreateCollectionJson() {
            DitaCollectionJson collectionJson;

            // Find the bookmap
            List<DitaBookMap> bookMaps = Collection?.GetBookMaps();
            if (bookMaps?.Count == 1) {
                collectionJson = CreateCollectionJson(bookMaps[0]);
            }
            else {
                throw new Exception($"Found {bookMaps?.Count} bookmaps instead of 1.");
            }

            return collectionJson;
        }

        // Create a collection from a bookmap
        internal DitaCollectionJson CreateCollectionJson(DitaBookMap bookMap) {
            DitaCollectionJson collectionJson = new DitaCollectionJson {
                Properties = new Dictionary<string, DitaPropertyJson>(),
                Chapters = new List<DitaTocLinkJson>()
            };

            // Loop through all the top level elements that aren't chapters
            foreach (DitaElement ditaElement in bookMap.RootElement.Children) {
                collectionJson.Properties.Add(ditaElement.Type, CreatePropertyJson(ditaElement));
            }

            return collectionJson;
        }

        // Write the json collection to a file
        internal void SerializeCollectionJsonToFile(DitaCollectionJson collectionJson, string output, string fileName) {
            using (StreamWriter file = File.CreateText(Path.Combine(output, fileName))) {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, collectionJson);
            }
        }

        // Converts a DitaElement to DitaPropertyJson for output
        internal DitaPropertyJson CreatePropertyJson(DitaElement ditaElement) {
            DitaPropertyJson propertyJson = new DitaPropertyJson {
                Properties = new Dictionary<string, string>()
            };

            if (ditaElement.IsContainer) {
                propertyJson.Properties.Add("type", ditaElement.Type);
                propertyJson.Children = new List<DitaPropertyJson>();

                foreach (DitaElement child in ditaElement.Children) {
                    propertyJson.Children.Add(CreatePropertyJson(child));
                }
            }
            else {
                propertyJson.Properties.Add(ditaElement.Type, ditaElement.InnerText);
            }

            return propertyJson;
        }
    }
}