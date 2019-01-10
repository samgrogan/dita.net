using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Dita.Net {

    public class DitaToJsonConverter : DitaConverter {

        public new bool Convert(string input, string output, bool rename = false) {
            if (base.Convert(input, output, rename)) {

                try {
                    // Write out the json table of contents
                    DitaCollectionJson collectionJson = new DitaCollectionJson(Collection, BookMap);
                    collectionJson.SerializeToFile(output);

                    // Write out the pages json
                    foreach (DitaPageJson page in collectionJson.Pages) {
                        page.SerializeToFile(output);
                    }

                    // Write out the search json
                    DitaSearchJson searchJson = new DitaSearchJson(collectionJson.Pages);
                    searchJson.SerializeToFile(output);

                    // Copy the images
                    CopyImages(input, output, collectionJson);
                }
                catch {
                    Console.WriteLine($"Error converting {input} to JSON.");
                    return false;
                }
                return true;
            }

            return false;
        }

        // Copies all of the images input to the output folder
        private void CopyImages(string input, string output, DitaCollectionJson collectionJson) {
            List<DitaSvg> images = collectionJson.Images;

            foreach (DitaSvg image in images) {
                string imageOutputPath = Path.Combine(output, image.FileName);

                File.Copy(image.FilePath, imageOutputPath, true);
                Console.WriteLine($"Copied {image.FilePath} to {imageOutputPath}");
            }
        }
    }
}