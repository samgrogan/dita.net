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

        #endregion Properties

        #region Public Methods

        // Construct a collection from a Dita bookmap in a Dita collection
        public DitaCollectionJson(DitaCollection collection, DitaBookMap bookMap, PageMapping pageMapping) {
            // Initialize the properties
            BookTitle = new Dictionary<string, string>();
            BookMeta = new Dictionary<string, string>();
            Chapters = new List<DitaCollectionLinkJson>();

            // Create the output object
            ParseBookMap(collection, bookMap, pageMapping);

        }

        // Write this collection to a given folder
        public void SerializeToFile(string output) {
            using (StreamWriter file = File.CreateText(Path.Combine(output, CollectionFileName))) {
                JsonSerializerSettings settings = new JsonSerializerSettings();
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, this);
            }
            Console.WriteLine($"Wrote {CollectionFileName}");
        }

        // Returns a list of 

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

        }

        // Parse the chapters in the document, recursively
        private void ParseBookMapChapters(DitaCollection collection, List<DitaElement> chapters) {
            foreach (DitaElement chapter in chapters) {
                // What is the href to the chapter?
                string chapterHref = chapter.Attributes?["href"];

                // Try to find this file
                DitaFile linkedFile = collection.GetFileByName(chapterHref);

                switch (linkedFile) {
                    case DitaBookMap bookMap:
                        // This should never happen
                        throw new Exception($"Found bookmap {linkedFile} nested in bookmap.");
                    case DitaMap map:
                        Console.WriteLine($"Found link to map {linkedFile}.");
                        break;
                    case DitaConcept concept:
                        Console.WriteLine($"Found link to concept {linkedFile}.");
                        break;
                    case DitaTopic topic:
                        Console.WriteLine($"Found link to topic {linkedFile}.");
                        break;
                }
            }
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