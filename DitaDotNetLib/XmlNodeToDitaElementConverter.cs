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
            bool isContainer = false;

            // What is the text in this node/element
            string innerText = null;
            

            // Create the new DITA element
            DitaElement outputElement = new DitaElement(type, isContainer, innerText);

            // Add the children of this node/element, if any
            if (isContainer) {

            }

            return outputElement;
        }

        #endregion

    }
}