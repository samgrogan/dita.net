using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace Dita.Net {
    // Valid types of DITA Files
    public enum DitaFileType {
        Unknown = 0,

        [Description("DITA Map")] Map = 100,
        [Description("DITA Book Map")] BookMap = 101,
        [Description("DITA Topic")] Topic = 200,
        [Description("DITA Concept")] Concept = 201,
        [Description("SVG")] Svg = 300
    }

    public class DitaFile {
        #region Properties

        // The Xml Document that holds the raw DITA content
        protected XmlDocument XmlDocument { get; set; }

        // The path to the file the XML was read from
        public string FilePath { get; protected set; }

        // Just the file name of the file
        public string FileName { get; protected set; }

        // New name for this file, if renamed
        public string NewFileName { get; set; }

        // The root DITA element that contains all of the markup for this item
        public DitaElement RootElement { get; protected set; }

        #endregion Properties

        #region Class Methods

        // Default constructor - as a blank document
        protected DitaFile() {
            XmlDocument = new XmlDocument();
            FilePath = null;
            FileName = null;
            NewFileName = null;
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

        // Try to determine the title of the contents in this file by finding the title element
        public string GetTitle() {
            // Try to find the title node
            try {
                List<DitaElement> titleElements = RootElement?.FindChildren("title");
                if (titleElements?.Count == 1) {
                    return titleElements[0]?.Children[0]?.InnerText;
                }
            }
            catch {
                //
            }

            return null;
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

                // Is this a dita concept
                if (DitaConcept.IsMatchingDocType(docType)) {
                    return DitaFileType.Concept;
                }
            }

            return DitaFileType.Unknown;
        }

        public static bool IsMatchingDocType(string docType) {
            throw new NotImplementedException();
        }

        // Converts a title to a file name
        public static string TitleToFileName(string title, string extension) {
            string fileName = null;
            char[] illegalCharacters = {'/', '\\', '?', '%', '*', ':', '|', '\"', '<', '>', ' ', ',', '_', '\n', '\r', '\t'};

            try {
                if (!string.IsNullOrWhiteSpace(title)) {
                    fileName = title.ToLower().Trim();
                    // Replace special characters and spaces with _
                    foreach (char illegalChar in illegalCharacters) {
                        fileName = fileName.Replace(illegalChar, '-');
                    }

                    // Replace multiple _ characters with a single _
                    fileName = Regex.Replace(fileName, "-+", "-");

                    // Add the extension, if needed
                    if (!string.IsNullOrWhiteSpace(extension)) {
                        fileName = Path.ChangeExtension(fileName, extension);
                    }
                }
            }
            catch (Exception ex) {
                Console.WriteLine($"Error generating file name for {title}");
            }

            return fileName;
        }

        #endregion Class Methods
    }
}