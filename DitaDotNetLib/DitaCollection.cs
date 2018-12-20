using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

// Group together a collection of dita files

namespace Dita.Net {
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
                Console.WriteLine($"Checking {files.Length} files...");

                foreach (string file in files) {
                    try {
                        DitaFile ditaFile = LoadFile(file);
                        Files.Add(ditaFile);
                    }
                    catch {
                        Console.WriteLine($"Unable to load file {file}");
                    }
                }

                Console.WriteLine($"Found {FileCount} valid DITA files.");
            }
            else {
                Console.WriteLine($"No files found in directory {input}");
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
                        Console.WriteLine($"{Path.GetFileName(filePath)} is a {DitaFileType.BookMap}");
                        return ditaBookMap;

                    case DitaFileType.Map:
                        DitaMap ditaMap = new DitaMap(xmlDocument, filePath);
                        Console.WriteLine($"{Path.GetFileName(filePath)} is a {DitaFileType.Map}");
                        return ditaMap;

                    case DitaFileType.Topic:
                        DitaTopic ditaTopic = new DitaTopic(xmlDocument, filePath);
                        Console.WriteLine($"{Path.GetFileName(filePath)} is a {DitaFileType.Topic}");
                        return ditaTopic;

                    case DitaFileType.Concept:
                        DitaConcept ditaConcept = new DitaConcept(xmlDocument, filePath);
                        Console.WriteLine($"{Path.GetFileName(filePath)} is a {DitaFileType.Concept}");
                        return ditaConcept;
                }
            }
            catch {
                Console.WriteLine($"Unable to load {filePath} as XML.");
            }

            // if it is not an xml document, is it an image?
            // Is this an svg?
            if (Path.HasExtension(filePath)) {
                string extension = Path.GetExtension(filePath)?.ToLower();
                if (!string.IsNullOrWhiteSpace(extension)) {
                    if (DitaSvg.Extensions.Contains(extension)) {
                        DitaSvg ditaSvg = new DitaSvg(filePath);
                        Console.WriteLine($"{Path.GetFileName(filePath)} is a {DitaFileType.Svg}");
                        return ditaSvg;
                    }
                }
            }

            throw new Exception($"{filePath} is an unknown file type.");
        }

        // Returns the list of Dita Book Maps in the collection
        public List<DitaBookMap> GetBookMaps() {
            List<DitaBookMap> result = new List<DitaBookMap>();

            foreach (DitaFile file in Files) {
                if (file is DitaBookMap ditaBookMap) {
                    result.Add(ditaBookMap);
                }
            }

            return result;
        }

        // Rename the files in the collection to match their title (instead of their given file names)
        public void RenameFiles() {
            List<string> fileNames = new List<string>();

            // Generate a new name for each file, based on it's title
            foreach (DitaFile file in Files) {
                string newFileName = DitaFile.TitleToFileName(file.GetTitle(), Path.GetExtension(file.FileName));
                if (!string.IsNullOrWhiteSpace(newFileName)) {
                    while (fileNames.Contains(newFileName)) {
                        newFileName = Path.ChangeExtension($"{Path.GetFileNameWithoutExtension(newFileName)}0", Path.GetExtension(newFileName));
                    }
                    fileNames.Add(newFileName);
                    file.NewFileName = newFileName;
                    Console.WriteLine($"Renaming {file.FileName} to {newFileName}");
                }
            }

            // Ensure that all filenames are unique
            //if (!AreFileNamesUnique()) {
            //    throw new Exception("File names (titles) are not unique.");
            //}

            // Update references from old to new file names
            UpdateReferences();
        }

        // Gets a file in the collection with the given name
        public DitaFile GetFileByName(string fileName) {
            return Files.FirstOrDefault((file) => (file.NewFileName ?? file.FileName) == fileName);
        }

        #endregion Public Methods

        #region Internal Methods

        // Are all of the files in the collection uniquely named?
        //protected bool AreFileNamesUnique() {
        //    bool result = true;

        //    List<string> fileNames = new List<string>();

        //    foreach (DitaFile file in Files) {
        //        string fileName = file.NewFileName ?? file.FileName;

        //        if (fileName == null) {
        //            throw new Exception("Null file names are not allowed.");
        //        }

        //        if (fileNames.Contains(fileName)) {
        //            Console.WriteLine($"Found duplicate file name {fileName}.");
        //            result = false;
        //        }
        //        else {
        //            fileNames.Add(fileName);
        //        }
        //    }

        //    return result;
        //}

        // Update all references in the collection from old file name to new file name
        protected void UpdateReferences() {
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