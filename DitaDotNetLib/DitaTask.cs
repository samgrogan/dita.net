using System;
using System.Xml;

namespace DitaDotNet {
    public class DitaTask : DitaTopicAbstract {
        #region Class Methods

        // Default constructor
        public DitaTask(XmlDocument xmlDocument, string filePath) : base(xmlDocument, filePath) {
            // Try to parse the file as a <type>
            if (!Parse()) {
                throw new Exception($"{FileName} is not parseable as a {this.GetType()}");
            }
        }

        public new bool Parse() {
            if (Parse("//task", "Task")) {
                return true;
            }

            return false;
        }

        public override string BodyElementName() {
            return "taskbody";
        }

        #endregion Class Methods

        #region Static Methods

        // Does the given DOCTYPE match this object?
        public new static bool IsMatchingDocType(string docType) {
            if (!string.IsNullOrWhiteSpace(docType)) {
                return (docType.Contains("!DOCTYPE task"));
            }

            return false;
        }

        #endregion Static Methods
    }
}