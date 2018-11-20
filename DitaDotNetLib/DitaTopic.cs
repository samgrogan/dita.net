using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dita.Net
{
    public class DitaTopic : DitaFile 
    {
        // Default constructor
        public DitaTopic(DitaFile ditaFile) : base()
        {

        }

        // Does the given DOCTYPE match this object?
        public new static bool IsMatchingDocType(string docType) {
            if (!string.IsNullOrWhiteSpace(docType)) {
                return (docType.Contains("topic") && docType.Contains("topic.dtd") && docType.Contains("-//OASIS//DTD DITA Topic//"));
            }
            return false;
        }

    }
}
