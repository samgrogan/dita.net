using System;
using System.Collections.Generic;
using System.IO;

namespace DitaDotNet {

    public class DitaToJsonConverter : DitaConverter {

        // Name of the images folder
        private readonly string ImagesFolderName = "images";

        public new bool Convert(string input, string output, bool rename = false, bool deleteExistingOutput = false) {
            if (base.Convert(input, output, rename)) {

                try {
                    // Delete and existing output, if asked
                    if (deleteExistingOutput) {
                        DeleteOutputFiles(Path.Combine(output, ImagesFolderName));
                    }

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
                    Trace.TraceError($"Error converting {input} to JSON.");
                    return false;
                }
                return true;
            }

            return false;
        }

        // Copies all of the images input to the output folder
        private void CopyImages(string input, string output, DitaCollectionJson collectionJson) {
            List<DitaImage> images = collectionJson.Images;

            // Create the images folder, if needed
            string imagesOutputFolder = Path.Combine(output, ImagesFolderName);
            try {
                if (!Directory.Exists(imagesOutputFolder)) {
                    Directory.CreateDirectory(imagesOutputFolder);
                }
            }
            catch {
                Trace.TraceError($"Error trying to create images folder: {imagesOutputFolder}.");
            }

            foreach (DitaImage image in images) {
                string imageOutputPath = Path.Combine(imagesOutputFolder, image.FileName);

                try {
                    File.Copy(image.FilePath, imageOutputPath, true);
                }
                catch {
                    Trace.TraceError($"Error trying to copy {image.FilePath} to {imageOutputPath}");
                }
                Trace.TraceInformation($"Copied {image.FilePath} to {imageOutputPath}");
            }
        }
    }
}