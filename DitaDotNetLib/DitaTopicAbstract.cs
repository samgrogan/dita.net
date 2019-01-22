using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DitaDotNet {
    public abstract class DitaTopicAbstract : DitaFile {
        public DitaTopicAbstract(XmlDocument xmlDocument, string filePath) : base(xmlDocument, filePath) {
        }

        protected new bool Parse(string rootNodePath, string rootNodeType) {
            if (base.Parse(rootNodePath, rootNodeType)) {
                // Set the title
                SetTitleFromXml();

                return true;
            }

            return false;
        }
    }
}