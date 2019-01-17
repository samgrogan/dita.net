using System;
using System.Xml;

namespace DitaDotNet {
    public class DitaMap : DitaFile {

        #region Class Methods

        // Default constructor
        public DitaMap(XmlDocument xmlDocument, string filePath) : base(xmlDocument, filePath) {
            // Try to parse the file as a <type>
            if (!Parse()) {
                throw new Exception($"{FileName} is not parseable as a {this.GetType()}");
            }
        }

        public new bool Parse() {
            if (Parse("//map", "Map")) {
                return true;
            }

            return false;
        }

        #endregion Class Methods

        #region Static Methods

        // Does the given DOCTYPE match this object?
        public new static bool IsMatchingDocType(string docType) {
            if (!string.IsNullOrWhiteSpace(docType)) {
                return (docType.Contains("map") && docType.Contains("map.dtd") && docType.Contains("-//OASIS//DTD DITA Map//"));
            }

            return false;
        }

        #endregion Static Methods
    }
}