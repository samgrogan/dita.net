using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Dita.Net {
    class XmlNodeToDitaElementConverter {


        #region Class Methods

        // Ingest an XmlNode and recursively parse it to create a group of DitaElements
        public DitaElement Convert(XmlNode inputNode) {

            // What type of element are we creating
            string type = "";

            // Does this node/element have children
            bool isContainer = inputNode.HasChildNodes;

            // What is the text in this node/element
            string innerText = isContainer ? null : inputNode.InnerText;
            
            // Create the new DITA element
            DitaElement outputElement = new DitaElement(type, isContainer, innerText);

            // Add the children of this node/element, if any
            if (isContainer) {
                foreach (XmlNode childNode in inputNode.ChildNodes) {
                    outputElement.Children.Add(Convert(childNode));
                }
            }

            return outputElement;
        }

        #endregion

    }
}