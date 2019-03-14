using System;
using DocumentCreation.Api.Controllers.Models;
using DocumentCreation.Api.Helpers;
using DocumentCreation.Api.Service;
using DocumentCreation.Api.Service.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocumentCreation.Api.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class DocumentCreationController : ControllerBase
    {
        public readonly IHandlebarHelper HandlebarHelper;
        public readonly IPdfCreationService PdfCreationService;

        public DocumentCreationController(IHandlebarHelper handlebarHelper, IPdfCreationService pdfCreationService)
        {
            HandlebarHelper = handlebarHelper;
            PdfCreationService = pdfCreationService;
        }

        [HttpPost]
        [Route("handlebar-pdf")]
        public ActionResult HandblebarPdf(HandlebarPdfRequestEntity model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var html = HandlebarHelper.DataTemplateToHtml(model.HandlebarHtmlTemplate, model.JsonData);
                var entity = new HandlebarPdfEntity
                {
                    Header = model.Header,
                    Html = html,
                    FileSavePath = model.FileSavePath,
                    UniqueReference = model.UniqueReference,
                    AdditionalDocuments = model.AdditionalDocuments
                };

                var response = PdfCreationService.CreatePdf(entity);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex);
            }
        }
    }
}
