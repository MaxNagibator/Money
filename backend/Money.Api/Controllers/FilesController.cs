using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Money.Api.Dto.Files;
using OpenIddict.Validation.AspNetCore;

namespace Money.Api.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
[Route("[controller]")]
public class FilesController(FilesService service) : ControllerBase
{
    /// <summary>
    /// Загрузить файл.
    /// </summary>
    /// <param name="file">Файл для загрузки.</param>
    /// <param name="cancellationToken">Токен отмены запроса.</param>
    /// <returns>Информация о файле.</returns>
    [HttpPost]
    public async Task<FileDto> UploadFile(IFormFile file, CancellationToken cancellationToken)
    {
        var uploadedFile = await service.Upload(file, cancellationToken);
        return FileDto.FromBusinessModel(uploadedFile);
    }
}
