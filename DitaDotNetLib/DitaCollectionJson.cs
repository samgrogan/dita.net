using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Trace = DitaDotNet.Trace;

namespace DitaDotNet {
    internal class DitaCollectionLinkJson {
        public string Title { get; set; }
        public string FileName { get; set; }
        public Guid Guid { get; set; }
        public bool IsEmpty { get; set; }
        public List<DitaCollectionLinkJson> Children { get; set; }

        public DitaCollectionLinkJson() {
            Guid = Guid.NewGuid();
            Children = new List<DitaCollectionLinkJson>();
        }
    }

    internal class DitaCollectionJson {
        private readonly string CollectionFileName = "collection.json";

        private readonly string[] RefElements = new[] {"topicref", "mapref"};

        #region Properties

        public Dictionary<string, string> BookTitle { get; set; }
        public Dictionary<string, string> BookMeta { get; set; }
        public List<DitaCollectionLinkJson> Chapters { get; set; }

        [JsonIgnore]
        public List<DitaPageJson> Pages { get; set; }

        [JsonIgnore]
        public List<DitaFileImage> Images => Collection?.GetImages();

        [JsonIgnore]
        private DitaCollection Collection { get; set; }

        [JsonIgnore]
        private DitaFile RootMap { get; set; }

        #endregion Properties

        #region Public Methods

        // Construct a collection from a Dita bookmap in a Dita collection
        public DitaCollectionJson(DitaCollection collection, DitaFile rootMap) {
            // Store the construction properties
            Collection = collection;
            RootMap = rootMap;

            // Initialize the properties
            BookTitle = new Dictionary<string, string>();
            BookMeta = new Dictionary<string, string>();
            Chapters = new List<DitaCollectionLinkJson>();
            Pages = new List<DitaPageJson>();

            // Create the output object
            if (RootMap is DitaFileBookMap) {
                ParseBookMap();
            }
            else if (RootMap is DitaFileMap) {
                ParseMap();
            }
        }

        // Write this collection to a given folder
        public void SerializeToFile(string output) {
            using (StreamWriter file = File.CreateText(Path.Combine(output, CollectionFileName))) {
                JsonSerializerSettings settings = new JsonSerializerSettings {
                    Formatting = Formatting.Indented
                };
                JsonSerializer serializer = JsonSerializer.Create(settings);
                BookMeta["doc path"] = new DirectoryInfo(output).Name;
                serializer.Serialize(file, this);
            }

            Trace.TraceInformation($"Wrote {CollectionFileName}");
        }

        #endregion Public Methods

        #region Private Methods

        // Build the internal structure based on the bookmap
        private void ParseBookMap() {
            // Find the booktitle element
            try {
                // Read the title
                ParseBookMapTitle();

                // Read the metadata
                ParseBookMapBookMeta();

                // Read the front matter

                // Read the chapters
                ParseBookMapChapters();

                // Read the back matter

                // Removes any blank topics and replaces them with links to their first populate child
                RemoveBlankPages();
            }
            catch (Exception ex) {
                Trace.TraceError(ex);
            }
        }

        // Build the internal structure based on a map
        private void ParseMap() {
            try {
                // Read the title
                BookTitle.Add("mainbooktitle", RootMap.Title);

                // Read the matadata
                ParseMapMeta();

                if (!BookMeta.ContainsKey("document title")) {
                    BookMeta.Add("document title", RootMap.Title);
                }

                // Add the chapters
                Chapters.AddRange(ParseChaptersFromFile(RootMap));
            }
            catch (Exception ex) {
                Trace.TraceError(ex);
            }
        }

        // Parse the title from the book map
        private void ParseBookMapTitle() {
            DitaElement titleElement = RootMap.RootElement.FindOnlyChild("booktitle");

            AddChildDitaElementTextToDictionary(titleElement, "mainbooktitle", BookTitle);
            AddChildDitaElementTextToDictionary(titleElement, "booktitlealt", BookTitle);
        }

        // Parse the book meta data from the book map
        private void ParseBookMapBookMeta() {
            DitaElement bookMetaElement = RootMap.RootElement.FindOnlyChild("bookmeta");

            ParseBookMapVersion(bookMetaElement);
            ParseBookMapReleaseDate(bookMetaElement);

            // Everything in category

            List<DitaElement> categoryData = bookMetaElement?.FindOnlyChild("category")?.Children;
            if (categoryData != null) {
                foreach (DitaElement data in categoryData) {
                    if (data?.Attributes["name"] != null) {
                        BookMeta.Add(data?.Attributes["name"], data?.Attributes["value"]);
                    }
                }
            }
        }

