using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dita.Net {
    class DitaToHtmlConverter : DitaConverter {
        public bool Convert(DitaElement bodyElement, out string body) {
            StringBuilder bodyStringBuilder = new StringBuilder();

            if (bodyElement != null) {
                if (bodyElement.IsContainer) {
                    foreach (DitaElement childElement in bodyElement?.Children) {
                        bodyStringBuilder.Append(Convert(childElement));
                    }
                }
                else {
                    bodyStringBuilder.Append(bodyElement.InnerText);
                }
            }

            body = bodyStringBuilder.ToString();

            return true;
        }

        private string Convert(DitaElement element) {
            // Determine the new html tag and attributes
            string htmlTag = ConvertDitaTagToHtmlTag(element);
            Dictionary<string, string> htmlAttributes = ConvertDitaTagAttributesToHtmlTagAttributes(element);
            AddHtmlTagAttributes(htmlAttributes, element);

            // If this is a parent, then add the children
            if (element.IsContainer) {
                StringBuilder elementStringBuilder = new StringBuilder();
                elementStringBuilder.Append(HtmlOpeningTag(htmlTag, htmlAttributes));

                foreach (DitaElement childElement in element.Children) {
                    elementStringBuilder.Append(Convert(childElement));
                }

                elementStringBuilder.Append(HtmlClosingTag(htmlTag));

                return elementStringBuilder.ToString();
            }
            else {
                return $"{HtmlOpeningTag(htmlTag, htmlAttributes)}{element.InnerText}{HtmlClosingTag(htmlTag)}";
            }
        }

        // Takes a DITA "tag" and returns the corresponding HTML tag
        private string ConvertDitaTagToHtmlTag(DitaElement element) {
            switch (element.Type) {
                // Suppress these tags
                case "colspec": return "";
                case "entry":
                    if (element.Parent?.Parent?.Type == "thead") {
                        return "th";
                    }
                    return "td";
                case "image": return "img";
                case "row": return "tr";
                case "tgroup": return "";
                case "title":
                    //if (element.Parent?.Type == "fig") {
                    //    return "h4";
                    //}
                    if (element.Parent?.Type == "section") {
                        return "h3";
                    }
                    return "h4";
                case "#text": return "";
            }

            return element.Type;
        }

        // Converts DITA tag attributes to html tag attributes
        private Dictionary<string, string> ConvertDitaTagAttributesToHtmlTagAttributes(DitaElement element) {
            Dictionary<string, string> htmlAttributes = new Dictionary<string, string>();

            foreach (string ditaAttributeKey in element.Attributes.Keys) {
                (string newKey, string newValue) = ConvertDitaTagAttributeToHtmlTagAttribute(ditaAttributeKey, element.Attributes[ditaAttributeKey], element);
                if (!string.IsNullOrWhiteSpace(newKey)) {
                    htmlAttributes.Add(newKey, newValue);
                }
            }

            return htmlAttributes;
        }

        // Converts a single dita tag attribute to an html attribute
        private (string newKey, string newValue) ConvertDitaTagAttributeToHtmlTagAttribute(string key, string value, DitaElement element) {
            switch (element.Type) {
                case "image":
                    if (key == "href") {
                        return ("src", $"%IMG_ROOT%/{value}");
                    }
                    break;
            }

            return ("", "");
        }

        // Add additional attributes to specific html tags
        private void AddHtmlTagAttributes(Dictionary<string, string> htmlAttributes, DitaElement element) {
            switch (element.Type) {
                case "image":
                    if (!htmlAttributes.ContainsKey("width")) {
                        htmlAttributes.Add("width", "100%");
                    }
                    break;
                case "table":
                    if (!htmlAttributes.ContainsKey("class")) {
                        htmlAttributes.Add("class", "table");
                    }
                    break;
            }
        }


        // Writes an open tag
        private string HtmlOpeningTag(string htmlTag, Dictionary<string, string> htmlAttributes = null) {
            StringBuilder attributes = new StringBuilder();
            if (htmlAttributes?.Count > 0) {
                foreach (string attributeKey in htmlAttributes.Keys) {
                    attributes.AppendFormat($" {attributeKey}=\"{htmlAttributes[attributeKey]}\"");
                }
            }

            if (!string.IsNullOrWhiteSpace(htmlTag)) {
                return $"<{htmlTag}{attributes}>";
            }

            return "";
        }

        // Writes an closing tag
        private string HtmlClosingTag(string htmlTag) {
            if (!string.IsNullOrWhiteSpace(htmlTag)) {
                return $"</{htmlTag}>";
            }

            return "";
        }
    }
}