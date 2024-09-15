using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Validation.AspNetCore;

namespace Money.Api.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
[Route("[controller]")]
public class CategoriesController(ILogger<CategoriesController> logger) : ControllerBase
{
    [HttpGet]
    [Route("")]
    public string Get()
    {
        return "list";
    }

    [HttpGet]
    [Route("{id:guid}")]
    public string GetById(Guid id)
    {
        return "byid" + id;
    }

    [HttpPost]
    [Route("{id:guid}")]
    public string Create(Guid id)
    {
        return "create" + id;
    }

    [HttpPut]
    [Route("{id:guid}")]
    public string Update(Guid id)
    {
        return "update";
    }

    [HttpDelete]
    [Route("{id:guid}")]
    public string Delete(Guid id)
    {
        return "delete";
    }
}
