using System;
using System.Xml;

namespace DitaDotNet {
    public class DitaFileLanguageReference : DitaFileTopicAbstract {
        #region Class Methods

        // Default constructor
        public DitaFileLanguageReference(XmlDocument xmlDocument, string filePath) : base(xmlDocument, filePath) {
            // Try to parse the file as a <type>
            if (!Parse()) {
                throw new Exception($"{FileName} is not parseable as a {this.GetType()}");
            }
        }

        public new bool Parse() {
            if (Parse("//LanguageRef", "LanguageRef")) {
                return true;
            }

            return false;
        }

        public new static string BodyElementName() {
            return "LanguageBody";
        }

        #endregion Class Methods

        #region Static Methods

        // Does the given DOCTYPE match this object?
        public new static bool IsMatchingDocType(string docType) {
            if (!string.IsNullOrWhiteSpace(docType)) {
                return (docType.Contains("LanguageRef") && docType.Contains("languageref.dtd") && docType.Contains("//DTD DITA Language Reference//"));
            }

            return false;
        }

        // Creates and returns a new object
        public new static DitaFileLanguageReference Create(XmlDocument xmlDocument, string filePath) {
            return new DitaFileLanguageReference(xmlDocument, filePath);
        }

        #endregion Static Methods
    }
}