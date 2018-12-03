using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

            // Create the output object
            ParseBookMap(bookMap, pageMapping);

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
        private void ParseBookMap(DitaBookMap bookMap, PageMapping pageMapping) {


        }

        // Parse the title from the book map
        private void ParseBookMapTitle(DitaElement titleElement) {
            BookTitle = new Dictionary<string, string>();

        }

        // Parse the book mata data from the book map
        private void ParseBookMapBookMeta(DitaElement bookMetaElement) {

        }

        #endregion
    }
}