﻿using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace DitaDotNet {
    public class DitaFile {
        #region Declarations

        // Dictionary of delegates to aid matching and creation of dita file subclasses
        public delegate bool IsMatchingDocTypeDelegate(string docType);

        public static Dictionary<Type, IsMatchingDocTypeDelegate> DitaFileTypeMatching = new Dictionary<Type, IsMatchingDocTypeDelegate> {
            {typeof(DitaFileBookMap), DitaFileBookMap.IsMatchingDocType},
            {typeof(DitaFileMap), DitaFileMap.IsMatchingDocType},
            {typeof(DitaFileTopic), DitaFileTopic.IsMatchingDocType},
            {typeof(DitaFileConcept), DitaFileConcept.IsMatchingDocType},
            {typeof(DitaFileReference), DitaFileReference.IsMatchingDocType},
            {typeof(DitaFileTask), DitaFileTask.IsMatchingDocType},
            {typeof(DitaFileLanguageReference), DitaFileLanguageReference.IsMatchingDocType},
            {typeof(DitaOptionReference), DitaOptionReference.IsMatchingDocType},
            {typeof(DitaFileReferableContent), DitaFileReferableContent.IsMatchingDocType}
        };

        public delegate DitaFile ConstructorDelegate(XmlDocument xmlDocument, string filePath);

        public static Dictionary<Type, ConstructorDelegate> DitaFileTypeCreation = new Dictionary<Type, ConstructorDelegate> {
            {typeof(DitaFileBookMap), DitaFileBookMap.Create},
            {typeof(DitaFileMap), DitaFileMap.Create},
            {typeof(DitaFileTopic), DitaFileTopic.Create},
            {typeof(DitaFileConcept), DitaFileConcept.Create},
            {typeof(DitaFileReference), DitaFileReference.Create},
            {typeof(DitaFileTask), DitaFileTask.Create},
            {typeof(DitaFileLanguageReference), DitaFileLanguageReference.Create},
            {typeof(DitaOptionReference), DitaOptionReference.Create},
            {typeof(DitaFileReferableContent), DitaFileReferableContent.Create}
        };

        public delegate string BodyElementNameDelegate();

        public static Dictionary<Type, BodyElementNameDelegate> DitaFileBodyElement = new Dictionary<Type, BodyElementNameDelegate> {
            {typeof(DitaFileTopic), DitaFileTopic.BodyElementName},
            {typeof(DitaFileConcept), DitaFileConcept.BodyElementName},
            {typeof(DitaFileReference), DitaFileReference.BodyElementName},
            {typeof(DitaFileTask), DitaFileTask.BodyElementName},
            {typeof(DitaFileLanguageReference), DitaFileLanguageReference.BodyElementName},
            {typeof(DitaOptionReference), DitaOptionReference.BodyElementName},
            {typeof(DitaFileReferableContent), DitaFileReferableContent.BodyElementName}
        };

        private static readonly int _maxFileNameLength = 80;

        #endregion Declarations

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

        // The title of the file
        public string Title { get; set; }

        // Does this file define key references
        public Dictionary<string, DitaKeyDef> KeyDefs { get; }

        // An index of elements in the document, by id
        public Dictionary<string, DitaElement> ElementsById { get; }

        #endregion Properties

        #region Class Methods

        // Default constructor - as a blank document
        protected DitaFile() {
            XmlDocument = new XmlDocument();
            FilePath = null;
            FileName = null;
            NewFileName = null;
            RootElement = null;
            Title = null;
            KeyDefs = new Dictionary<string, DitaKeyDef>();
            ElementsById = new Dictionary<string, DitaElement>();
        }

        // Constructor for just a path
        public DitaFile(string filePath) : this() {
            FilePath = filePath;
            FileName = Path.GetFileName(filePath);
        }

        // Constructor with an XmlDocument and its path
        public DitaFile(XmlDocument xmlDocument, string filePath) : this(filePath) {
            // Store the raw xml and path
            XmlDocument = xmlDocument;
            Title = FileName;
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

                SetTitleFromXml();

                ParseKeyDefs();

                ParseElementIds(RootElement);

                return true;
            }
            catch (Exception ex) {
                Trace.TraceError($"Error parsing {rootNodeType} in {FileName}");
                Trace.TraceError(ex);
            }

            return false;
        }

        // Try to determine the title of the contents in this file by finding the title element
        public virtual void SetTitleFromXml() {
            // Try to find the title node
            try {
                List<DitaElement> titleElements = RootElement?.FindChildren("title");
                if (titleElements?.Count >= 1) {
                    Title = titleElements[0].ToString();
                }
                else {
                    Trace.TraceWarning($"Couldn't find title in {FileName}");
                }
            }
            catch {
                Trace.TraceError($"Couldn't find title in {FileName}");
            }
        }

        public override string ToString() {
            return NewFileName ?? FileName;
        }

        // Updates content references (conrefs) in this file
        public void ResolveConRefs(DitaCollection collection) {
            ResolveConRefs(RootElement, collection);
            SetTitleFromXml();
        }

        // Resolve keyword references
        public void ResolveKeywords(DitaCollection collection) {
            ResolveKeywords(RootElement, collection);
            SetTitleFromXml();
        }

        #endregion Public Class Methods

        #region Private Class Methods

        // If there are any keys defined in this file, store them in a dictionary for easy use
        private void ParseKeyDefs() {
            List<DitaElement> keyDefElements = RootElement.FindChildren("keydef");
            if (keyDefElements != null) {
                foreach (DitaElement keyDefElement in keyDefElements) {
                    DitaKeyDef keyDef = new DitaKeyDef {
                        Format = keyDefElement.AttributeValueOrDefault("format", ""),
                        Href = keyDefElement.AttributeValueOrDefault("href", ""),
                        Keys = keyDefElement.AttributeValueOrDefault("keys", ""),
                        Scope = keyDefElement.AttributeValueOrDefault("scope", ""),
                        ProcessingRole = keyDefElement.AttributeValueOrDefault("processing-role", ""),
                        Keywords = keyDefElement.ToString()
                    };

                    if (!KeyDefs.ContainsKey(keyDef.Keys)) {
                        KeyDefs.Add(keyDef.Keys, keyDef);
                    }
                    else {
                        Trace.TraceWarning($"Duplicate keyref {keyDef.Keys}");
                    }
                }
            }
        }

        // Build a dictionary of elements in the document, by id
        private void ParseElementIds(DitaElement parentElement) {
            if (parentElement != null) {
                // Does this element have an id?
                string id = parentElement.AttributeValueOrDefault("id", string.Empty);
                if (!string.IsNullOrEmpty(id)) {
                    if (!ElementsById.ContainsKey(id)) {
                        ElementsById.Add(id, parentElement);
                    }
                    else {
                        Trace.TraceWarning($"Duplicate element id {id} found in {FileName}.");
                    }
                }

                // Parse the children
                if (parentElement.Children != null) {
                    foreach (DitaElement childElement in parentElement.Children) {
                        ParseElementIds(childElement);
                    }
                }
            }
        }

        // Resolves conrefs in this file
        private void ResolveConRefs(DitaElement parentElement, DitaCollection collection) {
            if (parentElement != null) {
                bool updated = false;

                // Does this element have a conref?
                string conref = parentElement.AttributeValueOrDefault("conref", String.Empty);
                if (!string.IsNullOrEmpty(conref)) {
                    // We expect the conref to be in the form of filename.xml#fileid/elementid
                    Regex conRefRegex = new Regex("^(.*)#(.*)/(.*)$", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
                    MatchCollection conRefMatchCollection = conRefRegex.Matches(conref);
                    if (conRefMatchCollection?.Count > 0 && (conRefMatchCollection[0].Groups.Count == 4)) {
                        // Extract the components of the conref
                        string refFileName = conRefMatchCollection[0].Groups[1].Value;
                        string refFileId = conRefMatchCollection[0].Groups[2].Value;
                        string refElementId = conRefMatchCollection[0].Groups[3].Value;

                        if (Path.GetFileNameWithoutExtension(refFileName) != refFileId) {
                            Trace.TraceWarning($"conref file name '{refFileName}' is not equal to file id '{refFileId}'.");
                        }

                        // Try to find the file that this conref refers to
                        DitaFile refFile = collection.GetFileByName(refFileName);
                        if (refFile != null) {
                            // Does the references element exist in this file
                            if (refFile.ElementsById.ContainsKey(refElementId)) {
                                DitaElement refElement = refFile.ElementsById[refElementId];
                                // Copy the refernce element
                                parentElement.Copy(refElement);
                                updated = true;
                            }
                            else {
                                Trace.TraceWarning($"Element '{refElementId}' not found in file '{refFileName}'.");
                            }
                        }
                        else {
                            Trace.TraceWarning($"Can't find file '{refFileName}' referenced in file '{FileName}'.");
                        }
                    }
                    else {
                        Trace.TraceWarning($"conref {conref} not in expected format.");
                    }
                }

                // Update child references
                if (!updated && parentElement.Children != null) {
                    foreach (DitaElement childElement in parentElement.Children) {
                        ResolveConRefs(childElement, collection);
                    }
                }
            }
        }

        private void ResolveKeywords(DitaElement parentElement, DitaCollection collection) {
            if (parentElement != null) {
                if (parentElement.Type == "keyword") {
                    string keyref = parentElement.AttributeValueOrDefault("keyref", String.Empty);
                    if (!string.IsNullOrWhiteSpace(keyref)) {
                        // Try to find the referred to file
                        DitaKeyDef keyDef = collection.GetKeyDefByKey(keyref);
                        if (keyDef != null) {
                            if (!string.IsNullOrWhiteSpace(keyDef.Keywords)) {
                                parentElement.SetInnerText(keyDef.Keywords);
                            }
                        }
                    }
                }
                else {
                    // Check the child elements
                    if (parentElement.Children != null) {
                        foreach (DitaElement childElement in parentElement.Children) {
                            ResolveKeywords(childElement, collection);
                        }
                    }
                }
            }
        }

        #endregion Private Class Methods

        #region Static Methods

        // Creates and returns a new object
        public static DitaFile Create(XmlDocument xmlDocument, string filePath) {
            return new DitaFile(xmlDocument, filePath);
        }

        public static XmlDocument LoadAndCheckType(string filePath, out Type fileType) {
            // Try to load the file as an Xml document
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(filePath);

            fileType = InspectFileType(xmlDocument);

            return xmlDocument;
        }

        // Inspect the xml content and see what type of DITA file this is
        protected static Type InspectFileType(XmlDocument xmlDocument) {
            // Does the document have a DOCTYPE?
            string docType = xmlDocument.DocumentType?.OuterXml;

            if (!string.IsNullOrWhiteSpace(docType)) {
                foreach (Type keyType in DitaFileTypeMatching.Keys) {
                    if (DitaFileTypeMatching[keyType](docType)) {
                        return keyType;
                    }
                }
            }

            return null;
        }

        public static bool IsMatchingDocType(string docType) {
            throw new NotImplementedException();
        }

        // Converts a title to a file name
        public static string TitleToFileName(string title, string extension) {
            string fileName = null;
            string[] illegalCharacters = {"&lt;", "&gt;", "/", "\\", "?", "%", "*", ":", "|", "\"", "<", ">", " ", ",", ";", "_", "\n", "\r", "\t", "#", "+", ".", "—", "–", "&", "=" , "{", "}", "[", "]", "(", ")"};

            try {
                if (!string.IsNullOrWhiteSpace(title)) {
                    fileName = Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(title.ToLower().Trim()));

                    // Replace special characters and spaces with _
                    foreach (string illegalChar in illegalCharacters) {
                        fileName = fileName.Replace(illegalChar, "-");
                    }

                    // Replace multiple _ characters with a single _
                    fileName = Regex.Replace(fileName, "-+", "-");

                    // Add the extension, if needed
                    if (!string.IsNullOrWhiteSpace(extension)) {
                        fileName = Path.ChangeExtension(fileName, extension);
                    }

                    // Make sure the filename isn't too long
                    if (Path.GetFileNameWithoutExtension(fileName).Length > _maxFileNameLength) {
                        fileName = Path.ChangeExtension(Path.GetFileNameWithoutExtension(fileName).Substring(0, _maxFileNameLength), Path.GetExtension(fileName));
                    }
                }
            }
            catch {
                Trace.TraceError($"Error generating file name for {title}");
            }

            return fileName;
        }

        // Replace pseudo-symbols with the correct character
        public static string FixSpecialCharacters(string input) {
            return input?.Replace("(R)", "®").Replace("(r)", "®").Replace("(TM)", "™").Replace("(tm)", "™").Replace("(Tm)", "™").Replace("(tM)", "™");
        }

        #endregion Static Methods
    }
}