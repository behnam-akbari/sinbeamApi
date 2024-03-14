using Microsoft.AspNetCore.Mvc;
using Scaphoid.Core.Model;

namespace Schaphoid.Api.Controllers
{
    public class AdminController : ControllerBase
    {
        [HttpGet]
        public Resource Get()
        {
            var resource = new Resource();

            resource.Links.Add(new Link("questions", Url.Action(nameof(QuestionController.Get),
            "Question", null,
            Request.Scheme),
            HttpMethods.Get));

            resource.Links.Add(new Link("requests", Url.Action(nameof(RequestController.Get),
            "Request", null,
            Request.Scheme),
            HttpMethods.Get));

            return resource;
        }
    }
}
