using System;
using System.Collections.Generic;
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

        // What sections are included in the page
        public List<DitaPageSectionJson> Sections { get; set; }

        [JsonIgnore] private DitaCollection Collection { get; set; }

        #endregion Properties

        #region Public Methods

        // Construct from a single topic
        public DitaPageJson(DitaFile file, DitaCollection collection) {
            Collection = collection;

            // Get the title of the page
            Title = file.Title;

            // Create the file name
            FileName = file.NewFileName ?? file.FileName;
            FileName = Path.ChangeExtension(FileName, ".json");

            OriginalFileName = file.FileName;

            // Find the body element
            string bodyElementName = null;
            switch (file) {
                case DitaConcept ditaConcept:
                    bodyElementName = ditaConcept.BodyElementName();
                    break;
                case DitaReference ditaReference:
                    bodyElementName = ditaReference.BodyElementName();
                    break;
                case DitaTask ditaTask:
                    bodyElementName = ditaTask.BodyElementName();
                    break;
                case DitaTopic ditaTopic:
                    bodyElementName = ditaTopic.BodyElementName();
                    break;
                case DitaLanguageReference ditaLanguageRef:
                    bodyElementName = ditaLanguageRef.BodyElementName();
                    break;
                case DitaOptionReference ditaOptionRef:
                    bodyElementName = ditaOptionRef.BodyElementName();
                    break;
            }

            DitaElement bodyElement = file.RootElement.FindOnlyChild(bodyElementName);

            if (bodyElement != null) {
                Sections = new List<DitaPageSectionJson>();

                // Convert the body to html
                DitaElementToHtmlConverter htmlConverter = new DitaElementToHtmlConverter(collection);
                htmlConverter.Convert(bodyElement, Sections, out string bodyHtml);
                BodyHtml = bodyHtml;

                // Convert the body to text
                DitaElementToTextConverter textConverter = new DitaElementToTextConverter();
                textConverter.Convert(bodyElement, out string bodyText);
                BodyText = bodyText;
            }
            else {
                Trace.TraceWarning($"Body element not found in {FileName} ({file.FileName}.");
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