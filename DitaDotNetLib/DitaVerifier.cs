using System.IO;

namespace DitaDotNet {
    public class DitaVerifier {
        // Try to verify the given file or directory
        public bool VerifyFileOrDirectory(string strPath) {
            // Is this a directory
            if (Directory.Exists(strPath)) {
                return VerifyDirectory(strPath);
            }

            return false;
        }

        // Verify all of the DITA files in the given directory
        // Returns true if *any* file in the directory is valid DITA (map or topic)
        public bool VerifyDirectory(string input) {

            try {
                // Try to load all of the input files
                DitaCollection collection = new DitaCollection();
                collection.LoadDirectory(input);

                return true;
            }
            catch {
                return false;
            }
        }

    }
}