namespace Money.Web.Pages;

public partial class Home
{
    private readonly List<VersionHistoryEntry> _versionHistory =
    [
        new("1.2.8", new(2025, 8, 20), [
            new("Интегрированы внешние провайдеры аутентификации: Auth и GitHub.", ChangeType.Feature),
            new("Добавлены кнопки входа для Auth и GitHub на странице входа.", ChangeType.UiUx),
        ]),

        new("1.2.7", new(2025, 7, 21), [
            new("Добавлена возможность выбора долгов для прощения.", ChangeType.Feature),
            new("Реализованы действия \"выбрать все\" и \"очистить\" для долгов.", ChangeType.Feature),
            new("Добавлена проверка долгов перед прощением.", ChangeType.Improvement),
            new("Обновлён интерфейс кнопки прощения долгов.", ChangeType.UiUx),
        ]),

        new("1.2.6", new(2025, 7, 21), [
            new("Добавлена возможность отображения оплаченных долгов.", ChangeType.Feature),
            new("Внесены изменения в стиль отображения карточек оплаченных долгов.", ChangeType.UiUx),
        ]),

        new("1.2.5", new(2025, 7, 11), [
            new("Добавлена настройка времени жизни токенов.", ChangeType.Feature),
            new("Исправлены возможные проблемы со скопами токенов.", ChangeType.BugFix),
            new("Исправлены возможные проблемы с обновлением состояния аутентификации.", ChangeType.BugFix),
            new("Выполнен рефакторинг системы аутентификации.", ChangeType.Improvement),
        ]),

        new("1.2.4", new(2025, 7, 11), [
            new("Добавлен компонент SmartDatePicker для улучшения работы с датами.", ChangeType.Feature),
            new("Реализовано ручное переключение месяцев в датах.", ChangeType.Improvement),
        ]),

        new("1.2.3", new(2025, 5, 13), [
            new("Исправлена ошибка создания регулярной операции не на ту дату.", ChangeType.BugFix),
            new("Удалены ошибочно перенесённые операции.", ChangeType.BugFix),
        ]),

        new("1.2.2", new(2025, 5, 6), [
            new("Исправление ошибки добавления операций.", ChangeType.BugFix),
            new("Исправление ошибки редактирования удаления всех сущностей.", ChangeType.BugFix),
        ]),

        new("1.2.1.7", new(2025, 5, 3), [
            new("Редизайн.", ChangeType.UiUx),
        ]),

        new("0.8.2", new(2023, 9, 30), [
            new("В шаблонах документов добавлена трансформация в PDF.", ChangeType.Feature),
        ]),

        new("0.8.1", new(2023, 1, 12), [
            new("Добавлен раздел \"Шаблоны документов\".", ChangeType.Feature),
        ]),

        new("0.7.3", new(2022, 7, 19), [
            new("Исправлена проблема с датой по умолчанию при добавлении платежей в часовом поясе отличном от часового пояса сервера.", ChangeType.BugFix),
        ]),

        new("0.7.2", new(2022, 7, 7), [
            new("В шаблоны операции добавлено поле сортировки.", ChangeType.Feature),
            new("Выделение суммы операции после добавления из шаблона.", ChangeType.Improvement),
        ]),

        new("0.7.1", new(2022, 4, 11), [
            new("Добавлены шаблоны операций.", ChangeType.Feature),
            new("Поиск в списке категорий теперь работает.", ChangeType.BugFix),
        ]),

        new("0.6.6", new(2021, 7, 11), [
            new("В регулярные платежи добавлено \"место\".", ChangeType.Feature),
        ]),

        new("0.6.5.1", new(2021, 7, 10), [
            new("\"Дни без операций\" отображаются если включить опцию.", ChangeType.Feature),
        ]),

        new("0.6.5", new(2021, 7, 9), [
            new("Исправлена ошибка null в заголовке суммарного значения по типу, при отсутствии операций с данным типом.", ChangeType.BugFix),
            new("Дни без операций отображаются, если они позже самой ранее операции и раньше самой последней операции в выборке.", ChangeType.Feature),
        ]),

        new("0.6.4", new(2021, 2, 22), [
            new("Обновление категории для отображаемых на странице операций", ChangeType.Improvement),
        ]),

        new("0.6.3.3", new(2020, 5, 6), [
            new("Исправлена ошибка с редактированием операции", ChangeType.BugFix),
        ]),

        new("0.6.3.2", new(2020, 4, 29), [
            new("Исправлены ошибка отправки почты при регистрации", ChangeType.BugFix),
            new("Доработка умных подсказиков для места операции", ChangeType.Improvement),
        ]),

        new("0.6.3.1", new(2020, 4, 19), [
            new("Редактирование суммы операций теперь тоже поддерживает формат 3*2+2*2 (запишется 10)", ChangeType.Feature),
            new("В операциях добавлено поле \"Место\" для заполнения и поиска (более менее умный подсказик к нему)", ChangeType.Feature),
            new("Исправлен баг в выборе текущей недели(если воскресенье, показывалась следующая неделя)", ChangeType.BugFix),
        ]),

        new("0.5.9.5", new(2019, 12, 21), [
            new("Исправлена ошибка запуска регулярный ежемесячный задач в декабре", ChangeType.BugFix),
            new("Добавлена иконочка у операций созданных регулярной задачей", ChangeType.UiUx),
        ]),

        new("0.5.9.4", new(2019, 10, 20), [
            new("Диаграмки в операция учитывают начало месяца и начало недели. Чуть ифнормативнее стало", ChangeType.Improvement),
        ]),

        new("0.5.9.3", new(2019, 8, 23), [
            new("Поле для сортировки в категориях", ChangeType.Feature),
            new("Режим просмотра операций \"месяц\" \"год\" с 1 числа месяца или года соответственно", ChangeType.Feature),
        ]),

        new("0.5.9.2", new(2019, 7, 14), [
            new("График по датам улучшен", ChangeType.Improvement),
            new("Проблемы с отрицательной суммой в графиках исправлены", ChangeType.BugFix),
        ]),

        new("0.5.9", new(2019, 7, 13), [
            new("Улучшено юзабилити смены даты", ChangeType.UiUx),
            new("Графики статистики, юзерфрендли", ChangeType.UiUx),
        ]),

        new("0.5.5", new(2019, 6, 16), [
            new("Функционал: показывать оплаченные долги", ChangeType.Feature),
            new("Функционал: перенос долгов в расходы", ChangeType.Feature),
            new("Функционал: слияние держателей долга", ChangeType.Feature),
        ]),

        new("0.5.1", new(2019, 4, 9), [
            new("Функционал: поиск по категориям с выбором вместо текста", ChangeType.Feature),
        ]),

        new("0.5", new(2019, 4, 8), [
            new("Функционал: Добавлены регулярные операции", ChangeType.Feature),
        ]),

        new("0.4.1", new(2018, 7, 4), [
            new("Андройд: удаление операций", ChangeType.Feature),
        ]),

        new("0.4", new(2018, 6, 28), [
            new("Андройд: бюджетная версия", ChangeType.Feature),
        ]),

        new("0.2.1", new(2018, 4, 11), [
            new("Операции: теперь можно заполнять свой доход", ChangeType.Feature),
        ]),

        new("0.2", new(2018, 4, 6), [
            new("Юзабили: редактирование операций стало более комфортным", ChangeType.UiUx),
            new("Сервисная часть: измененение архитектуры приложения, могут быть ошибки", ChangeType.General),
        ]),

        new("0.1.9.1", new(2018, 1, 9), [
            new("Юзабили: редирект на страницу логина при переходе по ссылке в запретные места", ChangeType.UiUx),
        ]),

        new("0.1.8.9", new(2017, 12, 26), [
            new("Операции: поиск по категории и комменатрию", ChangeType.Feature),
            new("Юзабили: в разделе авто", ChangeType.UiUx),
        ]),

        new("0.1.8.8", new(2017, 10, 6), [
            new("Новый раздел: авто", ChangeType.Feature),
            new("Юзабили: везде по чуть чуть", ChangeType.UiUx),
        ]),

        new("0.1.8.5", new(2017, 7, 17), [
            new("Прикручиваем андройд: произошло измененение сервисной части, возможны перебои в работе", ChangeType.General),
        ]),

        new("0.1.8.4", new(2017, 7, 16), [
            new("Категории: изменена сортировка", ChangeType.Improvement),
            new("Категории: добавлен компонент для выбора цвета", ChangeType.Feature),
            new("Категории: немного изменено редактирование", ChangeType.UiUx),
        ]),

        new("0.1.8.3", new(2017, 5, 31), [
            new("Аккаунт: добавление почты при регистрации", ChangeType.Feature),
            new("Аккаунт: возможность смены пароля", ChangeType.Feature),
            new("Отзывы: возможность оставлять, смотреть", ChangeType.Feature),
            new("Чуток красоты в логине", ChangeType.UiUx),
        ]),

        new("0.1.8.2", new(2017, 5, 30), [
            new("Долги: датапикеры на поля типа \"дата\"", ChangeType.UiUx),
            new("Операции: датапикеры на поля типа \"дата\"", ChangeType.UiUx),
            new("Операции: плюс небольшой график", ChangeType.Feature),
        ]),

        new("0.1.8.1", new(2017, 5, 27), [
            new("Долги: группировка по имени", ChangeType.Feature),
            new("Долги: удаление", ChangeType.Feature),
            new("Долги: редактирование", ChangeType.Feature),
        ]),

        new("0.1.7", new(2017, 5, 26), []),
    ];

