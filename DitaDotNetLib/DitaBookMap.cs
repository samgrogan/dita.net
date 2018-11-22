using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Dita.Net {
    class DitaBookMap : DitaFile {
        #region Class Methods

        // Default constructor
        public DitaBookMap(XmlDocument xmlDocument, string filePath) : base(xmlDocument, filePath) {
            // Try to parse the file as a <type>
            if (!Parse()) {
                throw new Exception($"{FileName} is not parseable as a {this.GetType()}");
            }
        }

        protected new bool Parse() {
            if (Parse("//bookmap", "Map")) {
                return true;
            }
            return false;
        }

        #endregion

        #region Static Members

        // Does the given DOCTYPE match this object?
        public new static bool IsMatchingDocType(string docType) {
            if (!string.IsNullOrWhiteSpace(docType)) {
                return (docType.Contains("bookmap") && docType.Contains("bookmap.dtd") && docType.Contains("-//OASIS//DTD DITA BookMap//"));
            }

            return false;
        }

        #endregion
    }
}