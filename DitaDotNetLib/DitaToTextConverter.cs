using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dita.Net {
    class DitaToTextConverter {
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
            if (element.IsContainer) {
                StringBuilder elementStringBuilder = new StringBuilder();

                foreach (DitaElement childElement in element.Children) {
                    elementStringBuilder.AppendLine(Convert(childElement));
                }

                return elementStringBuilder.ToString();
            }
            else {
                return $"{element.InnerText}";
            }
        }
    }
}