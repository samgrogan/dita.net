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
                Trace.TraceInformation($"- {GetConcepts().Count} concepts.");
                Trace.TraceInformation($"- {GetImages().Count} images.");
            }
            else {
                Trace.TraceWarning($"No files found in directory {input}");
            }
        }

        // Load a single file
        public DitaFile LoadFile(string filePath) {
            // Try to load as an XML document
            // Try to load the given file

            try {
                XmlDocument xmlDocument = DitaFile.LoadAndCheckType(filePath, out DitaFileType fileType);

                switch (fileType) {
                    case DitaFileType.BookMap:
                        DitaBookMap ditaBookMap = new DitaBookMap(xmlDocument, filePath);
                        Trace.TraceInformation($"{Path.GetFileName(filePath)} is a {DitaFileType.BookMap}");
                        return ditaBookMap;

                    case DitaFileType.Map:
                        DitaMap ditaMap = new DitaMap(xmlDocument, filePath);
                        Trace.TraceInformation($"{Path.GetFileName(filePath)} is a {DitaFileType.Map}");
                        return ditaMap;

                    case DitaFileType.Topic:
                        DitaTopic ditaTopic = new DitaTopic(xmlDocument, filePath);
                        Trace.TraceInformation($"{Path.GetFileName(filePath)} is a {DitaFileType.Topic}");
                        return ditaTopic;

                    case DitaFileType.Concept:
                        DitaConcept ditaConcept = new DitaConcept(xmlDocument, filePath);
                        Trace.TraceInformation($"{Path.GetFileName(filePath)} is a {DitaFileType.Concept}");
                        return ditaConcept;
                }
            }
            catch {
                Trace.TraceWarning($"Unable to load {filePath} as XML.");
            }

            // if it is not an xml document, is it an image?
            // Is this an svg?
            if (Path.HasExtension(filePath)) {
                string extension = Path.GetExtension(filePath)?.ToLower();
                if (!string.IsNullOrWhiteSpace(extension)) {
                    if (DitaSvg.Extensions.Contains(extension)) {
                        DitaSvg ditaSvg = new DitaSvg(filePath);
                        Trace.TraceInformation($"{Path.GetFileName(filePath)} is a {DitaFileType.Svg}");
                        return ditaSvg;
                    }
                }
            }

            throw new Exception($"{filePath} is an unknown file type.");
        }

        // Returns the list of Dita Book Maps in the collection
        public List<DitaBookMap> GetBookMaps() {
            List<DitaBookMap> results = new List<DitaBookMap>();

            foreach (DitaFile file in Files) {
                if (file is DitaBookMap ditaBookMap) {
                    results.Add(ditaBookMap);
                }
            }

            return results;
        }

        // Returns the list of Dita Maps in the collection
        public List<DitaMap> GetMaps() {
            List<DitaMap> results = new List<DitaMap>();

            foreach (DitaFile file in Files) {
                if (file is DitaMap ditaMap) {
                    results.Add(ditaMap);
                }
            }

            return results;
        }

        // Returns a list of the images in the collection
        public List<DitaImage> GetImages() {
            List<DitaImage> results = new List<DitaImage>();

            foreach (DitaFile file in Files) {
                if (file is DitaImage ditaImage) {
                    results.Add(ditaImage);
                }
            }

            return results;
        }

        // Returns a list of the topics in the collection
        public List<DitaTopic> GetTopics() {
            List<DitaTopic> results = new List<DitaTopic>();

            foreach (DitaFile file in Files) {
                if (file is DitaTopic ditaTopic) {
                    results.Add(ditaTopic);
                }
            }

            return results;
        }

        // Returns a list of the concepts in the collection
        public List<DitaConcept> GetConcepts() {
            List<DitaConcept> results = new List<DitaConcept>();

            foreach (DitaFile file in Files) {
                if (file is DitaConcept ditaConcept) {
                    results.Add(ditaConcept);
                }
            }

            return results;
        }

        // Rename the files in the collection to match their title (instead of their given file names)
        public void RenameFiles() {
            List<string> fileNames = new List<string>();

            // Generate a new name for each file, based on it's title
            foreach (DitaFile file in Files) {
                string newFileName = DitaFile.TitleToFileName(file.GetTitle(), Path.GetExtension(file.FileName));
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
            return Files.FirstOrDefault((file) => (file.NewFileName ?? file.FileName) == fileName);
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