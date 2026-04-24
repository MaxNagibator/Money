namespace Money.Web.Pages;

public partial class Home
{
    private readonly List<VersionHistoryEntry> _versionHistory =
    [
        new("1.3.14", new DateTime(2026, 4, 24), [
            new VersionChange("Исправлена ошибка рендера при отмене подсказанной категории в диалоге операции.", ChangeType.BugFix),
            new VersionChange("Чип с подсказанной категорией больше не вылезает за границы: длинное имя обрезается с троеточием, полное видно во всплывающей подсказке. Слово «Подставлено» перенесено в подсказку.", ChangeType.UiUx),
            new VersionChange("Крестик на чипе подсказанной категории подсвечивается красным при наведении и показывает подсказку «Убрать подстановку».", ChangeType.UiUx),
            new VersionChange("Убран вспомогательный текст под полем даты (Занимал много места).", ChangeType.UiUx),
        ]),

        new("1.3.13", new DateTime(2026, 4, 24), [
            new VersionChange("В поле даты добавлены короткие алиасы: «с» (сегодня), «з» (завтра), «в» (вчера), «пз» (послезавтра), «пв» (позавчера) и цепочки «ппз», «ппв» и т. д.", ChangeType.Feature),
        ]),

        new("1.3.12", new DateTime(2026, 4, 24), [
            new VersionChange("Исправлено зависание на экране загрузки у пользователей с нерусской локалью браузера: приложение не могло применить культуру ru-RU из-за неполных данных ICU.", ChangeType.BugFix),
        ]),

        new("1.3.11", new DateTime(2026, 4, 23), [
            new VersionChange("При открытии диалога операции (добавление или редактирование) фокус автоматически ставится на поле суммы. Ранее поле при этом отображалось пустым до первого клика мимо; теперь значение видно сразу.", ChangeType.UiUx),
        ]),

        new("1.3.10", new DateTime(2026, 4, 23), [
            new VersionChange("При вводе места в операции категория теперь подсказывается автоматически: если в 5 последних операциях по этому месту была одна и та же категория, появляется чип для быстрой подстановки.", ChangeType.Feature),
        ]),

        new("1.3.9", new DateTime(2026, 4, 23), [
            new VersionChange("В настройках отображения появился выбор чередования строк операций: подсветка фона или цветная полоса слева у каждой второй строки.", ChangeType.Feature),
            new VersionChange("Суммы в списке операций выравниваются так, что копейки стоят одна под другой. Ширина колонки и включение поведения настраиваются.", ChangeType.UiUx),
            new VersionChange("Кнопки удаления подсвечиваются красным при наведении.", ChangeType.UiUx),
            new VersionChange("Удалённая операция остаётся на своём месте с эффектом затухания и зачёркнутым текстом, а иконка корзины превращается в кнопку восстановления.", ChangeType.UiUx),
        ]),

        new("1.3.8", new DateTime(2026, 4, 22), [
            new VersionChange("Убрана поддержка PWA: приложение больше не устанавливается как отдельная программа и не работает в офлайне.", ChangeType.General),
        ]),

        new("1.3.7", new DateTime(2026, 4, 22), [
            new VersionChange("Цвета осей, сетки и легенды на графиках статистики теперь сразу принимают цвета новой темы при переключении светлой и тёмной.", ChangeType.BugFix),
        ]),

        new("1.3.6", new DateTime(2026, 4, 22), [
            new VersionChange("На экране ошибки добавлена кнопка «Повторить», а переход по меню автоматически сбрасывает состояние ошибки, так что перезагрузка страницы больше не требуется.", ChangeType.UiUx),
        ]),

        new("1.3.5", new DateTime(2026, 4, 22), [
            new VersionChange("Усилена безопасность аутентификации: refresh-токен теперь отзывается на сервере при выходе, параллельные запросы больше не ломают rotation refresh-токенов.", ChangeType.Improvement),
            new VersionChange("Исправлен открытый редирект: параметр returnUrl со ссылкой на чужой сайт больше не выполняется.", ChangeType.BugFix),
            new VersionChange("Поля текущего и нового пароля на странице профиля теперь скрывают ввод.", ChangeType.BugFix),
            new VersionChange("На странице регистрации при ошибке входа после успешной регистрации показывается корректное сообщение.", ChangeType.BugFix),
            new VersionChange("Исправлена утечка таймера на странице профиля при быстром переходе со страницы.", ChangeType.BugFix),
        ]),

        new("1.3.4", new DateTime(2026, 4, 22), [
            new VersionChange("Новые места и держатели долгов появляются в подсказках автодополнения сразу после сохранения, без перезагрузки страницы.", ChangeType.BugFix),
            new VersionChange("Поля «Место» и «Держатель» больше не подсвечиваются красной ошибкой «Заполни меня», пока в них вводится текст.", ChangeType.BugFix),
        ]),

        new("1.3.3", new DateTime(2026, 4, 22), [
            new VersionChange("Исправлена потеря введённой даты при клике по иконке календаря: при неверном формате поле остаётся в режиме правки и показывает ошибку. Также принимаются даты без ведущего нуля (например, 5.12.2025).", ChangeType.BugFix),
            new VersionChange("В поле даты добавлено распознавание «позавчера», «послезавтра» и их цепочек («позапозавчера», «послепослезавтра» и т. д.).", ChangeType.Feature),
        ]),

        new("1.3.2", new DateTime(2026, 4, 22), [
            new VersionChange("Исправлен сброс выбранного диапазона дат в фильтре операций при уходе на другую страницу и возврате обратно.", ChangeType.BugFix),
        ]),

        new("1.3.1", new DateTime(2026, 4, 22), [
            new VersionChange("Добавлена подсказка ранее заполненных держателей при создании долга.", ChangeType.Feature),
            new VersionChange("Исправлено сворачивание окна оплаты долга при наведении курсора.", ChangeType.BugFix),
            new VersionChange("Сумма платежа предзаполняется остатком долга.", ChangeType.UiUx),
        ]),

        new("1.3.0", new DateTime(2026, 4, 21), [
            new VersionChange("Исправлена ошибка со сдвигом даты операций, долгов и событий на предыдущий день в часовых поясах к востоку от UTC.", ChangeType.BugFix),
            new VersionChange("Переход на .NET 10.", ChangeType.Improvement),
        ]),

        new("1.2.12", new DateTime(2025, 12, 25), [
            new VersionChange("Исправлена ошибка редактирования категории: вместо сохранения изменений создавалась новая.", ChangeType.BugFix),
        ]),

        new("1.2.11", new DateTime(2025, 11, 19), [
            new VersionChange("Выполнен переход на Chart.js для отрисовки графиков.", ChangeType.Improvement),
            new VersionChange("Добавлена настройка использования цветов темы в графиках.", ChangeType.Feature),
            new VersionChange("Улучшена поддержка тем оформления в столбчатых и круговых диаграммах.", ChangeType.UiUx),
        ]),

        new("1.2.10", new DateTime(2025, 11, 11), [
            new VersionChange("Переделан компонент выбора места в операциях.", ChangeType.Improvement),
            new VersionChange("Приоритизирован текстовый ввод в компоненте выбора даты.", ChangeType.UiUx),
            new VersionChange("Удалена интеграция с Aspire.", ChangeType.General),
            new VersionChange("Исправлена плавающая проблема со StaticWebAssets.", ChangeType.BugFix),
            new VersionChange("Добавлена проверка наличия email при регистрации.", ChangeType.Improvement),
        ]),

        new("1.2.9", new DateTime(2025, 9, 17), [
            new VersionChange("Скорректировано выравнивание кнопок и сделан обязательным email при регистрации через внешние провайдеры.", ChangeType.UiUx),
        ]),

        new("1.2.8", new DateTime(2025, 8, 20), [
            new VersionChange("Интегрированы внешние провайдеры аутентификации: Auth и GitHub.", ChangeType.Feature),
            new VersionChange("Добавлены кнопки входа для Auth и GitHub на странице входа.", ChangeType.UiUx),
        ]),

        new("1.2.7", new DateTime(2025, 7, 21), [
            new VersionChange("Добавлена возможность выбора долгов для прощения.", ChangeType.Feature),
            new VersionChange("Реализованы действия \"выбрать все\" и \"очистить\" для долгов.", ChangeType.Feature),
            new VersionChange("Добавлена проверка долгов перед прощением.", ChangeType.Improvement),
            new VersionChange("Обновлён интерфейс кнопки прощения долгов.", ChangeType.UiUx),
        ]),

        new("1.2.6", new DateTime(2025, 7, 21), [
            new VersionChange("Добавлена возможность отображения оплаченных долгов.", ChangeType.Feature),
            new VersionChange("Внесены изменения в стиль отображения карточек оплаченных долгов.", ChangeType.UiUx),
        ]),

        new("1.2.5", new DateTime(2025, 7, 11), [
            new VersionChange("Добавлена настройка времени жизни токенов.", ChangeType.Feature),
            new VersionChange("Исправлены возможные проблемы со скопами токенов.", ChangeType.BugFix),
            new VersionChange("Исправлены возможные проблемы с обновлением состояния аутентификации.", ChangeType.BugFix),
            new VersionChange("Выполнен рефакторинг системы аутентификации.", ChangeType.Improvement),
        ]),

        new("1.2.4", new DateTime(2025, 7, 11), [
            new VersionChange("Добавлен компонент SmartDatePicker для улучшения работы с датами.", ChangeType.Feature),
            new VersionChange("Реализовано ручное переключение месяцев в датах.", ChangeType.Improvement),
        ]),

        new("1.2.3", new DateTime(2025, 5, 13), [
            new VersionChange("Исправлена ошибка создания регулярной операции не на ту дату.", ChangeType.BugFix),
            new VersionChange("Удалены ошибочно перенесённые операции.", ChangeType.BugFix),
        ]),

        new("1.2.2", new DateTime(2025, 5, 6), [
            new VersionChange("Исправление ошибки добавления операций.", ChangeType.BugFix),
            new VersionChange("Исправление ошибки редактирования удаления всех сущностей.", ChangeType.BugFix),
        ]),

        new("1.2.1.7", new DateTime(2025, 5, 3), [
            new VersionChange("Редизайн.", ChangeType.UiUx),
        ]),

        new("0.8.2", new DateTime(2023, 9, 30), [
            new VersionChange("В шаблонах документов добавлена трансформация в PDF.", ChangeType.Feature),
        ]),

        new("0.8.1", new DateTime(2023, 1, 12), [
            new VersionChange("Добавлен раздел \"Шаблоны документов\".", ChangeType.Feature),
        ]),

        new("0.7.3", new DateTime(2022, 7, 19), [
            new VersionChange("Исправлена проблема с датой по умолчанию при добавлении платежей в часовом поясе отличном от часового пояса сервера.", ChangeType.BugFix),
        ]),

        new("0.7.2", new DateTime(2022, 7, 7), [
            new VersionChange("В шаблоны операции добавлено поле сортировки.", ChangeType.Feature),
            new VersionChange("Выделение суммы операции после добавления из шаблона.", ChangeType.Improvement),
        ]),

        new("0.7.1", new DateTime(2022, 4, 11), [
            new VersionChange("Добавлены шаблоны операций.", ChangeType.Feature),
            new VersionChange("Поиск в списке категорий теперь работает.", ChangeType.BugFix),
        ]),

        new("0.6.6", new DateTime(2021, 7, 11), [
            new VersionChange("В регулярные платежи добавлено \"место\".", ChangeType.Feature),
        ]),

        new("0.6.5.1", new DateTime(2021, 7, 10), [
            new VersionChange("\"Дни без операций\" отображаются если включить опцию.", ChangeType.Feature),
        ]),

        new("0.6.5", new DateTime(2021, 7, 9), [
            new VersionChange("Исправлена ошибка null в заголовке суммарного значения по типу, при отсутствии операций с данным типом.", ChangeType.BugFix),
            new VersionChange("Дни без операций отображаются, если они позже самой ранее операции и раньше самой последней операции в выборке.", ChangeType.Feature),
        ]),

        new("0.6.4", new DateTime(2021, 2, 22), [
            new VersionChange("Обновление категории для отображаемых на странице операций", ChangeType.Improvement),
        ]),

        new("0.6.3.3", new DateTime(2020, 5, 6), [
            new VersionChange("Исправлена ошибка с редактированием операции", ChangeType.BugFix),
        ]),

        new("0.6.3.2", new DateTime(2020, 4, 29), [
            new VersionChange("Исправлены ошибка отправки почты при регистрации", ChangeType.BugFix),
            new VersionChange("Доработка умных подсказиков для места операции", ChangeType.Improvement),
        ]),

        new("0.6.3.1", new DateTime(2020, 4, 19), [
            new VersionChange("Редактирование суммы операций теперь тоже поддерживает формат 3*2+2*2 (запишется 10)", ChangeType.Feature),
            new VersionChange("В операциях добавлено поле \"Место\" для заполнения и поиска (более менее умный подсказик к нему)", ChangeType.Feature),
            new VersionChange("Исправлен баг в выборе текущей недели(если воскресенье, показывалась следующая неделя)", ChangeType.BugFix),
        ]),

        new("0.5.9.5", new DateTime(2019, 12, 21), [
            new VersionChange("Исправлена ошибка запуска регулярный ежемесячный задач в декабре", ChangeType.BugFix),
            new VersionChange("Добавлена иконочка у операций созданных регулярной задачей", ChangeType.UiUx),
        ]),

        new("0.5.9.4", new DateTime(2019, 10, 20), [
            new VersionChange("Диаграмки в операция учитывают начало месяца и начало недели. Чуть ифнормативнее стало", ChangeType.Improvement),
        ]),

        new("0.5.9.3", new DateTime(2019, 8, 23), [
            new VersionChange("Поле для сортировки в категориях", ChangeType.Feature),
            new VersionChange("Режим просмотра операций \"месяц\" \"год\" с 1 числа месяца или года соответственно", ChangeType.Feature),
        ]),

        new("0.5.9.2", new DateTime(2019, 7, 14), [
            new VersionChange("График по датам улучшен", ChangeType.Improvement),
            new VersionChange("Проблемы с отрицательной суммой в графиках исправлены", ChangeType.BugFix),
        ]),

        new("0.5.9", new DateTime(2019, 7, 13), [
            new VersionChange("Улучшено юзабилити смены даты", ChangeType.UiUx),
            new VersionChange("Графики статистики, юзерфрендли", ChangeType.UiUx),
        ]),

        new("0.5.5", new DateTime(2019, 6, 16), [
            new VersionChange("Функционал: показывать оплаченные долги", ChangeType.Feature),
            new VersionChange("Функционал: перенос долгов в расходы", ChangeType.Feature),
            new VersionChange("Функционал: слияние держателей долга", ChangeType.Feature),
        ]),

        new("0.5.1", new DateTime(2019, 4, 9), [
            new VersionChange("Функционал: поиск по категориям с выбором вместо текста", ChangeType.Feature),
        ]),

        new("0.5", new DateTime(2019, 4, 8), [
            new VersionChange("Функционал: Добавлены регулярные операции", ChangeType.Feature),
        ]),

        new("0.4.1", new DateTime(2018, 7, 4), [
            new VersionChange("Андройд: удаление операций", ChangeType.Feature),
        ]),

        new("0.4", new DateTime(2018, 6, 28), [
            new VersionChange("Андройд: бюджетная версия", ChangeType.Feature),
        ]),

        new("0.2.1", new DateTime(2018, 4, 11), [
            new VersionChange("Операции: теперь можно заполнять свой доход", ChangeType.Feature),
        ]),

        new("0.2", new DateTime(2018, 4, 6), [
            new VersionChange("Юзабили: редактирование операций стало более комфортным", ChangeType.UiUx),
            new VersionChange("Сервисная часть: измененение архитектуры приложения, могут быть ошибки", ChangeType.General),
        ]),

        new("0.1.9.1", new DateTime(2018, 1, 9), [
            new VersionChange("Юзабили: редирект на страницу логина при переходе по ссылке в запретные места", ChangeType.UiUx),
        ]),

        new("0.1.8.9", new DateTime(2017, 12, 26), [
            new VersionChange("Операции: поиск по категории и комменатрию", ChangeType.Feature),
            new VersionChange("Юзабили: в разделе авто", ChangeType.UiUx),
        ]),

        new("0.1.8.8", new DateTime(2017, 10, 6), [
            new VersionChange("Новый раздел: авто", ChangeType.Feature),
            new VersionChange("Юзабили: везде по чуть чуть", ChangeType.UiUx),
        ]),

        new("0.1.8.5", new DateTime(2017, 7, 17), [
            new VersionChange("Прикручиваем андройд: произошло измененение сервисной части, возможны перебои в работе", ChangeType.General),
        ]),

        new("0.1.8.4", new DateTime(2017, 7, 16), [
            new VersionChange("Категории: изменена сортировка", ChangeType.Improvement),
            new VersionChange("Категории: добавлен компонент для выбора цвета", ChangeType.Feature),
            new VersionChange("Категории: немного изменено редактирование", ChangeType.UiUx),
        ]),

        new("0.1.8.3", new DateTime(2017, 5, 31), [
            new VersionChange("Аккаунт: добавление почты при регистрации", ChangeType.Feature),
            new VersionChange("Аккаунт: возможность смены пароля", ChangeType.Feature),
            new VersionChange("Отзывы: возможность оставлять, смотреть", ChangeType.Feature),
            new VersionChange("Чуток красоты в логине", ChangeType.UiUx),
        ]),

        new("0.1.8.2", new DateTime(2017, 5, 30), [
            new VersionChange("Долги: датапикеры на поля типа \"дата\"", ChangeType.UiUx),
            new VersionChange("Операции: датапикеры на поля типа \"дата\"", ChangeType.UiUx),
            new VersionChange("Операции: плюс небольшой график", ChangeType.Feature),
        ]),

        new("0.1.8.1", new DateTime(2017, 5, 27), [
            new VersionChange("Долги: группировка по имени", ChangeType.Feature),
            new VersionChange("Долги: удаление", ChangeType.Feature),
            new VersionChange("Долги: редактирование", ChangeType.Feature),
        ]),

        new("0.1.7", new DateTime(2017, 5, 26), []),
    ];

    private bool _historyVisible;

    private void ChangeHistoryVisible()
    {
        _historyVisible = !_historyVisible;
    }

    private string GetCurrentVersion()
    {
        return _versionHistory.FirstOrDefault()?.Version ?? "unknown";
    }

    private Color GetVersionColor(VersionHistoryEntry entry)
    {
        return entry.Changes.Length switch
        {
            0 => Color.Default,
            1 => Color.Info,
            2 or 3 => Color.Primary,
            var _ => Color.Secondary,
        };
    }

    private string GetVersionIcon(VersionHistoryEntry entry)
    {
        return entry.Changes.Length switch
        {
            0 => Icons.Material.Rounded.Circle,
            1 => Icons.Material.Rounded.FiberManualRecord,
            var _ => Icons.Material.Rounded.RadioButtonChecked,
        };
    }

    private Color GetVersionChipColor(VersionHistoryEntry entry)
    {
        string[] versionParts = entry.Version.Split('.');

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
            var _ => Icons.Material.Rounded.Circle,
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
            var _ => Color.Default,
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
