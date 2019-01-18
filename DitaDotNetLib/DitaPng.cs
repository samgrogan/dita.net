using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DitaDotNet {
    class DitaPng : DitaImage {
        public static readonly string[] Extensions = new[] {".png"};

        public DitaPng(string filePath) : base(filePath) {
        }
    }
}