using System;
using System.ComponentModel;
using System.Xml;
using System.Xml.Linq;

namespace Dita.Net
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

    public class DitaFile
    {
        #region Declarations



        #endregion Declarations

        #region Members

        // The Xml Document that holds the raw DITA content
        protected readonly XmlDocument _xmlDocument;

        // The path to the file the XML was read from
        protected string _filePath;

        #endregion Members

        #region Properties

        public string Path => _filePath;

        // The root DITA element that contains all of the markup for this item
        public DitaElement RootElement { get; internal set; } = null;

        #endregion Properties

        #region Class Methods

        // Default constructor - initialize from a file
        public DitaFile() {
            _xmlDocument = new XmlDocument();
            _filePath = null;
        }

        public void Load(string filePath) {
            _filePath = filePath;

            // Try to load the file as an Xml document
            _xmlDocument.Load(filePath);
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

        // Try to parse the properties and data from this document 
        // Implemented by subclasses
        public bool Parse() {
            throw new NotImplementedException();
        }

        #endregion Class Methods

        #region Static Methods

        public static bool IsMatchingDocType(string docType) {
            throw new NotImplementedException();
        }

        #endregion Class Methods
    }
}
