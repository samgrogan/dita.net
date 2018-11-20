using System;
using System.ComponentModel;
using System.Xml;
using System.Xml.Linq;

namespace Dita.Net
{
    public class DitaFile
    {
        // Valid types of DITA Files
        public enum DitaFileType {
            Unknown = 0,

            [Description("DITA Map")]
            Map = 100,

            [Description("DITA Book Map")]
            BookMap = 101,

            [Description("DITA Topic")]
            Topic = 200
        }
       
        // The Xml Document that holds the raw DITA content
        protected readonly XmlDocument _xmlDocument;


        // Default constructor - initialize from a file
        public DitaFile() {
            _xmlDocument = new XmlDocument();
        }

        public void Load(string strPath) {
            // Try to load the file as an Xml document
            _xmlDocument.Load(strPath);
        }

        // Inspect the xml content and see what type of DITA file this is
        public DitaFileType InspectFileType() {
            // Does the document have a DOCTYPE?
            string docType = _xmlDocument.DocumentType?.OuterXml;

            if (!string.IsNullOrWhiteSpace(docType))
            {
                // Is this a dita book map?
                if (DitaBookMap.IsMatchingDocType(docType)) {
                    return DitaFileType.BookMap;
                }
                // Is this a dita map?
                if (DitaMap.IsMatchingDocType(docType)) {
                    return DitaFileType.Map;
                }
                // Is this a dita topic
                if (DitaTopic.IsMatchingDocType(docType)) {
                    return DitaFileType.Topic;
                }
            }

            return DitaFileType.Unknown;
        }

        public static bool IsMatchingDocType(string docType) {
            throw new NotImplementedException();
        }

    }
}
