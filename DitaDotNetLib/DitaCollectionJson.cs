using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Dita.Net {
    internal class DitaCollectionLinkJson {
        public string Title { get; set; }
        public string FileName { get; set; }
        public List<DitaCollectionLinkJson> Children { get; set; }
    }

    internal class DitaCollectionJson {

        private const string CollectionFileName = "collection.json";

        #region Properties

        public Dictionary<string, string> BookTitle { get; set; }
        public Dictionary<string, string> BookMeta { get; set; }
        public List<DitaCollectionLinkJson> Chapters { get; set; }

        [JsonIgnore]
        public List<DitaPageJson> Pages { get; set; }

        [JsonIgnore]
        private DitaCollection Collection { get; set; }

        [JsonIgnore]
        private DitaBookMap BookMap { get; set; }

        [JsonIgnore]


        #endregion Properties

        #region Public Methods

        // Construct a collection from a Dita bookmap in a Dita collection
        public DitaCollectionJson(DitaCollection collection, DitaBookMap bookMap, PageMapping pageMapping) {
            // Store the construction properties
            Collection = collection;
            BookMap = bookMap;


            // Initialize the properties
            BookTitle = new Dictionary<string, string>();
            BookMeta = new Dictionary<string, string>();
            Chapters = new List<DitaCollectionLinkJson>();

            // Create the output object
            ParseBookMap(bookMap, pageMapping);

        }

        // Write this collection to a given folder
        public void SerializeToFile(string output) {
            using (StreamWriter file = File.CreateText(Path.Combine(output, CollectionFileName))) {
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
        private void ParseBookMap(DitaCollection collection, DitaBookMap bookMap, PageMapping pageMapping) {
            // Find the booktitle element
            try {
                // Read the title
                ParseBookMapTitle(bookMap.RootElement.FindOnlyChild("booktitle"));

                // Read the metadata
                ParseBookMapBookMeta(bookMap.RootElement.FindOnlyChild("bookmeta"));

                // Read the front matter


                // Read the chapters
                ParseBookMapChapters(collection, bookMap.RootElement.FindChildren("chapter"));

                // Read the back matter
            }
            catch (Exception ex) {
                Console.WriteLine(ex);
            }
        }

        // Parse the title from the book map
        private void ParseBookMapTitle(DitaElement titleElement) {

            AddChildDitaElementTextToDictionary(titleElement, "mainbooktitle", BookTitle);
            AddChildDitaElementTextToDictionary(titleElement, "booktitlealt", BookTitle);
        }

        // Parse the book meta date from the book map
        private void ParseBookMapBookMeta(DitaElement bookMetaElement) {
            // Version
            DitaElement prodinfoElement = bookMetaElement?.FindOnlyChild("prodinfo");
            DitaElement vrmlistElement = prodinfoElement?.FindOnlyChild("vrmlist");
            DitaElement vrmElement = vrmlistElement?.FindChildren("vrm")?[0];
            BookMeta.Add("version", vrmElement?.Attributes?["version"]);

            // Everything in category
            List<DitaElement> categoryData = bookMetaElement?.FindOnlyChild("category").Children;
            if (categoryData != null) {
                foreach (DitaElement data in categoryData) {
                    if (data?.Attributes["name"] != null) {
                        BookMeta.Add(data?.Attributes["name"], data?.Attributes["value"]);
                    }
                }
            }
        }

        // Parse the chapters in the document, recursively
        private void ParseBookMapChapters(DitaCollection collection, List<DitaElement> chapters) {
            foreach (DitaElement chapter in chapters) {
                // What is the href to the chapter?
                string chapterHref = chapter.Attributes?["href"];

                // Try to find this file
                DitaFile linkedFile = collection.GetFileByName(chapterHref);
                Chapters.AddRange(ParseChaptersFromFile(linkedFile));
            }
        }

        // Parse chapter structure from a dita file
        private List<DitaCollectionLinkJson> ParseChaptersFromFile(DitaCollection collection, DitaFile linkedFile) {
            // What type of file is this?
            switch (linkedFile) {
                case DitaBookMap bookMap:
                    // This should never happen
                    throw new Exception($"Found bookmap {linkedFile} nested in bookmap.");
                case DitaMap map:
                    return ParseChaptersFromFile(map);
                case DitaTopic topic:
                    Console.WriteLine($"Found link to topic {linkedFile}.");
                    return ParseChaptersFromFile(topic);
            }

            return null;
        }

        private List<DitaCollectionLinkJson> ParseChaptersFromFile(DitaMap map) {
            List<DitaCollectionLinkJson> chapters = new List<DitaCollectionLinkJson>();

            Console.WriteLine($"Found link to map {map.NewFileName ?? map.FileName}.");

            // Find all the topic references
            List<DitaElement> topicRefElements = map.RootElement.FindChildren("topicref");

            foreach (DitaElement topicRefElement in topicRefElements) {
                // Try to find the linked file
                string topicRefHref = topicRefElement.Attributes["href"];

                DitaFile linkedFile = collection.GetFileByName(chapterHref);
                Chapters.AddRange(ParseChaptersFromFile(linkedFile));

            }


            return chapters;
        }

        private List<DitaCollectionLinkJson> ParseChaptersFromFile(DitaTopic topic) {
            List<DitaCollectionLinkJson> chapters = new List<DitaCollectionLinkJson>();

            Console.WriteLine($"Found link to topic {topic.NewFileName ?? topic.FileName}.");


            return chapters;
        }

        // Tries to add the text of the given element to the dictionary
        private bool AddChildDitaElementTextToDictionary(DitaElement parentElement, string type, Dictionary<string, string> dictionary) {
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

        #endregion
    }
}