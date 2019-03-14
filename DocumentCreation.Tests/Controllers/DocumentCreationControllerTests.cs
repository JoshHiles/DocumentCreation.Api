using DocumentCreation.Api.Controllers;
using DocumentCreation.Api.Controllers.Models;
using DocumentCreation.Api.Helpers;
using DocumentCreation.Api.Service;
using DocumentCreation.Api.Service.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace DocumentCreation.Tests.Controllers
{
    public class DocumentCreationControllerTests
    {
        private readonly Mock<IHandlebarHelper> _HandlebarHelper = new Mock<IHandlebarHelper>();
        private readonly Mock<IPdfCreationService> _PdfCreationHelper = new Mock<IPdfCreationService>();
        private readonly DocumentCreationController _controller;

        public DocumentCreationControllerTests()
        {
            _controller = new DocumentCreationController(_HandlebarHelper.Object, _PdfCreationHelper.Object);
        }

        [Fact]
        public void HandblebarPdf_CallsPdfService()
        {
            _PdfCreationHelper.Setup(x => x.CreatePdf(It.IsAny<HandlebarPdfEntity>()))
                .Returns("test");

            var entity = new HandlebarPdfRequestEntity
            {
                FileSavePath = "test",
                Header = "test",
                UniqueReference = "test",
                HandlebarHtmlTemplate = "test",
                JsonData = "test"
            };

            _controller.HandblebarPdf(entity);

            _PdfCreationHelper.Verify(x => x.CreatePdf(It.IsAny<HandlebarPdfEntity>()), Times.Once);
        }
    }
}
