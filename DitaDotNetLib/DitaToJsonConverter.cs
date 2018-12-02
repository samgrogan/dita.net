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

        public new bool Convert(string input, string output, bool rename = false, PageMapping pageMapping = PageMapping.TopicToPage) {
            if (base.Convert(input, output, rename)) {

                try {
                    // Write out the json table of contents
                    DitaContentsJson collectionJson = WriteContentsJson(output);

                    // Write out the pages json
                    switch ()

                }
                catch {
                    Console.WriteLine($"Error converting {input} to JSON.");
                    return false;
                }
                return true;
            }

            return false;
        }

        internal DitaContentsJson WriteContentsJson(string output) {
            DitaContentsJson collectionJson;

            // Find the bookmap
            List<DitaBookMap> bookMaps = Collection?.GetBookMaps();
            if (bookMaps?.Count == 1) {

                // Create the output object
                collectionJson = ConvertBookMapToContentsJson(bookMaps[0]);

                // Try to serialize the collection
                SerializeCollectionJsonToFile(collectionJson, output, "contents.json");
                Console.WriteLine($"Wrote contents.json");
            }
            else {
                throw new Exception($"Found {bookMaps?.Count} bookmaps instead of 1.");
            }

            return collectionJson;
        }

        // Create a contents object from a bookmap
        internal DitaContentsJson ConvertBookMapToContentsJson(DitaBookMap bookMap) {
            DitaContentsJson contentsJson = new DitaContentsJson {
                Properties = new Dictionary<string, string>(),
                Chapters = new List<DitaContentsLinkJson>()
            };

            // Write out the book metadata



            return contentsJson;
        }

        // Write the json collection to a file
        internal void SerializeContentsJsonToFile(DitaContentsJson collectionJson, string output, string fileName) {
            using (StreamWriter file = File.CreateText(Path.Combine(output, fileName))) {
                JsonSerializerSettings settings = new JsonSerializerSettings();
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, collectionJson);
            }
        }

        // Converts a DitaElement to DitaPropertyJson for output
        internal DitaPropertyJson CreatePropertyJson(DitaElement ditaElement) {
            DitaPropertyJson propertyJson = new DitaPropertyJson {
                Properties = new Dictionary<string, object>()
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