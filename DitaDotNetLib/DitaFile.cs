using System;
using System.Xml;
using System.Xml.Linq;

namespace Dita.Net
{
    public class DitaFile
    {
        // Valid types of DITA Files
        public enum DitaFileType {
            Unknown = 0,
            Map = 100,
            Topic = 200,
            Svg = 300
        }


        // The Xml Document that holds the raw DITA content
        private readonly XmlDocument _xmlDocument;


        // Default constructor - initialize from a file
        public DitaFile(string strPath) {
            // Try to load the file as an Xml document
            _xmlDocument = new XmlDocument();
            _xmlDocument.Load(strPath);
        }

        // Inspect the xml content and see what type of DITA file this is
        public DitaFileType InspectFileType() {
            // Does the document have a DOCTYPE?
            string docType = _xmlDocument.DocumentType?.OuterXml;

            if (!string.IsNullOrWhiteSpace(docType))
            {
                // Is this a dita map?




            }

            return DitaFileType.Unknown;
        }
    }
}
