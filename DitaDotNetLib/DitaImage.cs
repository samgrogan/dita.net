using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DitaDotNet {
    public class DitaImage : DitaFile {

        public static readonly string[] Extensions = {".svg", ".png", ".jpg", ".gif", ".image"};

        public DitaImage(string filePath) : base(filePath) {
        }
    }
}