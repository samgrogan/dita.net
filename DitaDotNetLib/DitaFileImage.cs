using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DitaDotNet {
    public class DitaFileImage : DitaFile {

        public static readonly string[] Extensions = {".svg", ".png", ".jpg", ".gif", ".image"};

        public DitaFileImage(string filePath) : base(filePath) {
            if (Path.GetExtension(FileName) == ".image") {
                FileName = Path.ChangeExtension(FileName, ".svg");
            }
        }
    }
}