using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Dita.Net {
    public class DitaConcept : DitaTopic {
        #region Class Methods

        // Default constructor
        public DitaConcept(XmlDocument xmlDocument, string filePath) : base(xmlDocument, filePath) {
            // Try to parse the file as a <type>
            if (!Parse()) {
                throw new Exception($"{FileName} is not parseable as a {this.GetType()}");
            }
        }

        public new bool Parse() {
            if (Parse("//concept", "Concept")) {
                return true;
            }

            return false;
        }

        #endregion Class Methods

        #region Static Methods

        // Does the given DOCTYPE match this object?
        public new static bool IsMatchingDocType(string docType) {
            if (!string.IsNullOrWhiteSpace(docType)) {
                return (docType.Contains("concept") && docType.Contains("concept.dtd") && docType.Contains("-//OASIS//DTD DITA Concept//"));
            }

            return false;
        }

        #endregion Static Methods
    }
}