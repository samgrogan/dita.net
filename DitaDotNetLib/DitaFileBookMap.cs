using System;
using System.Xml;

namespace DitaDotNet {
    public class DitaFileBookMap : DitaFile {
        #region Class Methods

        // Default constructor
        public DitaFileBookMap(XmlDocument xmlDocument, string filePath) : base(xmlDocument, filePath) {
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

        public override void SetTitleFromXml() {
            // Bookmaps don't have titles
            return;
        }

        #endregion

        #region Static Members

        // Does the given DOCTYPE match this object?
        public new static bool IsMatchingDocType(string docType) {
            if (!string.IsNullOrWhiteSpace(docType)) {
                return (docType.Contains("!DOCTYPE bookmap"));
            }

            return false;
        }

        // Creates and returns a new object
        public new static DitaFileBookMap Create(XmlDocument xmlDocument, string filePath) {
            return new DitaFileBookMap(xmlDocument, filePath);
        }

        #endregion
    }
}