        // Parse the  meta date from the map
        private void ParseMapMeta() {
            DitaElement bookMetaElement = RootMap.RootElement.FindOnlyChild("topicmeta");

            // Everything in category
            try {
                List<DitaElement> categoryData = bookMetaElement?.FindOnlyChild("category")?.Children;
                if (categoryData != null) {
                    foreach (DitaElement data in categoryData) {
                        if (data?.Attributes["name"] != null) {
                            BookMeta.Add(data?.Attributes["name"], data?.Attributes["value"]);
                        }
                    }
                }
            }
            catch (Exception ex) {
                Trace.TraceError(ex);
            }
        }

        // Parse the version of the document
        private void ParseBookMapVersion(DitaElement bookMetaElement) {
            // Try checking the publisher information section
            DitaElement publisherInformationElement = bookMetaElement?.FindOnlyChild("publisherinformation");
            DitaElement publishedElement = publisherInformationElement?.FindOnlyChild("published");
            DitaElement revisionIdElement = publishedElement?.FindOnlyChild("revisionid");
            string version = revisionIdElement?.ToString();

            // Try checking the prodinfo section
            if (string.IsNullOrWhiteSpace(version) || version == "ProductVersionNumber") {
                DitaElement prodinfoElement = bookMetaElement?.FindOnlyChild("prodinfo");
                DitaElement vrmlistElement = prodinfoElement?.FindOnlyChild("vrmlist");
                DitaElement vrmElement = vrmlistElement?.FindChildren("vrm")?[0];
                version = vrmElement?.Attributes?["version"];
            }

            if (!string.IsNullOrWhiteSpace(version)) {
                BookMeta.Add("version", version);
            }
        }

        // Parse the date of the document
        private void ParseBookMapReleaseDate(DitaElement bookMetaElement) {
            DitaElement publisherInformationElement = bookMetaElement?.FindOnlyChild("publisherinformation");
            DitaElement publishedElement = publisherInformationElement?.FindOnlyChild("published");
            DitaElement completedElement = publishedElement?.FindOnlyChild("completed");
            string year = completedElement?.FindOnlyChild("year")?.ToString();
            string month = completedElement?.FindOnlyChild("month")?.ToString();
            string day = completedElement?.FindOnlyChild("day")?.ToString();

            if (!string.IsNullOrWhiteSpace(year) && !string.IsNullOrWhiteSpace(month) && !string.IsNullOrWhiteSpace(day)) {
                try {
                    // Is this a valid date?
                    DateTime publishDate = new DateTime(int.Parse(year), int.Parse(month), int.Parse(day));
                    BookMeta.Add("published date", $"{publishDate.Day}/{publishDate.Month}/{publishDate.Year} 00:00:00");
                }
                catch {
                    //
                }
            }
        }


        // Parse the chapters in the document, recursively
        private void ParseBookMapChapters() {
            List<DitaElement> chapters = RootMap.RootElement.FindChildren("chapter");

            foreach (DitaElement chapter in chapters) {
                // What is the href to the chapter?
                string chapterHref = chapter.Attributes?["href"];

                // Try to find this file
                DitaFile linkedFile = Collection.GetFileByName(chapterHref);
                Chapters.AddRange(ParseChaptersFromFile(linkedFile));
            }
        }

        // Parse chapter structure from a dita file
        private List<DitaCollectionLinkJson> ParseChaptersFromFile(DitaFile linkedFile, string navTitle = null) {
            Trace.TraceInformation($"Converting {linkedFile}");

            // What type of file is this?
            switch (linkedFile) {
                case DitaFileBookMap bookMap:
                    // This should never happen
                    throw new Exception($"Found bookmap {linkedFile} nested in bookmap.");
                case DitaFileMap map:
                    return ParseChaptersFromMap(map);
                case DitaFileTopicAbstract topic:
                    return ParseChaptersFromTopic(topic, navTitle);
            }

            return null;
        }

        // Parse chapter structure from a .ditamap file
        private List<DitaCollectionLinkJson> ParseChaptersFromMap(DitaFileMap map) {
            Trace.TraceInformation($"Found link to map {map.NewFileName ?? map.FileName}.");

            // Find all the topic references
            List<DitaCollectionLinkJson> chapters = ParseRefs(map.RootElement.FindChildren(RefElements));

            return chapters;
        }

