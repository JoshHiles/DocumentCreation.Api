using Newtonsoft.Json.Linq;
using HandlebarsDotNet;

namespace DocumentCreation.Api.Helpers
{
    public interface IHandlebarHelper
    {
        string DataTemplateToHtml(string template, string data);
    }

    public class HandlebarHelper : IHandlebarHelper
    {
        public string DataTemplateToHtml(string template, string data)
        {
            var compiledTemplate = Handlebars.Compile(template);
            var parsedData = JObject.Parse(data);
            var htmlResult = compiledTemplate(parsedData);
            return htmlResult;
        }
    }
}
