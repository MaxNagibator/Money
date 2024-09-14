using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Validation.AspNetCore;

namespace Money.Api.Controllers
{
    [ApiController]
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    [Route("[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(ILogger<CategoriesController> logger)
        {
            _logger = logger;
        }

        [HttpGet()]
        [Route("")]
        public string Get()
        {
            return "list";
        }

        [HttpGet()]
        [Route("{id}")]
        public string GetById(Guid id)
        {
            return "byid" + id;
        }

        [HttpPost]
        [Route("{id}")]
        public string Create(Guid id)
        {
            return "create" + id;
        }

        [HttpPut]
        [Route("{id}")]
        public string Update([FromQuery] Guid id)
        {
            return "update";
        }

        [HttpDelete]
        [Route("{id}")]
        public string Delete([FromQuery] Guid id)
        {
            return "delete";
        }
    }
}
