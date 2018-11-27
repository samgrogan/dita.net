using System;
using System.ComponentModel;
using System.Xml;
using System.Xml.Linq;

namespace Dita.Net {
    // Valid types of DITA Files
    public enum DitaFileType {
        Unknown = 0,

        [Description("DITA Map")] Map = 100,
        [Description("DITA Book Map")] BookMap = 101,
        [Description("DITA Topic")] Topic = 200,
        [Description("SVG")] Svg = 300
    }

    public class DitaFile {
        #region Properties

        // The Xml Document that holds the raw DITA content
        protected XmlDocument XmlDocument { get; set; }

        // The path to the file the XML was read from
        protected string FilePath { get; set; }

        // Just the file name of the file
        protected string FileName { get; set; }

        // The root DITA element that contains all of the markup for this item
        public DitaElement RootElement { get; protected set; }

        #endregion Properties

        #region Class Methods

        // Default constructor - as a blank document
        protected DitaFile() {
            XmlDocument = new XmlDocument();
            FilePath = null;
            FileName = null;
            RootElement = null;
        }

        // Constructor with an XmlDocument and its path
        public DitaFile(XmlDocument xmlDocument, string filePath) {
            // Store the raw xml and path
            XmlDocument = xmlDocument;
            FilePath = filePath;
            FileName = System.IO.Path.GetFileName(filePath);
        }

        // Try to parse the properties and data from this document 
        // Implemented by subclasses
        protected bool Parse() {
            throw new NotImplementedException();
        }

        // Try to parse the given Xml element and convert into Dita elements
        protected bool Parse(string rootNodePath, string rootNodeType) {
            try {
                // Try to find the topic node in the document
                XmlNodeList queryNodes = XmlDocument?.DocumentElement?.SelectNodes(rootNodePath);
                if (queryNodes == null) {
                    throw new Exception($"{rootNodeType} node not found.");
                }

                if (queryNodes.Count != 1) {
                    throw new Exception($"More then one {rootNodeType} node found, but only 1 expected.");
                }

                // Create and populate elements in this topic
                XmlNodeToDitaElementConverter converter = new XmlNodeToDitaElementConverter();
                RootElement = converter.Convert(queryNodes[0]);

                return true;
            }
            catch {
                Console.WriteLine($"Error parsing {rootNodeType} in {FileName}");
            }

            return false;
        }

        #endregion Class Methods

        #region Static Methods

        public static XmlDocument LoadAndCheckType(string filePath, out DitaFileType fileType) {
            // Try to load the file as an Xml document
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(filePath);

            fileType = InspectFileType(xmlDocument);

            return xmlDocument;
        }

        // Inspect the xml content and see what type of DITA file this is
        protected static DitaFileType InspectFileType(XmlDocument xmlDocument) {
            // Does the document have a DOCTYPE?
            string docType = xmlDocument.DocumentType?.OuterXml;

            if (!string.IsNullOrWhiteSpace(docType)) {
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

        #endregion Class Methods
    }
}