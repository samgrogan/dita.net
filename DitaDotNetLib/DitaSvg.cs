using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dita.Net {
    public class DitaSvg : DitaFile {

        public static readonly string[] Extensions = new[] {".svg"};

        public DitaSvg(string filePath) : base() {
            FilePath = filePath;
        }

    }
}