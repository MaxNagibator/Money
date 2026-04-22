https://www.mudblazor.com/features/icons#icons - иконки муд айкона
https://www.mudblazor.com/utilities/spacing#how-it-works - css муд классы

## Аутентификация

Используется самописный клиент поверх OpenIddict (а не `Microsoft.AspNetCore.Components.WebAssembly.Authentication`),
потому что стандартный пакет не поддерживает `grant_type=password` и кастомный `grant_type=external`, используемые в
этом проекте. Следствия и компромиссы:

- **Password grant.** По OAuth 2.1 / RFC 9700 этот flow считается устаревшим. Здесь он приемлем, так как бэкенд является
  first-party Authorization Server + Resource Server одновременно. Внешние входы (Auth, GitHub) идут через PKCE на
  бэкенде.
- **Хранение токенов в `localStorage`.** Компромисс простоты против XSS-риска: `httpOnly` cookie потребовал бы
  куки-сессию, BFF или отдельный proxy. CSP и аудит зависимостей должны компенсировать этот выбор.
- **Access token без шифрования (`DisableAccessTokenEncryption`).** Рекомендация Microsoft для публичных SPA-клиентов:
  клиент должен уметь читать `exp` из JWT локально для проактивного refresh. Шифрование (JWE) бессмысленно, так как
  токен всё равно видим JavaScript-коду.
- **Refresh с rotation.** Параллельные запросы сериализуются `SemaphoreSlim` в `RefreshTokenService`, чтобы OpenIddict
  не инвалидировал refresh-токены из-за повторного использования.
- **Logout с revoke.** `AuthenticationService.LogoutAsync` вызывает `connect/revoke` (RFC 7009) до очистки
  `localStorage`, чтобы refresh-токен нельзя было переиспользовать при XSS.
- **Returnurl.** Все редиректы проходят через `NavigationManagerExtensions.ReturnToSafe`, который отбрасывает внешние
  URL (защита от open redirect).

(\s+)(?=\w+[-\w]*="[^"]*"|@\w+[-\w]*="[^"]*"|@\w+[-\w]*)
TODO: При добавлении операции вне фильтруемого диапазона, он отображается. На сайте аналогичное поведение.
dotnet workload restore

Вывод:
MudTooltip - неоптимизированный.
Крайне нежелательно использовать в нагруженных местах.
-60ms из 240ms = 180ms

Вариант дополнительной оптимизации
@*<MudIcon Color="type.Color"
Disabled="_dialog.IsOpen"
Icon="@Icons.Material.Rounded.Add"
@onclick="() => _dialog.ToggleOpen(type)"
Style="cursor: pointer;"
Size="Size.Small" />*@
