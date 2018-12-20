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
            string htmlTag = ConvertDitaTagToHtmlTag(element.Type);

            if (element.IsContainer) {
                StringBuilder elementStringBuilder = new StringBuilder();
                elementStringBuilder.Append(HtmlOpeningTag(htmlTag));

                foreach (DitaElement childElement in element.Children) {
                    elementStringBuilder.Append(Convert(childElement));
                }

                elementStringBuilder.Append(HtmlClosingTag(htmlTag));

                return elementStringBuilder.ToString();
            }
            else {
                return $"{HtmlOpeningTag(htmlTag)}{element.InnerText}{HtmlClosingTag(htmlTag)}";
            }
        }

        // Takes a DITA "tag" and returns the corresponding HTML tag
        private string ConvertDitaTagToHtmlTag(string ditaTag) {
            switch (ditaTag) {
                case "#text":
                    return "";
            }

            return ditaTag;
        }

        // Writes an open tag
        private string HtmlOpeningTag(string htmlTag) {
            if (!string.IsNullOrWhiteSpace(htmlTag)) {
                return $"<{htmlTag}>";
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