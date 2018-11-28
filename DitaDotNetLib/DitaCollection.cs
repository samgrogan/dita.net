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
            foreach (DitaFile file in Files) {
                string title = file.GetTitle();
                if (!string.IsNullOrWhiteSpace(title)) {
                    Console.WriteLine($"Renaming {file.FileName} to {title}.{Path.GetExtension(file.FileName)}");
                }
            }
        }

        #endregion Public Methods

        #region Internal Methods

        #endregion
    }
}