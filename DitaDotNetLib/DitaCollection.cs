using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

// Group together a collection of dita files

namespace DitaDotNet {
    public class DitaCollection {
        #region Members

        // Holds the collection of dita files
        protected List<DitaFile> Files;

        #endregion

        #region Properties

        public int FileCount => Files.Count;

        #endregion

        #region Public Methods

        public DitaCollection() {
            Files = new List<DitaFile>();
        }

        // Loads all of the DITA files and supports from the given directory
        public void LoadDirectory(string input) {
            // Get a list of all the files in the directory
            string[] files = Directory.GetFiles(input);
            if (files.Length > 0) {
                Trace.TraceInformation($"Checking {files.Length} files...");

                foreach (string file in files) {
                    try {
                        DitaFile ditaFile = LoadFile(file);
                        Files.Add(ditaFile);
                    }
                    catch {
                        Trace.TraceWarning($"Unable to load file {file}");
                    }
                }

                Trace.TraceInformation($"Found {FileCount} valid DITA files.");
                Trace.TraceInformation($"- {GetBookMaps().Count} bookmaps.");
                Trace.TraceInformation($"- {GetMaps().Count} maps.");
                Trace.TraceInformation($"- {GetTopics().Count} topics.");
                Trace.TraceInformation($"- {GetImages().Count} images.");
            }
            else {
                Trace.TraceWarning($"No files found in directory {input}");
            }
        }

        // Load a single file
        public DitaFile LoadFile(string filePath) {
            // Look for known extensions
            // Is this an image?
            if (Path.HasExtension(filePath)) {
                string extension = Path.GetExtension(filePath)?.ToLower();
                if (DitaFileImage.Extensions.Contains(extension)) {
                    DitaFileImage image = new DitaFileImage(filePath);
                    Trace.TraceInformation($"{Path.GetFileName(filePath)} is a {typeof(DitaFileImage)}");
                    return image;
                }
            }

            // Try to load the given file

            try {
                // Try to load as an XML document
                XmlDocument xmlDocument = DitaFile.LoadAndCheckType(filePath, out Type fileType);

                // Create a new object of the correct type
                if (DitaFile.DitaFileTypeCreation.ContainsKey(fileType)) {
                    return DitaFile.DitaFileTypeCreation[fileType](xmlDocument, filePath);
                }
            }
            catch {
                Trace.TraceWarning($"Unable to load {filePath} as XML.");
            }

            throw new Exception($"{filePath} is an unknown file type.");
        }

        // Returns the list of Dita Book Maps in the collection
        public List<DitaFileBookMap> GetBookMaps() {
            List<DitaFileBookMap> results = new List<DitaFileBookMap>();

            foreach (DitaFile file in Files) {
                if (file is DitaFileBookMap ditaBookMap) {
                    results.Add(ditaBookMap);
                }
            }

            return results;
        }

        // Returns the list of Dita Maps in the collection
        public List<DitaFileMap> GetMaps() {
            List<DitaFileMap> results = new List<DitaFileMap>();

            foreach (DitaFile file in Files) {
                if (file is DitaFileMap ditaMap) {
                    results.Add(ditaMap);
                }
            }

            return results;
        }

        // Returns a list of the images in the collection
        public List<DitaFileImage> GetImages() {
            List<DitaFileImage> results = new List<DitaFileImage>();

            foreach (DitaFile file in Files) {
                if (file is DitaFileImage ditaImage) {
                    results.Add(ditaImage);
                }
            }

            return results;
        }

        // Returns a list of the topics in the collection
        public List<DitaFileTopicAbstract> GetTopics() {
            List<DitaFileTopicAbstract> results = new List<DitaFileTopicAbstract>();

            foreach (DitaFile file in Files) {
                if (file is DitaFileTopicAbstract ditaTopic) {
                    results.Add(ditaTopic);
                }
            }

            return results;
        }

        // Rename the files in the collection to match their title (instead of their given file names)
        public void RenameFiles() {
            List<string> fileNames = new List<string>();

            // Generate a new name for each file, based on it's title
            foreach (DitaFile file in Files) {
                string newFileName = DitaFile.TitleToFileName(file.Title, Path.GetExtension(file.FileName));
                if (!string.IsNullOrWhiteSpace(newFileName)) {
                    if (fileNames.Contains(newFileName)) {
                        string newFileNameBase = newFileName;
                        int counter = 1;
                        while (fileNames.Contains(newFileName)) {
                            newFileName = Path.ChangeExtension($"{Path.GetFileNameWithoutExtension(newFileNameBase)}_{counter}", Path.GetExtension(newFileNameBase));
                            counter++;
                        }
                    }

                    fileNames.Add(newFileName);
                    file.NewFileName = newFileName;
                    Trace.TraceInformation($"Renaming {file.FileName} to {newFileName}");
                }
            }

            // Update references from old to new file names
            UpdateReferences();
        }

        // Gets a file in the collection with the given name
        public DitaFile GetFileByName(string fileName) {
            return Files.FirstOrDefault((file) => (file.FileName == fileName || file.NewFileName == fileName || Path.GetFileNameWithoutExtension(file.FileName) == fileName));
        }

        #endregion Public Methods

        #region Private Methods

        // Update all references in the collection from old file name to new file name
        private void UpdateReferences() {
            // Loop through each file and update references if the file has changed
            Parallel.ForEach(Files, (fileRenamed) => {
                if (fileRenamed.FileName != fileRenamed.NewFileName && !string.IsNullOrWhiteSpace(fileRenamed.NewFileName)) {
                    // Change the references in this file from old to new
                    Parallel.ForEach(Files, (file) => { file.RootElement?.UpdateReferences(fileRenamed.FileName, fileRenamed.NewFileName); });
                }
            });
        }

        #endregion
    }
}