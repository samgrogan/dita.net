using System;
using System.Xml;

namespace DitaDotNet {
    public class DitaLanguageReference : DitaTopicAbstract {
        #region Class Methods

        // Default constructor
        public DitaLanguageReference(XmlDocument xmlDocument, string filePath) : base(xmlDocument, filePath) {
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

        public override string BodyElementName() {
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

        #endregion Static Methods
    }
}