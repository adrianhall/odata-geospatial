using Microsoft.AspNetCore.Mvc;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.OData.Edm.Validation;
using System.Text;
using System.Xml;
using System;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class MetadataHandler : Controller
    {
        [HttpGet("/api/$meta")]
        public async Task Get()
        {
            IEdmModel edmModel = ModelCache.GetEdmModel(typeof(Country));

            HttpContext context = Request.HttpContext;
            Request.HttpContext.Response.ContentType = "application/xml";

            using (StringWriter sw = new StringWriter())
            {
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Encoding = Encoding.UTF8;
                settings.Indent = true; // for better readability

                using (XmlWriter xw = XmlWriter.Create(sw, settings))
                {
                    IEnumerable<EdmError> errors;
                    CsdlWriter.TryWriteCsdl(edmModel, xw, CsdlTarget.OData, out errors);
                    xw.Flush();
                }

                string output = sw.ToString();
                await context.Response.WriteAsync(output).ConfigureAwait(false);
            }
        }
    }
}
