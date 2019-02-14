using System;
using System.Xml;

namespace DitaDotNet {
    public class DitaFileTopic : DitaFileTopicAbstract {
        #region Class Methods

        // Default constructor
        public DitaFileTopic(XmlDocument xmlDocument, string filePath) : base(xmlDocument, filePath) {
            // Try to parse the file as a <type>
            if (!Parse()) {
                throw new Exception($"{FileName} is not parseable as a {this.GetType()}");
            }
        }

        public new bool Parse() {
            return Parse("//topic", "Topic");
        }

        public new static string BodyElementName() {
            return "body";
        }

        #endregion Class Methods

        #region Static Methods

        // Does the given DOCTYPE match this object?
        public new static bool IsMatchingDocType(string docType) {
            if (!string.IsNullOrWhiteSpace(docType)) {
                return (docType.Contains("!DOCTYPE topic"));
            }

            return false;
        }

        // Creates and returns a new object
        public new static DitaFileTopic Create(XmlDocument xmlDocument, string filePath) {
            return new DitaFileTopic(xmlDocument, filePath);
        }

        #endregion Static Methods
    }
}