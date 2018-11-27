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
            XmlDocument xmlDocument = DitaFile.LoadAndCheckType(filePath, out DitaFileType fileType);

            switch (fileType)
            {
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

        #endregion Public Methods

        #region Internal Methods



        #endregion
    }
}