        private List<DitaCollectionLinkJson> ParseRefs(List<DitaElement> topicRefElements) {
            List<DitaCollectionLinkJson> chapters = new List<DitaCollectionLinkJson>();
            if (topicRefElements?.Count > 0) {
                foreach (DitaElement topicRefElement in topicRefElements) {
                    // Try to find the linked file
                    string topicRefHref = topicRefElement.AttributeValueOrDefault("href", "");
                    string topicRefKeyRef = topicRefElement.AttributeValueOrDefault("keyref", "");
                    string topicRefNavTitle = topicRefElement.AttributeValueOrDefault("navtitle", "");

                    // If there is no navtitle, check the topicmeta
                    if (string.IsNullOrWhiteSpace(topicRefNavTitle)) {
                        topicRefNavTitle = topicRefElement?.FindOnlyChild("topicmeta")?.FindOnlyChild("navtitle")?.ToString();
                    }

                    DitaFile linkedFile = null;
                    if (!string.IsNullOrWhiteSpace(topicRefHref)) {
                        linkedFile = Collection.GetFileByName(topicRefHref);
                    }
                    // If no href, try to find by keyref
                    if (linkedFile == null && !string.IsNullOrWhiteSpace(topicRefKeyRef)) {
                        linkedFile = Collection.GetFileByKey(topicRefKeyRef);
                    }

                    if (linkedFile != null) {
                        if (string.IsNullOrWhiteSpace(linkedFile.Title)) {
                            linkedFile.Title = topicRefNavTitle;
                        }

                        // Add references from the linked files
                        List<DitaCollectionLinkJson> newChapters = ParseChaptersFromFile(linkedFile, topicRefNavTitle);

                        if (newChapters != null && newChapters.Count > 0) {
                            // Are there child chapters?
                            List<DitaCollectionLinkJson> childChapters = ParseRefs(topicRefElement.FindChildren(RefElements));

                            if (newChapters.Count > 1 && childChapters.Count > 0) {
                                // This should never happen
                                throw new Exception("Found multiple children in a map and topic refs.");
                            }

                            if (childChapters != null && childChapters.Count > 0) {
                                newChapters[0]?.Children?.AddRange(childChapters);
                            }

                            chapters.AddRange(newChapters);
                        }
                    }
                    else {
                        Trace.TraceWarning($"Reference with missing href/keyref: {topicRefElement}");
                    }
                }
            }

            return chapters;
        }


        // Parse chapter structure from a dita topic (leaf node)
        private List<DitaCollectionLinkJson> ParseChaptersFromTopic(DitaFileTopicAbstract topic, string navTitle = null) {
            List<DitaCollectionLinkJson> chapters = new List<DitaCollectionLinkJson>();

            try {
                // Build a page for this topic
                DitaPageJson topicPage = new DitaPageJson(topic, Collection);
                Pages.Add(topicPage);

                // Add this chapter to the toc for this page
                DitaCollectionLinkJson chapter = new DitaCollectionLinkJson {
                    FileName = topicPage.FileName,
                    Title = navTitle ?? topicPage.Title
                };
                chapters.Add(chapter);

                Trace.TraceInformation($"Found link to topic {chapter.FileName}.");
            }
            catch (Exception ex) {
                Trace.TraceError($"Error parsing topic {topic.FileName}");
            }

            return chapters;
        }

        // Tries to add the text of the given element to the dictionary
        private bool AddChildDitaElementTextToDictionary(DitaElement parentElement, string type,
            Dictionary<string, string> dictionary) {
            // Try to find the child elements that match the type
            List<DitaElement> childElements = parentElement.FindChildren(type);

            if (childElements?.Count > 0) {
                foreach (DitaElement childElement in childElements) {
                    dictionary.Add(type, childElement.InnerText);
                }

                return true;
            }

            return false;
        }

        // Removes blank pages from the output
        private void RemoveBlankPages() {
            // Remove links to blank pages
            MarkBlankChapterLinks(Chapters);

            // Remove the blank pages
            List<DitaPageJson> removePages = new List<DitaPageJson>();
            foreach (DitaPageJson page in Pages) {
                if (page.IsEmpty) {
                    Trace.TraceWarning($"Removing empty page {page.FileName}");
                    removePages.Add(page);
                }
            }

            Pages = Pages.Except(removePages).ToList();
        }


        private void MarkBlankChapterLinks(List<DitaCollectionLinkJson> chapters) {
            if (chapters != null) {
                foreach (DitaCollectionLinkJson link in chapters) {
                    // Is the page empty?
                    link.IsEmpty = IsLinkToEmptyPage(link, out DitaPageJson _);

                    // Remove blanks from the children
                    MarkBlankChapterLinks(link.Children);
                }
            }
        }

        // Does the given chapter link point to an empty page?
        private bool IsLinkToEmptyPage(DitaCollectionLinkJson link, out DitaPageJson pageJson) {
            pageJson = Pages.FirstOrDefault(o => o.FileName == link.FileName);
            // Is the page empty?
            return (pageJson?.IsEmpty ?? false);
        }

        #endregion Private Methods
    }
}