    private bool _historyVisible;

    private void ChangeHistoryVisible()
    {
        _historyVisible = !_historyVisible;
    }

    private string GetCurrentVersion()
    {
        return _versionHistory.FirstOrDefault()?.Version ?? "1.2.8";
    }

    private Color GetVersionColor(VersionHistoryEntry entry)
    {
        return entry.Changes.Length switch
        {
            0 => Color.Default,
            1 => Color.Info,
            2 or 3 => Color.Primary,
            _ => Color.Secondary,
        };
    }

    private string GetVersionIcon(VersionHistoryEntry entry)
    {
        return entry.Changes.Length switch
        {
            0 => Icons.Material.Rounded.Circle,
            1 => Icons.Material.Rounded.FiberManualRecord,
            _ => Icons.Material.Rounded.RadioButtonChecked,
        };
    }

    private Color GetVersionChipColor(VersionHistoryEntry entry)
    {
        var versionParts = entry.Version.Split('.');

        if (versionParts.Length < 2)
        {
            return Color.Default;
        }

        if (versionParts[0] != "0")
        {
            return Color.Success;
        }

        if (versionParts[1] != "0")
        {
            return Color.Primary;
        }

        return Color.Default;
    }

    private string GetChangeTypeIcon(ChangeType type)
    {
        return type switch
        {
            ChangeType.Feature => Icons.Material.Rounded.Add,
            ChangeType.BugFix => Icons.Material.Rounded.BugReport,
            ChangeType.Improvement => Icons.Material.Rounded.TrendingUp,
            ChangeType.UiUx => Icons.Material.Rounded.Palette,
            ChangeType.General => Icons.Material.Rounded.Settings,
            _ => Icons.Material.Rounded.Circle,
        };
    }

    private Color GetChangeTypeColor(ChangeType type)
    {
        return type switch
        {
            ChangeType.Feature => Color.Success,
            ChangeType.BugFix => Color.Error,
            ChangeType.Improvement => Color.Info,
            ChangeType.UiUx => Color.Warning,
            ChangeType.General => Color.Default,
            _ => Color.Default,
        };
    }
}

public record VersionHistoryEntry(string Version, DateTime Date, VersionChange[] Changes);

public record VersionChange(string Description, ChangeType Type);

public enum ChangeType
{
    Feature,
    BugFix,
    Improvement,
    UiUx,
    General,
}
