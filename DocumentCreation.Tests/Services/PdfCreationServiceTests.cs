using DocumentCreation.Api.Service;
using System;
using System.Collections.Generic;
using DocumentCreation.Api.Controllers.Models;
using DocumentCreation.Api.Helpers;
using DocumentCreation.Api.Service.Models;
using DocumentCreation.Tests.TestData;
using Moq;
using Xunit;

namespace DocumentCreation.Tests.Services
{
    public class PdfCreationServiceTests
    {
        private readonly Mock<IFileHelper> _fileHelper = new Mock<IFileHelper>();
        private readonly PdfCreationService _service;

        public PdfCreationServiceTests()
        {
            _service = new PdfCreationService(_fileHelper.Object);
        }

        [Fact]
        public void AddAdditionalDocument_ReturnsBytes()
        {
            var entity = new HandlebarPdfEntity
            {
                Html = "<h1>Test</h1>",
                FileSavePath = "Test",
                Header = "Test",
                UniqueReference = "Test"
            };

            var initialPdfResponse = _service.CreateInitialPdf(entity);

            var imageString = BaseStrings.ReturnImageString();

            var additionalDocuments = new List<AdditionalDocuments>
            {
                new AdditionalDocuments
                {
                    Base64Document = imageString,
                    Type = AdditionalDocumentTypes.Jpeg
                },
                new AdditionalDocuments
                {
                    Base64Document = imageString,
                    Type = AdditionalDocumentTypes.Jpg
                },
                new AdditionalDocuments
                {
                    Base64Document = imageString,
                    Type = AdditionalDocumentTypes.Png
                },
                new AdditionalDocuments
                {
                    Base64Document = BaseStrings.ReturnPdfString(),
                    Type = AdditionalDocumentTypes.Pdf
                },
                new AdditionalDocuments
                {
                    Base64Document = BaseStrings.ReturnHtmlString(),
                    Type = AdditionalDocumentTypes.Html
                }
            };

            var response = _service.AddAdditionalDocument(initialPdfResponse, additionalDocuments);

            Assert.IsType<byte[]>(initialPdfResponse);
            Assert.NotEmpty(initialPdfResponse);

            Assert.IsType<byte[]>(response);
            Assert.NotEmpty(response);
        }

        [Fact]
        public void CreateInitialPdf_ReturnsBytes()
        {
            var entity = new HandlebarPdfEntity
            {
                Html = "<h1>Test</h1>",
                FileSavePath = "Test",
                Header = "Test",
                UniqueReference = "Test"
            };

            var response = _service.CreateInitialPdf(entity);

            Assert.IsType<byte[]>(response);
            Assert.NotEmpty(response);
        }

        [Fact]
        public void SplitAdditionalDocuments()
        {
            var imageString = BaseStrings.ReturnImageString();

            var additionalDocuments = new List<AdditionalDocuments>
            {
                new AdditionalDocuments
                {
                    Base64Document = imageString,
                    Type = AdditionalDocumentTypes.Jpeg
                },
                new AdditionalDocuments
                {
                    Base64Document = imageString,
                    Type = AdditionalDocumentTypes.Jpg
                },
                new AdditionalDocuments
                {
                    Base64Document = imageString,
                    Type = AdditionalDocumentTypes.Png
                },
                new AdditionalDocuments
                {
                    Base64Document = BaseStrings.ReturnPdfString(),
                    Type = AdditionalDocumentTypes.Pdf
                },
                new AdditionalDocuments
                {
                    Base64Document = BaseStrings.ReturnHtmlString(),
                    Type = AdditionalDocumentTypes.Html
                },
                new AdditionalDocuments
                {
                    Base64Document = BaseStrings.ReturnWordString(),
                    Type = AdditionalDocumentTypes.Docx
                },
                new AdditionalDocuments
                {
                    Base64Document = BaseStrings.ReturnRtfString(),
                    Type = AdditionalDocumentTypes.Rtf
                }
            };

            _service.SplitAdditionalDocuments(additionalDocuments);

            Assert.NotEmpty(_service.ImageList);
            Assert.NotEmpty(_service.PdfList);
            Assert.NotEmpty(_service.HtmlList);
            Assert.NotEmpty(_service.WordList);
            Assert.Equal(2, _service.WordList.Count);
        }

        [Fact]
        public void AttachImages_ReturnsByteArray()
        {
            var imageString = BaseStrings.ReturnImageString();

            var additionalDocuments = new List<AdditionalDocuments>
            {
                new AdditionalDocuments
                {
                    Base64Document = imageString,
                    Type = AdditionalDocumentTypes.Jpeg
                },
                new AdditionalDocuments
                {
                    Base64Document = imageString,
                    Type = AdditionalDocumentTypes.Jpg
                },
                new AdditionalDocuments
                {
                    Base64Document = imageString,
                    Type = AdditionalDocumentTypes.Png
                }
            };

            var response = _service.AttachImages(additionalDocuments);

            Assert.IsType<List<byte[]>>(response);
            Assert.NotEmpty(response);
        }

        [Fact]
        public void AttachPdfs_ReturnsByteArray()
        {
            var pdfString = BaseStrings.ReturnPdfString();

            var additionalDocuments = new List<AdditionalDocuments>
            {
                new AdditionalDocuments
                {
                    Base64Document = pdfString,
                    Type = AdditionalDocumentTypes.Pdf
                }
            };

            var response = _service.AttachPdfs(additionalDocuments);

            Assert.IsType<List<byte[]>>(response);
            Assert.NotEmpty(response);
        }

        [Fact]
        public void AttachHtml_ReturnsByteArray()
        {
            var htmlString = BaseStrings.ReturnHtmlString();

            var additionalDocuments = new List<AdditionalDocuments>
            {
                new AdditionalDocuments
                {
                    Base64Document = htmlString,
                    Type = AdditionalDocumentTypes.Html
                }
            };

            var response = _service.AttachHtml(additionalDocuments);

            Assert.IsType<List<byte[]>>(response);
            Assert.NotEmpty(response);
        }
    }
}
