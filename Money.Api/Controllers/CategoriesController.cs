using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Money.Api.Dto;
using Money.Business.Enums;
using Money.Business.Services;
using OpenIddict.Validation.AspNetCore;

namespace Money.Api.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
[Route("[controller]")]
public class CategoriesController(
    ILogger<CategoriesController> logger,
    PaymentCategoryService paymentCategoryService) : ControllerBase
{
    [HttpGet]
    [Route("")]
    public async Task<GetCategoriesResponse> Get(PaymentTypes type)
    {
        var categories = await paymentCategoryService.Get(type);
        return new GetCategoriesResponse(categories);
    }

    [HttpGet]
    [Route("{id:guid}")]
    public string GetById(Guid id)
    {
        return "byid" + id;
    }

    [HttpPost]
    [Route("")]
    public async Task<int> CreateAsync([FromBody]CreatePaymentRequest request)
    {
        var business = request.GetBusinessModel();
        var id = await paymentCategoryService.Create(business);
        return id;
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
