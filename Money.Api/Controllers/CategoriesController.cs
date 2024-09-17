using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Money.Api.Dto;
using Money.Business.Models;
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
    public async Task<GetCategoriesResponse> Get([FromQuery] int? type, CancellationToken cancellationToken)
    {
        ICollection<PaymentCategory> categories = await paymentCategoryService.GetAsync(type, cancellationToken);
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
    public async Task<int> CreateAsync([FromBody] CreatePaymentRequest request, CancellationToken cancellationToken)
    {
        PaymentCategory business = request.GetBusinessModel();
        int id = await paymentCategoryService.CreateAsync(business, cancellationToken);
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
