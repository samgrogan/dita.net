using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dita.Net {
    internal class DitaContentsJson {
        public Dictionary<string, string> Properties { get; set; }
        public List<DitaContentsLinkJson> Chapters { get; set; }
    }

    internal class DitaPropertyJson {
        public Dictionary<string, object> Properties { get; set; }
        public List<DitaPropertyJson> Children { get; set; }
    }

    internal class DitaContentsLinkJson {
        public string Title { get; set; }
        public string Link { get; set; }
        public List<DitaContentsLinkJson> Children { get; set; }
    }
}