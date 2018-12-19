using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace Dita.Net {
    class XmlNodeToDitaElementConverter {
        #region Class Methods

        // Ingest an XmlNode and recursively parse it to create a group of DitaElements
        public DitaElement Convert(XmlNode inputNode) {
            // What type of element are we creating
            string type = inputNode?.Name;

            // Does this node/element have children
            bool isContainer = !IsNodeOnlyText(inputNode, out string innerText);
            if (!isContainer) {
                innerText = CleanInnerText(innerText);
            }

            // Create the new DITA element
            DitaElement outputElement = new DitaElement(type, isContainer, innerText);

            // Add the attributes
            if (inputNode?.Attributes != null) {
                foreach (XmlAttribute attribute in inputNode?.Attributes) {
                    outputElement.Attributes.Add(attribute.Name, attribute.InnerText);
                }
            }

            // Add the children of this node/element, if any
            // ReSharper disable once InvertIf
            if (isContainer && inputNode?.ChildNodes != null) {
                foreach (XmlNode childNode in inputNode.ChildNodes) {
                    outputElement.Children.Add(Convert(childNode));
                }
            }

            return outputElement;
        }

        private bool IsNodeOnlyText(XmlNode inputNode, out string innerText) {
            if (!inputNode.HasChildNodes) {
                innerText = inputNode.InnerText;
                return true;
            }

            if (inputNode.ChildNodes.Count > 1) {
                innerText = null;
                return false;
            }

            if (inputNode.ChildNodes[0].HasChildNodes) {
                innerText = null;
                return false;
            }

            if (inputNode.InnerXml.StartsWith("<?")) {
                innerText = null;
                return false;
            }

            if (!string.IsNullOrWhiteSpace(inputNode.InnerXml) && inputNode.InnerXml != inputNode.InnerText) {
                innerText = null;
                return false;
            }

            innerText = inputNode.ChildNodes[0].InnerText;
            return true;
        }

        // Cleans up text to remove extra formatting characters and spaces
        private string CleanInnerText(string innerText) {
            char[] replaceChars = {'\t', '\n', '\r'};

            foreach (char replaceChar in replaceChars) {
                innerText = innerText.Replace(replaceChar, ' ');
            }

            innerText = Regex.Replace(innerText, @" +", " ");
            return innerText.Trim();
        }

        #endregion
    }
}