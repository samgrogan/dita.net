using System;
using System.Xml;

namespace DitaDotNet {
    public class DitaReference : DitaTopicAbstract {
        #region Class Methods

        // Default constructor
        public DitaReference(XmlDocument xmlDocument, string filePath) : base(xmlDocument, filePath) {
            // Try to parse the file as a <type>
            if (!Parse()) {
                throw new Exception($"{FileName} is not parseable as a {this.GetType()}");
            }
        }

        public new bool Parse() {
            if (Parse("//reference", "Reference")) {
                return true;
            }

            return false;
        }

        public override string BodyElementName() {
            return "refbody";
        }

        #endregion Class Methods

        #region Static Methods

        // Does the given DOCTYPE match this object?
        public new static bool IsMatchingDocType(string docType) {
            if (!string.IsNullOrWhiteSpace(docType)) {
                return (docType.Contains("!DOCTYPE reference"));
            }

            return false;
        }

        #endregion Static Methods
    }
}