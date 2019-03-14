using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DocumentCreation.Api.Controllers.Models
{
    public class HandlebarPdfRequestEntity
    {
        [Required]
        public string Header { get; set; }

        [Required]
        public string UniqueReference { get; set; }

        public string EscapedUniqueReference => UniqueReference.Replace('/', '-');

        [Required]
        public string JsonData { get; set; }

        [Required]
        public string HandlebarHtmlTemplate { get; set; }

        [Required]
        public string FileSavePath { get; set; }

        public List<AdditionalDocuments> AdditionalDocuments { get; set; }
    }

    public class AdditionalDocuments
    {
        public string Base64Document { get; set; }
        public AdditionalDocumentTypes Type { get; set; }
    }

    public enum AdditionalDocumentTypes
    {
        Jpg,
        Jpeg,
        Png,
        Pdf,
        Doc,
        Docx,
        Html,
        Htm,
        Rtf
    }
}
