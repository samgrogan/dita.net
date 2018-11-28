using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dita.Net {
    public class DitaElement {
        #region Properties

        // What type of element is this
        public string Type { get; private set; }

        // What attributes does this element have
        public Dictionary<string, string> Attributes { get; } = new Dictionary<string, string>();

        // Is this element a container for other elements?
        public bool IsContainer { get; } = false;

        // The children of this elements
        // Only available if this is a container
        public List<DitaElement> Children { get; } = null;

        // The inner text of the element
        // Only available if this is *not* a container
        public string InnerText { get; } = null;

        #endregion Properties

        #region Class Methods 

        // Default constructor
        public DitaElement(string type, bool isContainer = false, string innerText = null) {
            Type = type;
            IsContainer = isContainer;
            if (IsContainer) {
                Children = new List<DitaElement>();
            }
            else {
                InnerText = innerText;
            }
        }

        // Returns a list of all the elements of a given type. Only returns elements on the same level
        public List<DitaElement> FindChildren(string type) {
            return FindChildren(type, this);
        }

        protected List<DitaElement> FindChildren(string type, DitaElement parentElement) {
            List<DitaElement> result = null;

            if (IsContainer) {
                // Are any of our direct children of this type?
                result = parentElement.Children.Where(e => e.Type == type).ToList();

                if (result.Count == 0) {
                    // Try finding children of children
                    foreach (DitaElement childElement in parentElement.Children) {
                        result = FindChildren(type, childElement);
                        if (result.Count != 0) {
                            break;
                        }
                    }
                }
            }

            return result;
        }

        #endregion Class Methods
    }
}