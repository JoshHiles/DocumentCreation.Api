using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace DocumentCreation.Api.Helpers
{
    public class PdfHelper
    {
    }

    public class CreateFooterAndHeaderEvent : PdfPageEventHelper
    {
        private readonly string _header;
        private readonly string _submissionDate;
        private readonly string _referenceNumber;

        public CreateFooterAndHeaderEvent(string header, string submissionDate, string referenceNumber)
        {
            _header = header;
            _submissionDate = submissionDate;
            _referenceNumber = referenceNumber;
        }

        public override void OnEndPage(PdfWriter writer, Document document)
        {
            PdfContentByte cb = writer.DirectContent;
            cb.SetFontAndSize(BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED),
                11.0f);
            cb.SetColorFill(BaseColor.GRAY);

            cb.BeginText();
            cb.SetTextMatrix(40, document.Top);
            cb.ShowText(_submissionDate);
            cb.EndText();

            cb.BeginText();
            cb.SetTextMatrix(200, document.Top);
            cb.ShowText(_header);
            cb.EndText();

            if (!string.IsNullOrEmpty(_referenceNumber))
            {
                cb.BeginText();
                cb.SetTextMatrix(380, document.Top);
                cb.ShowText(String.Format("ref no. {0}", _referenceNumber));
                cb.EndText();
            }

            cb.BeginText();
            cb.SetTextMatrix(540, document.BottomMargin);
            cb.ShowText(String.Format("{0}", writer.CurrentPageNumber));
            cb.EndText();
        }
    }
}
