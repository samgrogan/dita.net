using System;
using System.Xml;

namespace DitaDotNet {
    public class DitaFileConcept : DitaFileTopicAbstract {
        #region Class Methods

        // Default constructor
        public DitaFileConcept(XmlDocument xmlDocument, string filePath) : base(xmlDocument, filePath) {
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

        public new static string BodyElementName() {
            return "conbody";
        }

        #endregion Class Methods

        #region Static Methods

        // Does the given DOCTYPE match this object?
        public new static bool IsMatchingDocType(string docType) {
            if (!string.IsNullOrWhiteSpace(docType)) {
                return (docType.Contains("!DOCTYPE concept"));
            }

            return false;
        }

        // Creates and returns a new object
        public new static DitaFileConcept Create(XmlDocument xmlDocument, string filePath) {
            return new DitaFileConcept(xmlDocument, filePath);
        }

        #endregion Static Methods
    }
}