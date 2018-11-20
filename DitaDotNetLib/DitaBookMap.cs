using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dita.Net
{
    class DitaBookMap : DitaMap
    {
        // Default constructor
        public DitaBookMap(DitaFile ditaFile) : base(ditaFile)
        {
            
        }

        // Does the given DOCTYPE match this object?
        public new static bool IsMatchingDocType(string docType)
        {
            if (!string.IsNullOrWhiteSpace(docType))
            {
                return (docType.Contains("bookmap") && docType.Contains("bookmap.dtd") && docType.Contains("-//OASIS//DTD DITA BookMap//"));
            }
            return false;
        }
    }
}
