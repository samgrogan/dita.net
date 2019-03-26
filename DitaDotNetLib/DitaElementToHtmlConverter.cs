using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DitaDotNet {
    public class DitaElementToHtmlConverter {
        private class DitaTableColumnSpec {
            public string Name { get; set; }
            public string Width { get; set; }
            public int Number { get; set; }
        }

        #region Properties

        // The collection we are converting
        private DitaCollection Collection { get; }

        // Maintain the states of the current table

        private int TableColumnIndex { get; set; }
        private DitaTableColumnSpec[] _tableColumnSpecs;
        private int TableRowColumnIndex { get; set; }
        private List<DitaPageSectionJson> Sections { get; set; }
        private DitaPageSectionJson CurrentSection { get; set; }
        private string FileName { get; set; }

        #endregion Properties

        #region Public Methods

        public DitaElementToHtmlConverter(DitaCollection collection) {
            Collection = collection;
        }

        public bool Convert(DitaElement bodyElement, List<DitaPageSectionJson> sections, string fileName, out string body) {
            StringBuilder bodyStringBuilder = new StringBuilder();
            Sections = sections;
            FileName = fileName;

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
            StringBuilder elementStringBuilder = new StringBuilder();

            // Does this element need to be wrapped by another element?
            DitaElement prependElement = PrependDitaElementIfNeeded(element);
            if (prependElement != null) {
                elementStringBuilder.Append(Convert(prependElement));
            }

            // Determine the new html tag and attributes
            string htmlTag = ConvertDitaTagToHtmlTag(element);
            Dictionary<string, string> htmlAttributes = ConvertDitaTagAttributesToHtmlTagAttributes(element);
            AddHtmlTagAttributes(htmlAttributes, element);

            // If this is a parent, then add the children
            if (element.IsContainer) {
                elementStringBuilder.Append(HtmlOpeningTag(htmlTag, htmlAttributes));

                foreach (DitaElement childElement in element.Children) {
                    elementStringBuilder.Append(Convert(childElement));
                }

                elementStringBuilder.Append(HtmlClosingTag(htmlTag));
            }
            else {
                elementStringBuilder.Append($"{HtmlOpeningTag(htmlTag, htmlAttributes)}{element.InnerText}{HtmlClosingTag(htmlTag)}");
            }

            return elementStringBuilder.ToString();
        }

        // Takes a DITA "tag" and returns the corresponding HTML tag
        private string ConvertDitaTagToHtmlTag(DitaElement element) {
            switch (element.Type) {
                case "b": return "strong";
                case "colspec":
                    TableColumnIndex++;
                    FixUpTableColumnSpecs();
                    _tableColumnSpecs[TableColumnIndex] = new DitaTableColumnSpec {
                        Number = (TableColumnIndex + 1)
                    };

                    return "";
                case "entry":
                    TableRowColumnIndex++;
                    if (element.Parent?.Parent?.Type == "thead" || element.AttributeValueOrDefault("class", "") == "th") {
                        return "th";
                    }

                    return "td";
                case "fig": return "figure";
                case "image":
                    // Is this referring to an SVG or other type of image?
                    if (IsImageElementSvg(element)) {
                        return "object";
                    }

                    return "img";
                case "keyword": return "";
                case "row":
                    TableRowColumnIndex = -1;
                    return "tr";
                case "table":
                    TableColumnIndex = -1;
                    _tableColumnSpecs = null;
                    break;
                case "tgroup":
                    TableColumnIndex = -1;
                    _tableColumnSpecs = null;
                    return "";
                case "title":
                    if (element.Parent?.Type == "section") {
                        // Create a reference to this section, if this is the title of the section
                        if (CurrentSection != null) {
                            if (string.IsNullOrEmpty(CurrentSection.Title) && !string.IsNullOrEmpty(CurrentSection.Anchor)) {
                                CurrentSection.Title = element.ToString();
                                Sections.Add(CurrentSection);
                                CurrentSection = null;
                            }
                        }

                        return "h3";
                    }

                    return "h4";
                case "xref":
                    return "a";
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
                case "a":
                    return (key, value);
                case "colspec":
                    if (key == "colname") {
                        FixUpTableColumnSpecs();
                        _tableColumnSpecs[TableColumnIndex].Name = value;
                    }

                    if (key == "colwidth") {
                        FixUpTableColumnSpecs();
                        _tableColumnSpecs[TableColumnIndex].Width = value;
                    }

                    if (key == "colnum") {
                        if (int.TryParse(value, out int colnum)) {
                            FixUpTableColumnSpecs();
                            _tableColumnSpecs[TableColumnIndex].Number = colnum;
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
                        if (IsImageElementSvg(element)) {
                            return ("data", ImageUrlFromHref(value));
                        }

                        return ("src", ImageUrlFromHref(value));
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
                            _tableColumnSpecs = new DitaTableColumnSpec[columns];
                        }
                    }

                    break;
                case "xref":
                    if (key == "href") {
                        return (key, UrlFromXref(element));
                    }

                    break;
            }

            return ("", "");
        }

        // Add additional attributes to specific html tags
        private void AddHtmlTagAttributes(Dictionary<string, string> htmlAttributes, DitaElement element) {
            switch (element.Type) {
                case "image":
                    if (IsImageElementSvg(element)) {
                        // Add the type of embedded svg
                        if (!htmlAttributes.ContainsKey("type")) {
                            htmlAttributes.Add("type", "image/svg+xml");
                        }
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
                            string widthAsPercent = ColumnWidthAsPercent(colname);
                            if (!string.IsNullOrEmpty(widthAsPercent)) {
                                htmlAttributes.Add("width", widthAsPercent);
                            }
                        }
                    }

                    // If there is a colspan defined, add it to the entry
                    if (element.Attributes.ContainsKey("namest") && element.Attributes.ContainsKey("nameend")) {
                        // Build the colspan
                        int startColumn = _tableColumnSpecs?.FirstOrDefault(o => o.Name == element.Attributes["namest"])?.Number ?? -1;
                        int endColumn = _tableColumnSpecs?.FirstOrDefault(o => o.Name == element.Attributes["nameend"])?.Number ?? -1;

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

        // Returns the relative or absolute url from a Dita XREF for use in an html A tag
        private string UrlFromXref(DitaElement xrefElement) {
            // What is the scope
            string scope = xrefElement.AttributeValueOrDefault("scope", null);
            string format = xrefElement.AttributeValueOrDefault("format", null);
            string href = xrefElement.AttributeValueOrDefault("href", null);

            if (scope == "external") {
                return href;
            }

            if (!string.IsNullOrEmpty(href)) {
                string result = null;
                if (href[0] == '#') {
                    // Link to the same page
                    if (href.Contains('/')) {
                        string[] anchorSplit = href.Split('/');
                        if (anchorSplit.Length > 1) {
                            result = $"#{anchorSplit[1]}";
                        }
                    }
                    else {
                        result = href.Substring(1);
                    }
                }
                else if (href.ToLowerInvariant().StartsWith("http")) {
                    result = href;
                }
                else {
                    // Split by hash, if any
                    string[] hashSplit = href.Split('#');

                    // Try to find the topic it is linking to
                    DitaFile referenceFile = Collection?.GetFileByName(hashSplit[0]);
                    if (referenceFile != null) {
                        result = $"%DOCUMENT_ROOT%/{Path.GetFileNameWithoutExtension(referenceFile.NewFileName)}";
                        if (hashSplit.Length > 1) {
                            result += $"#{hashSplit[1]}";
                        }
                    }
                    else {
                        Trace.TraceError($"Xref refers to unknown local file {hashSplit[0]}");
                    }
                }

                if (!string.IsNullOrEmpty(result)) {
                    return result;
                }
            }

            Trace.TraceWarning($"Unknown xref scope: {scope}, format: {format}, href: {href}");
            return "#";
        }

        // Translates an image url. Rewrite some images types
        private string ImageUrlFromHref(string inputHref) {
            string outputHref = inputHref;

            if (!string.IsNullOrEmpty(inputHref)) {
                // Is this a pdf?
                if (Path.GetExtension(inputHref) == ".pdf") {
                    outputHref = Path.ChangeExtension(inputHref, ".png");

                    // Does file exist
                    if (Collection.GetFileByName(outputHref) == null) {
                        // Try replacing -high with -source
                        if (outputHref.Contains("-high")) {
                            // Fixes an issue where source png files are converted to pdf - we want to go back to the source pngs
                            outputHref = outputHref.Replace("-high", "-source");
                        }

                        if (Collection.GetFileByName(outputHref) == null) {
                            // Try replacing -source with -low
                            if (outputHref.Contains("-source")) {
                                // Fixes an issue where source png files are converted to pdf - we want to go back to the source pngs
                                outputHref = outputHref.Replace("-source", "-low");
                            }
                        }
                    }
                }

                // Rename .image to .svg
                if (Path.GetExtension(inputHref) == ".image") {
                    outputHref = Path.ChangeExtension(inputHref, ".svg");
                }
            }

            return $"%IMG_ROOT%/{outputHref}";
        }

        // Wraps a dita element in another dita element, if needed to output correct html
        // For instance, wrap tables, sections, figures, etc in an <a name="..."> for instance
        private DitaElement PrependDitaElementIfNeeded(DitaElement inputElement) {
            string id = inputElement.AttributeValueOrDefault("id", null);
            // If this object has an id, wrap it in an a, name
            if (!string.IsNullOrEmpty(id)) {
                DitaElement outputElement = new DitaElement("a", false, null, inputElement.Parent, inputElement.PreviousSibling);
                outputElement.Attributes.Add("name", id);
                return outputElement;
            }

            return null;
        }

        // Does any fix up needed for the TableColumnSpecs due to markup errors
        private void FixUpTableColumnSpecs() {
            if (_tableColumnSpecs == null) {
                if (TableColumnIndex >= 0) {
                    _tableColumnSpecs = new DitaTableColumnSpec[TableColumnIndex + 1];
                }
            }
            else {
                if (TableColumnIndex >= _tableColumnSpecs.Length) {
                    Array.Resize(ref _tableColumnSpecs, TableColumnIndex + 1);
                    Trace.TraceWarning($"Resized table column specs in {FileName}");
                }
            }
        }

        // Does a given image element refer to SVG?
        private bool IsImageElementSvg(DitaElement imageElement) {
            if (imageElement != null) {
                if (imageElement.Type == "image") {
                    string extension = Path.GetExtension(imageElement.AttributeValueOrDefault("href", ""));

                    if ((extension?.Equals(".svg", StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (extension?.Equals(".image", StringComparison.OrdinalIgnoreCase) ?? false)) {
                        return true;
                    }
                }
            }

            return false;
        }

        // Returns the width of a given column, as a percent
        private string ColumnWidthAsPercent(string colname) {
            // Get the total width of all the columns
            double total = 0.0;
            double columnValue = 1.0;
            foreach (DitaTableColumnSpec columnSpec in _tableColumnSpecs) {
                double value = 1.0;
                if (!string.IsNullOrWhiteSpace(columnSpec.Width)) {
                    double.TryParse(columnSpec.Width.Replace("*", "").Replace("%", ""), out value);
                }

                value = Math.Max(value, 1.0);

                if (columnSpec.Name == colname) {
                    columnValue = value;
                }

                total += value;
            }

            if (total.Equals(0.0) || columnValue.Equals(0.0)) {
                return "";
            }

            return $"{(columnValue / total * 100.0)}%";
        }

        #endregion Private Methods
    }
}