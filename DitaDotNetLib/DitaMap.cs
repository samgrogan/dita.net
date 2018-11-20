﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dita.Net
{
    public class DitaMap : DitaFile
    {
        // Default constructor
        public DitaMap(DitaFile ditaFile) : base() {

        }

        // Does the given DOCTYPE match this object?
        public new static bool IsMatchingDocType(string docType)
        {
            if (!string.IsNullOrWhiteSpace(docType))
            {
                return (docType.Contains("map") && docType.Contains("map.dtd") && docType.Contains("-//OASIS//DTD DITA Map//"));
            }
            return false;
        }
    }
}
