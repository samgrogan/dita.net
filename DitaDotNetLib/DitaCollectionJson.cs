using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Dita.Net
{
    internal class DitaCollectionLinkJson
    {
        public string Title { get; set; }
        public string FileName { get; set; }
        public List<DitaCollectionLinkJson> Children { get; set; }
    }

    internal class DitaCollectionJson
    {
        private const string CollectionFileName = "collection.json";

        #region Properties

        public Dictionary<string, string> BookTitle { get; set; }
        public Dictionary<string, string> BookMeta { get; set; }
        public List<DitaCollectionLinkJson> Chapters { get; set; }

        [JsonIgnore] public List<DitaPageJson> Pages { get; set; }

        [JsonIgnore] private DitaCollection Collection { get; set; }

        [JsonIgnore] private DitaBookMap BookMap { get; set; }

        #endregion Properties

        #region Public Methods

        // Construct a collection from a Dita bookmap in a Dita collection
        public DitaCollectionJson(DitaCollection collection, DitaBookMap bookMap)
        {
            // Store the construction properties
            Collection = collection;
            BookMap = bookMap;

            // Initialize the properties
            BookTitle = new Dictionary<string, string>();
            BookMeta = new Dictionary<string, string>();
            Chapters = new List<DitaCollectionLinkJson>();
            Pages = new List<DitaPageJson>();

            // Create the output object
            ParseBookMap();
        }

        // Write this collection to a given folder
        public void SerializeToFile(string output)
        {
            using (StreamWriter file = File.CreateText(Path.Combine(output, CollectionFileName)))
            {
                JsonSerializerSettings settings = new JsonSerializerSettings();
                settings.Formatting = Formatting.Indented;
                JsonSerializer serializer = JsonSerializer.Create(settings);
                serializer.Serialize(file, this);
            }

            Console.WriteLine($"Wrote {CollectionFileName}");
        }

        #endregion Public Methods

        #region Private Methods

        // 
        private void ParseBookMap()
        {
            // Find the booktitle element
            try
            {
                // Read the title
                ParseBookMapTitle();

                // Read the metadata
                ParseBookMapBookMeta();

                // Read the front matter


                // Read the chapters
                ParseBookMapChapters();

                // Read the back matter
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        // Parse the title from the book map
        private void ParseBookMapTitle()
        {
            DitaElement titleElement = BookMap.RootElement.FindOnlyChild("booktitle");

            AddChildDitaElementTextToDictionary(titleElement, "mainbooktitle", BookTitle);
            AddChildDitaElementTextToDictionary(titleElement, "booktitlealt", BookTitle);
        }

        // Parse the book meta date from the book map
        private void ParseBookMapBookMeta()
        {
            DitaElement bookMetaElement = BookMap.RootElement.FindOnlyChild("bookmeta");

            // Version
            DitaElement prodinfoElement = bookMetaElement?.FindOnlyChild("prodinfo");
            DitaElement vrmlistElement = prodinfoElement?.FindOnlyChild("vrmlist");
            DitaElement vrmElement = vrmlistElement?.FindChildren("vrm")?[0];
            string version = vrmElement?.Attributes?["version"];

            if (string.IsNullOrWhiteSpace(version) || version == "ProductVersionNumber") {
                DitaElement publisherinformationElement = bookMetaElement?.FindOnlyChild("publisherinformation");
                DitaElement publishedElement = publisherinformationElement?.FindOnlyChild("published");
                DitaElement revisionidElement = publishedElement?.FindOnlyChild("revisionid");
                version = revisionidElement?.InnerText;
            }

            BookMeta.Add("version",version);

            // Everything in category
            List<DitaElement> categoryData = bookMetaElement?.FindOnlyChild("category")?.Children;
            if (categoryData != null)
            {
                foreach (DitaElement data in categoryData)
                {
                    if (data?.Attributes["name"] != null)
                    {
                        BookMeta.Add(data?.Attributes["name"], data?.Attributes["value"]);
                    }
                }
            }
        }

        // Parse the chapters in the document, recursively
        private void ParseBookMapChapters()
        {
            List<DitaElement> chapters = BookMap.RootElement.FindChildren("chapter");

            foreach (DitaElement chapter in chapters)
            {
                // What is the href to the chapter?
                string chapterHref = chapter.Attributes?["href"];

                // Try to find this file
                DitaFile linkedFile = Collection.GetFileByName(chapterHref);
                Chapters.AddRange(ParseChaptersFromFile(linkedFile));
            }
        }

        // Parse chapter structure from a dita file
        private List<DitaCollectionLinkJson> ParseChaptersFromFile(DitaFile linkedFile)
        {
            Console.WriteLine($"Converting {linkedFile}");

            // What type of file is this?
            switch (linkedFile)
            {
                case DitaBookMap bookMap:
                    // This should never happen
                    throw new Exception($"Found bookmap {linkedFile} nested in bookmap.");
                case DitaMap map:
                    return ParseChaptersFromFile(map);
                case DitaTopic topic:
                    return ParseChaptersFromFile(topic);
                case DitaConcept concept:
                    return ParseChaptersFromFile(concept);
            }

            return null;
        }

        // Parse chapter structure from a .ditamap file
        private List<DitaCollectionLinkJson> ParseChaptersFromFile(DitaMap map)
        {
            Console.WriteLine($"Found link to map {map.NewFileName ?? map.FileName}.");

            // Find all the topic references
            List<DitaCollectionLinkJson> chapters = ParseTopicRefs(map.RootElement.FindChildren("topicref"));

            return chapters;
        }

        private List<DitaCollectionLinkJson> ParseTopicRefs(List<DitaElement> topicRefElements)
        {
            List<DitaCollectionLinkJson> chapters = new List<DitaCollectionLinkJson>();
            if (topicRefElements?.Count > 0)
            {
                foreach (DitaElement topicRefElement in topicRefElements)
                {
                    // Try to find the linked file
                    string topicRefHref = topicRefElement.Attributes["href"];

                    // Add references from the linked files
                    DitaFile linkedFile = Collection.GetFileByName(topicRefHref);

                    List<DitaCollectionLinkJson> newChapters = ParseChaptersFromFile(linkedFile);

                    // Are there child chapters?
                    List<DitaCollectionLinkJson> childChapters =
                        ParseTopicRefs(topicRefElement.FindChildren("topicref"));

                    if (newChapters.Count > 1 && childChapters.Count > 0)
                    {
                        // This should never happen
                        throw new Exception("Found multiple children in a map and topic refs.");
                    }

                    newChapters[0].Children = childChapters;
                    chapters.AddRange(newChapters);
                }
            }

            return chapters;
        }


        // Parse chapter structure from a dita topic (leaf node)
        private List<DitaCollectionLinkJson> ParseChaptersFromFile(DitaTopic topic)
        {
            List<DitaCollectionLinkJson> chapters = new List<DitaCollectionLinkJson>();

            // Build a page for this topic
            DitaPageJson topicPage = new DitaPageJson(topic);
            Pages.Add(topicPage);

            // Add this chapter to the toc for this page
            DitaCollectionLinkJson chapter = new DitaCollectionLinkJson
            {
                FileName = topicPage.FileName,
                Title = topicPage.Title
            };
            chapters.Add(chapter);

            Console.WriteLine($"Found link to topic {chapter.FileName}.");

            return chapters;
        }

        // Parse chapter structure from a dita concept (leaf node)
        private List<DitaCollectionLinkJson> ParseChaptersFromFile(DitaConcept concept)
        {
            List<DitaCollectionLinkJson> chapters = new List<DitaCollectionLinkJson>();

            // Build a page for this topic
            DitaPageJson topicPage = new DitaPageJson(concept);
            Pages.Add(topicPage);

            // Add this chapter to the toc for this page
            DitaCollectionLinkJson chapter = new DitaCollectionLinkJson
            {
                FileName = topicPage.FileName,
                Title = topicPage.Title
            };
            chapters.Add(chapter);

            Console.WriteLine($"Found link to concept {chapter.FileName}.");

            return chapters;
        }

        // Tries to add the text of the given element to the dictionary
        private bool AddChildDitaElementTextToDictionary(DitaElement parentElement, string type,
            Dictionary<string, string> dictionary)
        {
            // Try to find the child elements that match the type
            List<DitaElement> childElements = parentElement.FindChildren(type);

            if (childElements?.Count > 0)
            {
                foreach (DitaElement childElement in childElements)
                {
                    dictionary.Add(type, childElement.InnerText);
                }

                return true;
            }

            return false;
        }

        #endregion
    }
}