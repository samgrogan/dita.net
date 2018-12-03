using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dita.Net {

    // Encapsulates a single "page" for display that consists of 1 or more dita topics
    internal class DitaPageJson {
        
        // The title of the page
        public string Title { get; set; }

        // The collection of topics
        public List<DitaTopicJson> Topics { get; set; }

        // Construct from a single topic
        public DitaPageJson(DitaTopic topic) {

        }

        // Construct from a dita map (collapse all topics)
        public DitaPageJson(DitaMap map) {

        }
    }
}