using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DitaDotNet {
    public class DitaToHtmlConverter : DitaConverter {
        private class DitaTableColumnSpec {
            public string Name { get; set; }
            public string Width { get; set; }
            public int Number { get; set; }

            public string WidthAsPercent() {
                if (Width?.Length > 1 && (Width?.Contains("*") ?? false)) {
                    return Width.Replace("*", "%");
                }

                return null;
            }
        }

        #region Properties

        // Maintain the states of the current table

        private int TableColumnIndex { get; set; }
        private DitaTableColumnSpec[] TableColumnSpecs { get; set; }
        private int TableRowColumnIndex { get; set; }
        private List<DitaPageSectionJson> Sections { get; set; }
        private DitaPageSectionJson CurrentSection { get; set; }

        #endregion Properties

        #region Public Methods

        public bool Convert(DitaElement bodyElement, List<DitaPageSectionJson> sections, out string body) {
            StringBuilder bodyStringBuilder = new StringBuilder();
            Sections = sections;

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

        #endregion Public Methods

        #region Private Methods

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
                case "b": return "strong";
                case "colspec":
                    TableColumnIndex++;
                    TableColumnSpecs[TableColumnIndex] = new DitaTableColumnSpec();
                    TableColumnSpecs[TableColumnIndex].Number = (TableColumnIndex + 1);
                    return "";
                case "entry":
                    TableRowColumnIndex++;
                    if (element.Parent?.Parent?.Type == "thead") {
                        return "th";
                    }

                    return "td";
                case "fig": return "figure";
                case "image": return "img";
                case "row":
                    TableRowColumnIndex = -1;
                    return "tr";
                case "table":
                    TableColumnIndex = -1;
                    TableColumnSpecs = null;
                    break;
                case "tgroup": return "";
                case "title":
                    if (element.Parent?.Type == "section") {
                        // Create a reference to this section, if this is the title of the section
                        if (CurrentSection != null) {
                            if (string.IsNullOrEmpty(CurrentSection.Title) && !string.IsNullOrEmpty(CurrentSection.Anchor)) {
                                CurrentSection.Title = element.InnerText;
                                Sections.Add(CurrentSection);
                                CurrentSection = null;
                            }
                        }

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
                case "colspec":
                    if (key == "colname") {
                        TableColumnSpecs[TableColumnIndex].Name = value;
                    }

                    if (key == "colwidth") {
                        TableColumnSpecs[TableColumnIndex].Width = value;
                    }

                    if (key == "colnum") {
                        if (int.TryParse(value, out int colnum)) {
                            TableColumnSpecs[TableColumnIndex].Number = colnum;
                        }
                    }

                    break;
                case "entry":
                    if (key == "morerows") {
                        if (int.TryParse(value, out int rowspan)) {
                            return ("rowspan", $"{rowspan + 1}");
                        }
                    }

                    if (key == "valign") {
                        return (key, value);
                    }

                    break;
                case "image":
                    if (key == "href") {
                        return ("src", $"%IMG_ROOT%/{value}");
                    }

                    break;
                case "section":
                    if (key == "id") {
                        CurrentSection = new DitaPageSectionJson {Anchor = value};
                        return (key, value);
                    }

                    break;
                case "tgroup":
                    if (key == "cols") {
                        if (int.TryParse(value, out int columns)) {
                            TableColumnSpecs = new DitaTableColumnSpec[columns];
                        }
                    }

                    break;
            }

            return ("", "");
        }

        // Add additional attributes to specific html tags
        private void AddHtmlTagAttributes(Dictionary<string, string> htmlAttributes, DitaElement element) {
            switch (element.Type) {
                case "image":
                    // All images should be full column width
                    if (!htmlAttributes.ContainsKey("width")) {
                        htmlAttributes.Add("width", "100%");
                    }

                    break;
                case "table":
                    // Add the generic "table" class to all tables
                    if (!htmlAttributes.ContainsKey("class")) {
                        htmlAttributes.Add("class", "table");
                    }

                    break;
                case "entry":
                    // If there is a width defined, add it to the entry
                    if (element.Attributes.ContainsKey("colname")) {
                        string colname = element.Attributes["colname"];
                        if (!htmlAttributes.ContainsKey("width")) {
                            string widthAsPercent = TableColumnSpecs?.FirstOrDefault(o => o?.Name == colname)?.WidthAsPercent();
                            if (!string.IsNullOrEmpty(widthAsPercent)) {
                                htmlAttributes.Add("width", widthAsPercent);
                            }
                        }
                    }

                    // If there is a colspan defined, add it to the entry
                    if (element.Attributes.ContainsKey("namest") && element.Attributes.ContainsKey("nameend")) {
                        // Build the colspan
                        int startColumn = TableColumnSpecs?.FirstOrDefault(o => o.Name == element.Attributes["namest"])?.Number ?? -1;
                        int endColumn = TableColumnSpecs?.FirstOrDefault(o => o.Name == element.Attributes["nameend"])?.Number ?? -1;

                        if (startColumn >= 0 && endColumn >= 0) {
                            if (!htmlAttributes.ContainsKey("colspan")) {
                                htmlAttributes.Add("colspan", $"{endColumn - startColumn + 1}");
                            }
                        }
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

        #endregion Private Methods
    }
}