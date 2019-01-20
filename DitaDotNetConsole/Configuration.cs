using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;


// Encapsulates the settings for a single run of the console application
// Typically loaded from a JSON configuration

namespace DitaDotNet.Console {
    public class Configuration {

        #region Properties

        // What command are we executing?
        public string Command { get; set; }

        // What format are we converting to
        public string Format { get; set; }

        // Should we rename files during conversion?
        public bool Rename { get; set; }

        // What is the path to the input file or folder?
        public string Input { get; set; }

        // What is the path to the output file or folder?
        public string Output { get; set; }

        // What level of tracing should we produce?
        public TraceLevel TraceLevel { get; set; }

        // Should existing output files be deleted
        public bool DeleteExistingOutput { get; set; }

        #endregion Properties


        #region Public Methods

        // Default constructor
        public Configuration() {
            // Set the defaults
            Command = Parameters.CommandVerify;
            Format = Parameters.FormatJson;
            Rename = false;
            DeleteExistingOutput = false;
        }

        #endregion Public Methods

        // Create a config object from a json file
        public static Configuration CreateFromJson(string jsonPath) {
            Configuration config = null;

            // Try to deserialize from a json file
            using (StreamReader file = File.OpenText(jsonPath)) {
                JsonSerializer serializer = new JsonSerializer();
                config = (Configuration)serializer.Deserialize(file, typeof(Configuration));
            }

            return config;
        }
    }
}