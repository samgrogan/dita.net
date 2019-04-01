using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DitaDotNet {
    public class DitaElement {
        #region Properties

        // What type of element is this
        public string Type { get; private set; }

        // What attributes does this element have
        public Dictionary<string, string> Attributes { get; private set; } = new Dictionary<string, string>();

        // Is this element a container for other elements?
        public bool IsContainer { get; private set; } = false;

        // The children of this elements
        // Only available if this is a container
        public List<DitaElement> Children { get; private set; } = null;

        // The parent of this element
        public DitaElement Parent { get; set; }

        // The previous sibling of this element
        public DitaElement PreviousSibling { get; set; }

        // The inner text of the element
        // Only available if this is *not* a container
        public string InnerText { get; private set; } = null;

        #endregion Properties

        #region Class Methods 

        // Default constructor
        public DitaElement(string type, bool isContainer = false, string innerText = null, DitaElement parent = null, DitaElement previousSibling = null) {
            Type = type;
            IsContainer = isContainer;
            Parent = parent;
            PreviousSibling = previousSibling;
            if (IsContainer) {
                Children = new List<DitaElement>();
            }
            else {
                InnerText = innerText;
            }
        }

        // Returns a list of all the elements of a given type. Only returns elements on the same level
        public List<DitaElement> FindChildren(string type) {
            return FindChildren(new[] {type}, this);
        }

        public List<DitaElement> FindChildren(string[] types) {
            return FindChildren(types, this);
        }

        protected List<DitaElement> FindChildren(string[] types, DitaElement parentElement) {
            List<DitaElement> result = null;
            if (IsContainer) {
                // Are any of our direct children of this type?
                result = parentElement?.Children?.Where(e => types.Contains(e.Type)).ToList();

                if (result?.Count == 0) {
                    // Try finding children of children
                    foreach (DitaElement childElement in parentElement.Children) {
                        result = FindChildren(types, childElement);
                        if (result?.Count != 0) {
                            break;
                        }
                    }
                }
            }

            return result;
        }

        // Returns a list of the one and only child of the given type
        // If more than 1 is found, throws an exception
        public DitaElement FindOnlyChild(string type) {
            List<DitaElement> children = FindChildren(type);
            if (children?.Count > 1) {
                throw new Exception($"Expected at most one child of type {type} but found {children.Count}");
            }

            return children?[0];
        }

        // Returns the given attribute value, if it exists, or the default if it doesn't
        public string AttributeValueOrDefault(string key, string defaultValue) {
            return Attributes?.ContainsKey(key) ?? false ? Attributes?[key] : defaultValue;
        }

        // Update references
        // Find and hrefs that point to an old file name and replace them with a new reference
        public void UpdateReferences(string oldFileName, string newFileName) {
            if (oldFileName != newFileName && !string.IsNullOrWhiteSpace(newFileName)) {
                // Are there any href attributes?
                if (Attributes.ContainsKey("href")) {
                    string href = Attributes["href"];
                    if (href == oldFileName) {
                        Attributes["href"] = newFileName;
                        Trace.TraceInformation($"Updated reference from {href} to {newFileName} in {Type}");
                    }
                }

                // Update our children
                if (IsContainer) {
                    foreach (DitaElement element in Children) {
                        element.UpdateReferences(oldFileName, newFileName);
                    }
                }
            }
        }

        // Collapses the element to a string
        public override string ToString() {
            if (IsContainer) {
                StringBuilder concat = new StringBuilder();
                foreach (DitaElement childElement in Children) {
                    concat.Append($"{childElement.ToString().Trim()} ");
                }

                return concat.ToString().Trim();
            }

            string expanded = ExpandInnerText(InnerText);
            if (!string.IsNullOrEmpty(expanded)) {
                return expanded;
            }

            return string.Empty;
        }

        // Replaces the values in this element with those copied from another element
        public void Copy(DitaElement sourceElement) {
            Type = sourceElement.Type;
            Attributes = sourceElement.Attributes;
            IsContainer = sourceElement.IsContainer;
            Children = sourceElement.Children;
            InnerText = sourceElement.InnerText;
        }

        // Set the inner text
        public void SetInnerText(string innerText) {
            InnerText = ExpandInnerText(innerText);
        }

        #endregion Class Methods

        #region Private Methods

        // Dynamic string substitution
        // Some elements have rules for adding additional text
        private string ExpandInnerText(string inputText) {
            string outputText = inputText;

            if (!string.IsNullOrWhiteSpace(outputText)) {
                switch (Type) {
                    case "tm":
                        switch (AttributeValueOrDefault("tmtype", "")) {
                            case "reg":
                                return $"{inputText}®";
                            case "tm":
                                return $"{inputText}™";
                        }

                        break;
                }

                // Replace < >
                outputText = outputText.Replace("<", "&lt;").Replace(">", "&gt;");
                // Replace multiple spaces with 1 space
                while (outputText.Contains("  ")) {
                    outputText = outputText.Replace("  ", " ");
                }
            }

            return outputText;
        }

        #endregion
    }
}