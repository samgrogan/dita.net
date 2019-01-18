using System;
using System.IO;
using Newtonsoft.Json;

namespace DitaDotNet {
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

        // Is this page empty?
        public bool IsEmpty { get; set; }

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

            // Find the body element
            DitaElement bodyElement = null;
            switch (file) {
                case DitaTopic ditaTopic:
                    bodyElement = file.RootElement.FindOnlyChild("body");
                    break;
                case DitaConcept ditaConcept:
                    bodyElement = file.RootElement.FindOnlyChild("conbody");
                    break;
            }

            if (bodyElement != null) {
                // Convert the body to html
                DitaToHtmlConverter htmlConverter = new DitaToHtmlConverter();
                htmlConverter.Convert(bodyElement, out string bodyHtml);
                BodyHtml = bodyHtml;

                // Convert the body to text
                DitaToTextConverter textConverter = new DitaToTextConverter();
                textConverter.Convert(bodyElement, out string bodyText);
                BodyText = bodyText;
            }
            else {
                Trace.TraceWarning($"Body element not found in {FileName}.");
            }

            IsEmpty = string.IsNullOrEmpty(BodyText) || string.IsNullOrEmpty(Title);
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

            Trace.TraceInformation($"Wrote {FileName}");
        }

        #endregion

        #region Private Methods



        #endregion
    }
}