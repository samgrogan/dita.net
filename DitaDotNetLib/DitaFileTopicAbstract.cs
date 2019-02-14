using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DitaDotNet {
    public abstract class DitaFileTopicAbstract : DitaFile {
        protected DitaFileTopicAbstract(XmlDocument xmlDocument, string filePath) : base(xmlDocument, filePath) {
        }

        public static string BodyElementName() {
            throw new NotImplementedException();
        }

        protected new bool Parse(string rootNodePath, string rootNodeType) {
            return base.Parse(rootNodePath, rootNodeType);
        }
    }
}