using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Money.Api.Dto.Files;
using Money.Business;
using Money.Business.Services;
using OpenIddict.Validation.AspNetCore;

namespace Money.Api.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
[Route("[controller]")]
public class FileController(FileService fileService) : ControllerBase
{
    [HttpPost]
    public async Task<FileDto> UploadFile(IFormFile file)
    {
        var uploadedFile = await fileService.Upload(file);
        return FileDto.FromBusinessModel(uploadedFile);
    }
}