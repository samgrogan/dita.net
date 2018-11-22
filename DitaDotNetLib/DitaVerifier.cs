using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;

namespace Dita.Net {
    public class DitaVerifier {
        // Try to verify the given file or directory
        public bool VerifyFileOrDirectory(string strPath) {
            // Is this a directory
            if (Directory.Exists(strPath)) {
                return VerifyDirectory(strPath);
            }

            if (File.Exists(strPath)) {
                return VerifyFile(strPath, true);
            }

            return false;
        }

        // Verify all of the DITA files in the given directory
        // Returns true if *any* file in the directory is valid DITA (map or topic)
        public bool VerifyDirectory(string strPath) {
            // Get a list of all the files in the directory
            string[] files = Directory.GetFiles(strPath);
            if (files.Length > 0) {
                Console.WriteLine($"Verifying {files.Length} files...");

                int validCount = 0;
                foreach (string file in files) {
                    if (VerifyFile(file)) {
                        validCount++;
                    }
                }

                Console.WriteLine($"Found {validCount} valid DITA files.");

                return (validCount > 0);
            }

            Console.WriteLine($"No files found in directory {strPath}");
            return false;
        }

        // Verify a single DITA file
        public bool VerifyFile(string filePath, bool recursive = false) {
            try {
                // Try to load the given file
                XmlDocument xmlDocument = DitaFile.LoadAndCheckType(filePath, out DitaFileType fileType);

                switch (fileType) {
                    case DitaFileType.BookMap:
                        DitaBookMap ditaBookMap = new DitaBookMap(xmlDocument, filePath);
                        Console.WriteLine($"{Path.GetFileName(filePath)} is a {DitaFileType.BookMap}");
                        return true;


                    case DitaFileType.Map:
                        DitaMap ditaMap = new DitaMap(xmlDocument, filePath);
                        Console.WriteLine($"{Path.GetFileName(filePath)} is a {DitaFileType.Map}");
                        return true;

                    case DitaFileType.Topic:
                        DitaTopic ditaTopic = new DitaTopic(xmlDocument, filePath);
                        Console.WriteLine($"{Path.GetFileName(filePath)} is a {DitaFileType.Topic}");
                        return true;
                }
            }
            catch (Exception ex) {
                Console.WriteLine($"Unable to parse {System.IO.Path.GetFileName(filePath)}.");
            }

            return false;
        }
    }
}