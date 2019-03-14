using DocumentCreation.Api.Helpers;
using Moq;
using Xunit;

namespace DocumentCreation.Tests.Helpers
{
    public class HandlebarHelperTests
    {
        private readonly HandlebarHelper _helper;


        public HandlebarHelperTests()
        {
            _helper = new HandlebarHelper();
        }

        [Fact]
        public void DataTemplateToHtml_ReturnsProcessedString()
        {
            var template = "<h1>{{test}}</h1>";
            var data = "{\"test\":\"test\"}";

            var response = _helper.DataTemplateToHtml(template, data);

            Assert.Equal("<h1>test</h1>", response);
        }

    }
}
