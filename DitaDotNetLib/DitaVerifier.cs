using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Dita.Net
{
    public class DitaVerifier
    {
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
        public bool VerifyFile(string strPath, bool recursive = false) {
            try {
                // Try to load the given file
                DitaFile ditaFile = new DitaFile();
                ditaFile.Load(strPath);

                // Try to determine the type of the file
                DitaFile.DitaFileType fileType = ditaFile.InspectFileType();

                switch (fileType) {
                    case DitaFile.DitaFileType.BookMap:
                        DitaBookMap ditaBookMap = new DitaBookMap(ditaFile);

                        Console.WriteLine($"{Path.GetFileName(strPath)} is a {DitaFile.DitaFileType.BookMap}");
                        return true;

                    case DitaFile.DitaFileType.Map:
                        DitaMap ditaMap = new DitaMap(ditaFile);

                        Console.WriteLine($"{Path.GetFileName(strPath)} is a {DitaFile.DitaFileType.Map}");
                        return true;

                    case DitaFile.DitaFileType.Topic:
                        DitaTopic ditaTopic = new DitaTopic(ditaFile);

                        Console.WriteLine($"{Path.GetFileName(strPath)} is a {DitaFile.DitaFileType.Topic}");
                        return true;
                }
            }
            catch {
                Console.WriteLine($"Unable to parse {strPath}.");
            }

            return false;
        }
    }
}
