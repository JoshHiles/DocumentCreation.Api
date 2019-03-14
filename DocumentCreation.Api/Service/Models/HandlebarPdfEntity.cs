using System.Collections.Generic;
using DocumentCreation.Api.Controllers.Models;

namespace DocumentCreation.Api.Service.Models
{
    public class HandlebarPdfEntity
    {
        public string Header { get; set; }

        public string UniqueReference { get; set; }

        public string EscapedUniqueReference => UniqueReference.Replace('/', '-');

        public string Html { get; set; }

        public string FileSavePath { get; set; }

        public List<AdditionalDocuments> AdditionalDocuments { get; set; }
    }
}
