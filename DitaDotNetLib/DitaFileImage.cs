using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DitaDotNet {
    public class DitaFileImage : DitaFile {

        public static readonly string[] Extensions = {".svg", ".png", ".jpg", ".gif", ".image"};

        public DitaFileImage(string filePath) : base(filePath) {
        }
    }
}