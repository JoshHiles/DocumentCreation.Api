using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DocumentCreation.Api.Controllers.Models;
using DocumentCreation.Api.Helpers;
using DocumentCreation.Api.Service.Models;
using iText.Html2pdf;
using iTextSharp.text;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;
using Document = iTextSharp.text.Document;

namespace DocumentCreation.Api.Service
{
    public interface IPdfCreationService
    {
        string CreatePdf(HandlebarPdfEntity model);
    }

    public class PdfCreationService : IPdfCreationService
    {
        public readonly IFileHelper FileHelper;

        public PdfCreationService(IFileHelper fileHelper)
        {
            FileHelper = fileHelper;
        }

        public readonly List<AdditionalDocuments> ImageList = new List<AdditionalDocuments>();
        public readonly List<AdditionalDocuments> PdfList = new List<AdditionalDocuments>();
        public readonly List<AdditionalDocuments> HtmlList = new List<AdditionalDocuments>();
        public readonly List<AdditionalDocuments> WordList = new List<AdditionalDocuments>();

        public string CreatePdf(HandlebarPdfEntity model)
        {
            var bytes = CreateInitialPdf(model);

            var dirPath = model.FileSavePath + "/" + model.EscapedUniqueReference;
            Directory.CreateDirectory(dirPath);

            if (model.AdditionalDocuments != null)
            {
                SplitAdditionalDocuments(model.AdditionalDocuments);
                bytes = AddAdditionalDocument(bytes, model.AdditionalDocuments);

                if (WordList != null)
                {
                    CreateWordDocuments(dirPath, model.EscapedUniqueReference);
                }
            }

            FileHelper.StreamToFile(dirPath, model.EscapedUniqueReference, "pdf", new MemoryStream(bytes));

            return dirPath;
        }

        public void SplitAdditionalDocuments(List<AdditionalDocuments> additionalDocuments)
        {
            ImageList.Clear();
            PdfList.Clear();
            HtmlList.Clear();
            WordList.Clear();

            ImageList.AddRange(additionalDocuments.Where(_ =>
                _.Type == AdditionalDocumentTypes.Jpeg || 
                _.Type == AdditionalDocumentTypes.Jpg || 
                _.Type == AdditionalDocumentTypes.Png
                )
            );

            PdfList.AddRange(additionalDocuments.Where(_ => _.Type == AdditionalDocumentTypes.Pdf));

            HtmlList.AddRange(additionalDocuments.Where(_ => _.Type == AdditionalDocumentTypes.Html || 
                                                             _.Type == AdditionalDocumentTypes.Htm
                                                             ));

            WordList.AddRange(additionalDocuments.Where(_ => _.Type == AdditionalDocumentTypes.Doc || 
                                                             _.Type == AdditionalDocumentTypes.Docx || 
                                                             _.Type == AdditionalDocumentTypes.Rtf
                                                             ));
        }

        public byte[] CreateInitialPdf(HandlebarPdfEntity model)
        {
            var memoryStream = new MemoryStream();
            var document = new Document();
            var writer = PdfWriter.GetInstance(document, memoryStream);

            document.Open();

            writer.PageEvent = new CreateFooterAndHeaderEvent(model.Header, DateTime.Now.ToString("d"), model.UniqueReference);

            var htmlWorker = new HTMLWorker(document);
            var sr = new StringReader(model.Html);
            var styles = new StyleSheet();
            styles.LoadStyle("li", "list-style-type", "circle");

            htmlWorker.SetStyleSheet(styles);
            htmlWorker.Parse(sr);

            document.Close();

            return memoryStream.ToArray();
        }

        public byte[] AddAdditionalDocument(byte[] orginialDocument, List<AdditionalDocuments> additionalDocuments)
        {
            var additionalDocumentBytes = new List<byte[]>
            {
                orginialDocument
            };

            additionalDocumentBytes.AddRange(AttachImages(ImageList));

            additionalDocumentBytes.AddRange(AttachPdfs(PdfList));

            additionalDocumentBytes.AddRange(AttachHtml(HtmlList));


            var memoryStreamPdf = new MemoryStream();
            var documentPdf = new Document();
            var pdfCopy = new PdfCopy(documentPdf, memoryStreamPdf);

            documentPdf.Open();
            foreach (var additionalDocumentByte in additionalDocumentBytes)
            {
                var pdfReader = new PdfReader(additionalDocumentByte);
                var numberOfPages = pdfReader.NumberOfPages;
                for (var page = 0; page < numberOfPages;)
                {
                    pdfCopy.AddPage(pdfCopy.GetImportedPage(pdfReader, ++page));
                }
            }
            documentPdf.Close();

            var mergedPdfBytes = memoryStreamPdf.ToArray();

            return mergedPdfBytes;
        }

        public List<byte[]> AttachImages(List<AdditionalDocuments> images)
        {
            var imageBytes = new List<byte[]>();

            foreach (var image in images)
            {
                var base64 = Convert.FromBase64String(image.Base64Document).ToArray();

                var memoryStreamImage = new MemoryStream();
                var documentImage = new Document();
                var writer = PdfWriter.GetInstance(documentImage, memoryStreamImage);

                documentImage.Open();
                var picture = Image.GetInstance(base64);

                if (picture.Height > picture.Width)
                {
                    var percentage = 700 / picture.Height;
                    picture.ScalePercent(percentage * 100);
                }
                else
                {
                    var percentage = 540 / picture.Width;
                    picture.ScalePercent(percentage * 100);
                }

                documentImage.Add(picture);
                documentImage.Close();
                imageBytes.Add(memoryStreamImage.ToArray());
            }
            return imageBytes;
        }

        public List<byte[]> AttachPdfs(List<AdditionalDocuments> pdfDocs)
        {
            var pdfBytes = new List<byte[]>();

            foreach (var pdf in pdfDocs)
            {
                var base64 = Convert.FromBase64String(pdf.Base64Document).ToArray();

                pdfBytes.Add(base64);
            }

            return pdfBytes;
        }

        public List<byte[]> AttachHtml(List<AdditionalDocuments> htmlDocs)
        {
            var htmlBytes = new List<byte[]>();

            foreach (var html in htmlDocs)
            {
                var memoryStream = new MemoryStream();

                byte[] data = Convert.FromBase64String(html.Base64Document);
                string decodedString = Encoding.UTF8.GetString(data);

                HtmlConverter.ConvertToPdf(decodedString, memoryStream);

                htmlBytes.Add(memoryStream.ToArray());
            }

            return htmlBytes;
        }

        public void CreateWordDocuments(string dirPath, string escapedUniqueRef)
        {
            int count = 1;
            foreach (var doc in WordList)
            {
                FileHelper.StreamToFile(dirPath, escapedUniqueRef + "-Additional-Document-" + count, doc.Type.ToString(), new MemoryStream(Convert.FromBase64String(doc.Base64Document)));
                count++;
            }
        }
    }
}