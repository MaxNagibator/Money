https://www.mudblazor.com/features/icons#icons - иконки муд айкона
https://www.mudblazor.com/utilities/spacing#how-it-works - css муд классы

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
