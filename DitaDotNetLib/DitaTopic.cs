using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Dita.Net
{
    public class DitaTopic : DitaFile 
    {
        #region Members

        // The Dita File we were created from
        private readonly DitaFile _ditaFile;

        #endregion Members

        #region Class Methods

        // Default constructor
        public DitaTopic(DitaFile ditaFile) : base() {
            _ditaFile = ditaFile;
        }

        public new bool Parse() {

            try {
                // Try to find the topic node in the document
                XmlNodeList queryNodes = _xmlDocument?.DocumentElement?.SelectNodes("//topic");
                if (queryNodes != null) {
                    throw new Exception("Topic node not found.");
                }

                // Create and populate the root node

                // Populate the child nodes

                return true;
            }
            catch (Exception ex) {
                Console.WriteLine(ex);
                Console.WriteLine($"Error parsing topic in {System.IO.Path.GetFileName(_ditaFile?.Path)}");
            }

            return false;
        }

        #endregion Class Methods

        #region Static Methods

        // Does the given DOCTYPE match this object?
        public new static bool IsMatchingDocType(string docType) {
            if (!string.IsNullOrWhiteSpace(docType)) {
                return (docType.Contains("topic") && docType.Contains("topic.dtd") && docType.Contains("-//OASIS//DTD DITA Topic//"));
            }
            return false;
        }

        #endregion Static Methods

    }
}
