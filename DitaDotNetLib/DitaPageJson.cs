using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Dita.Net {
    // Encapsulates a single "page" for display that consists of 1 or more dita topics
    internal class DitaPageJson {

        #region Properties

        // The title of the page
        public string Title { get; set; }

        // The filename of this page
        public string FileName { get; set; }

        // The original file name (if changed)
        public string OriginalFileName { get; set; }

        // The body of the page
        public string BodyHtml { get; set; }

        // The text of the page (without markup)
        public string BodyText { get; set; }

        // Is the body empty?
        public bool IsBodyEmpty { get; set; }

        #endregion Properties

        #region Public Methods

        // Construct from a single topic
        public DitaPageJson(DitaFile file) {

            // Get the title of the page
            Title = file.GetTitle();

            // Create the file name
            FileName = file.NewFileName ?? file.FileName;
            FileName = Path.ChangeExtension(FileName, ".json");

            OriginalFileName = file.FileName;

            // Convert the body to html
            DitaToHtmlConverter htmlConverter = new DitaToHtmlConverter();
            htmlConverter.Convert(file.RootElement.FindOnlyChild("body"), out string bodyHtml);
            BodyHtml = bodyHtml;

            // Convert the body to text
            DitaToTextConverter textConverter = new DitaToTextConverter();
            textConverter.Convert(file.RootElement.FindOnlyChild("body"), out string bodyText);
            BodyText = bodyText;

            IsBodyEmpty = string.IsNullOrEmpty(BodyText);
        }

        // Write this collection to a given folder
        public void SerializeToFile(string output) {
            using (StreamWriter file = File.CreateText(Path.Combine(output, FileName))) {
                JsonSerializerSettings settings = new JsonSerializerSettings {
                    Formatting = Formatting.Indented
                };
                JsonSerializer serializer = JsonSerializer.Create(settings);
                serializer.Serialize(file, this);
            }

            Console.WriteLine($"Wrote {FileName}");
        }

        #endregion

        #region Internal Methods

        #endregion
    }
}