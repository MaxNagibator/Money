using Money.Data.Entities;

namespace Money.Data;

public static class DatabaseSeeder
{
    public static List<Category> SeedCategories(int userId, out int lastIndex, int startIndex = 1)
    {
        var categories = new List<Category>
        {
            new()
            {
                UserId = userId,
                Name = "Продукты",
                Description = "Расходы на продукты питания",
                TypeId = 1,
                Color = "#FFCC00",
                SubCategories =
                [
                    new()
                    {
                        UserId = userId,
                        Name = "Фрукты и овощи",
                        Description = "Расходы на свежие фрукты и овощи",
                        TypeId = 1,
                        Color = "#FF9900",
                    },

                    new()
                    {
                        UserId = userId,
                        Name = "Мясо и рыба",
                        Description = "Расходы на мясные и рыбные продукты",
                        TypeId = 1,
                        Color = "#CC3300",
                    },

                    new()
                    {
                        UserId = userId,
                        Name = "Хлеб и выпечка",
                        Description = "Расходы на хлебобулочные изделия",
                        TypeId = 1,
                        Color = "#D2691E",
                    },

                    new()
                    {
                        UserId = userId,
                        Name = "Напитки",
                        Description = "Расходы на безалкогольные напитки и воду",
                        TypeId = 1,
                        Color = "#87CEEB",
                    },

                    new()
                    {
                        UserId = userId,
                        Name = "Сладости",
                        Description = "Расходы на кондитерские изделия",
                        TypeId = 1,
                        Color = "#FF69B4",
                    },
                ],
            },

            new()
            {
                UserId = userId,
                Name = "Транспорт",
                Description = "Расходы на транспорт",
                TypeId = 1,
                Color = "#0099CC",
                SubCategories =
                [
                    new()
                    {
                        UserId = userId,
                        Name = "Общественный транспорт",
                        Description = "Расходы на проезд в общественном транспорте",
                        TypeId = 1,
                        Color = "#007ACC",
                    },

                    new()
                    {
                        UserId = userId,
                        Name = "Такси",
                        Description = "Расходы на такси",
                        TypeId = 1,
                        Color = "#005B99",
                    },

                    new()
                    {
                        UserId = userId,
                        Name = "Бензин",
                        Description = "Заправка автомобиля",
                        TypeId = 1,
                        Color = "#4169E1",
                    },

                    new()
                    {
                        UserId = userId,
                        Name = "Парковка",
                        Description = "Оплата парковочных мест",
                        TypeId = 1,
                        Color = "#708090",
                    },
                ],
            },

            new()
            {
                UserId = userId,
                Name = "Развлечения",
                Description = "Расходы на развлечения и досуг",
                TypeId = 1,
                Color = "#FF3366",
                SubCategories =
                [
                    new()
                    {
                        UserId = userId,
                        Name = "Кино",
                        Description = "Расходы на посещение кинотеатров",
                        TypeId = 1,
                        Color = "#FF3366",
                    },

                    new()
                    {
                        UserId = userId,
                        Name = "Концерты",
                        Description = "Расходы на посещение концертов",
                        TypeId = 1,
                        Color = "#FF6699",
                    },

                    new()
                    {
                        UserId = userId,
                        Name = "Игры",
                        Description = "Покупка видеоигр и подписок",
                        TypeId = 1,
                        Color = "#9370DB",
                    },

                    new()
                    {
                        UserId = userId,
                        Name = "Книги",
                        Description = "Художественная литература и комиксы",
                        TypeId = 1,
                        Color = "#DEB887",
                    },
                ],
            },

            new()
            {
                UserId = userId,
                Name = "Коммунальные услуги",
                Description = "Расходы на коммунальные услуги",
                TypeId = 1,
                Color = "#66CC66",
                SubCategories =
                [
                    new()
                    {
                        UserId = userId,
                        Name = "Электричество",
                        Description = "Расходы на электроэнергию",
                        TypeId = 1,
                        Color = "#FFCC00",
                    },

                    new()
                    {
                        UserId = userId,
                        Name = "Вода",
                        Description = "Расходы на водоснабжение",
                        TypeId = 1,
                        Color = "#66CCFF",
                    },

                    new()
                    {
                        UserId = userId,
                        Name = "Интернет",
                        Description = "Оплата домашнего интернета",
                        TypeId = 1,
                        Color = "#00CED1",
                    },

                    new()
                    {
                        UserId = userId,
                        Name = "Газ",
                        Description = "Оплата газоснабжения",
                        TypeId = 1,
                        Color = "#FFA07A",
                    },
                ],
            },

            new()
            {
                UserId = userId,
                Name = "Одежда",
                Description = "Расходы на одежду и обувь",
                TypeId = 1,
                Color = "#FF6600",
                SubCategories =
                [
                    new()
                    {
                        UserId = userId,
                        Name = "Одежда для детей",
                        Description = "Расходы на детскую одежду",
                        TypeId = 1,
                        Color = "#FF9966",
                    },

                    new()
                    {
                        UserId = userId,
                        Name = "Одежда для взрослых",
                        Description = "Расходы на одежду для взрослых",
                        TypeId = 1,
                        Color = "#CC6600",
                    },

                    new()
                    {
                        UserId = userId,
                        Name = "Обувь",
                        Description = "Расходы на обувь",
                        TypeId = 1,
                        Color = "#8B4513",
                    },
                ],
            },

            new()
            {
                UserId = userId,
                Name = "Здоровье",
                Description = "Расходы на медицинские услуги и лекарства",
                TypeId = 1,
                Color = "#CC33FF",
                SubCategories =
                [
                    new()
                    {
                        UserId = userId,
                        Name = "Лекарства",
                        Description = "Покупка лекарственных средств",
                        TypeId = 1,
                        Color = "#DA70D6",
                    },

                    new()
                    {
                        UserId = userId,
                        Name = "Стоматология",
                        Description = "Стоматологические услуги",
                        TypeId = 1,
                        Color = "#BA55D3",
                    },
                ],
            },

            new()
            {
                UserId = userId,
                Name = "Образование",
                Description = "Расходы на обучение и курсы",
                TypeId = 1,
                Color = "#FFCCFF",
                SubCategories =
                [
                    new()
                    {
                        UserId = userId,
                        Name = "Онлайн-курсы",
                        Description = "Подписки и платные онлайн-курсы",
                        TypeId = 1,
                        Color = "#FFD700",
                    },

                    new()
                    {
                        UserId = userId,
                        Name = "Книги и материалы",
                        Description = "Учебники и учебные материалы",
                        TypeId = 1,
                        Color = "#DDA0DD",
                    },
                ],
            },

            new()
            {
                UserId = userId,
                Name = "Кафе и рестораны",
                Description = "Расходы на общественное питание",
                TypeId = 1,
                Color = "#FF4500",
                SubCategories =
                [
                    new()
                    {
                        UserId = userId,
                        Name = "Кафе",
                        Description = "Завтраки, обеды, кофейни",
                        TypeId = 1,
                        Color = "#FFA500",
                    },

                    new()
                    {
                        UserId = userId,
                        Name = "Рестораны",
                        Description = "Ужины в ресторанах",
                        TypeId = 1,
                        Color = "#FF6347",
                    },

                    new()
                    {
                        UserId = userId,
                        Name = "Доставка еды",
                        Description = "Заказ готовой еды на дом",
                        TypeId = 1,
                        Color = "#FF8C00",
                    },
                ],
            },

            new()
            {
                UserId = userId,
                Name = "Спорт",
                Description = "Расходы на занятия спортом",
                TypeId = 1,
                Color = "#32CD32",
                SubCategories =
                [
                    new()
                    {
                        UserId = userId,
                        Name = "Абонемент",
                        Description = "Абонементы в фитнес-центры и бассейн",
                        TypeId = 1,
                        Color = "#228B22",
                    },

                    new()
                    {
                        UserId = userId,
                        Name = "Инвентарь",
                        Description = "Спортивный инвентарь и экипировка",
                        TypeId = 1,
                        Color = "#6B8E23",
                    },
                ],
            },

            new()
            {
                UserId = userId,
                Name = "Подарки",
                Description = "Расходы на подарки близким",
                TypeId = 1,
                Color = "#FF1493",
            },

            new()
            {
                UserId = userId,
                Name = "Путешествия",
                Description = "Расходы на поездки и отпуск",
                TypeId = 1,
                Color = "#1E90FF",
                SubCategories =
                [
                    new()
                    {
                        UserId = userId,
                        Name = "Билеты",
                        Description = "Авиа и железнодорожные билеты",
                        TypeId = 1,
                        Color = "#6495ED",
                    },

                    new()
                    {
                        UserId = userId,
                        Name = "Отели",
                        Description = "Проживание в отелях и апартаментах",
                        TypeId = 1,
                        Color = "#4682B4",
                    },
                ],
            },

            new()
            {
                UserId = userId,
                Name = "Техника и гаджеты",
                Description = "Расходы на бытовую технику и электронику",
                TypeId = 1,
                Color = "#696969",
            },

            new()
            {
                UserId = userId,
                Name = "Зарплата",
                Description = "Основной источник дохода",
                TypeId = 2,
                Color = "#66B3FF",
                SubCategories =
                [
                    new()
                    {
                        UserId = userId,
                        Name = "Бонусы",
                        Description = "Бонусы от работодателя",
                        TypeId = 2,
                        Color = "#66B3FF",
                    },
                ],
            },

            new()
            {
                UserId = userId,
                Name = "Дополнительный доход",
                Description = "Доход от фриланса и подработок",
                TypeId = 2,
                Color = "#FFB366",
                SubCategories =
                [
                    new()
                    {
                        UserId = userId,
                        Name = "Фриланс",
                        Description = "Доход от фриланс-проектов",
                        TypeId = 2,
                        Color = "#FFB366",
                    },

                    new()
                    {
                        UserId = userId,
                        Name = "Курсы и тренинги",
                        Description = "Доход от проведения курсов и тренингов",
                        TypeId = 2,
                        Color = "#FFB3FF",
                    },
                ],
            },

            new()
            {
                UserId = userId,
                Name = "Инвестиции",
                Description = "Доход от инвестиций",
                TypeId = 2,
                Color = "#66FF66",
                SubCategories =
                [
                    new()
                    {
                        UserId = userId,
                        Name = "Дивиденды",
                        Description = "Доход от акций и инвестиций",
                        TypeId = 2,
                        Color = "#FF6666",
                    },
                ],
            },

            new()
            {
                UserId = userId,
                Name = "Пассивный доход",
                Description = "Доход от аренды и других источников",
                TypeId = 2,
                Color = "#FF6666",
                SubCategories =
                [
                    new()
                    {
                        UserId = userId,
                        Name = "Аренда недвижимости",
                        Description = "Доход от аренды квартир или домов",
                        TypeId = 2,
                        Color = "#66FF66",
                    },
                ],
            },

            new()
            {
                UserId = userId,
                Name = "Премии",
                Description = "Дополнительные выплаты и бонусы",
                TypeId = 2,
                Color = "#FFCC00",
                SubCategories =
                [
                    new()
                    {
                        UserId = userId,
                        Name = "Премии за достижения",
                        Description = "Премии за выполнение планов и целей",
                        TypeId = 2,
                        Color = "#FFCC00",
                    },
                ],
            },

            new()
            {
                UserId = userId,
                Name = "Возврат долга",
                Description = "Возвращённые ранее одолженные средства",
                TypeId = 2,
                Color = "#90EE90",
            },

            new()
            {
                UserId = userId,
                Name = "Денежный подарок",
                Description = "Подаренные на праздники средства",
                TypeId = 2,
                Color = "#FFB6C1",
            },
        };

        lastIndex = SetCategoryIds(categories, ref startIndex);
        return categories;
    }

    public static (List<Operation> operations, List<Place> places) SeedOperations(int userId, List<Category> categories, int startIndex = 0, int placeStartIndex = 0)
    {
        var places = SeedPlaces(userId, placeStartIndex);

        var categoryDictionary = GetAllCategories(categories).ToDictionary(x => x.Name, x => x.Id);
        var placeDictionary = places.ToDictionary(x => x.Name, x => x.Id);

        List<Operation> operations =
        [
            new()
            {
                UserId = userId,
                Id = startIndex + 1,
                Sum = 150000.00m,
                CategoryId = GetCategoryId("Зарплата"),
                Comment = "Зарплата за сентябрь",
                Date = new(2023, 9, 30),
                PlaceId = GetPlaceId("Работа"),
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 2,
                Sum = 2000.00m,
                CategoryId = GetCategoryId("Продукты"),
                Comment = "Покупка продуктов в супермаркете",
                Date = new(2023, 10, 01),
                PlaceId = GetPlaceId("Супермаркет"),
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 3,
                Sum = 5000.00m,
                CategoryId = GetCategoryId("Концерты"),
                Comment = "Билет на концерт",
                Date = new(2023, 10, 05),
                PlaceId = GetPlaceId("Концертный зал"),
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 4,
                Sum = 30000.00m,
                CategoryId = GetCategoryId("Аренда недвижимости"),
                Comment = "Аренда квартиры за октябрь",
                Date = new(2023, 10, 01),
                PlaceId = GetPlaceId("Квартира"),
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 5,
                Sum = 10000.00m,
                CategoryId = GetCategoryId("Здоровье"),
                Comment = "Посещение врача",
                Date = new(2023, 10, 10),
                PlaceId = GetPlaceId("Поликлиника"),
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 6,
                Sum = 705.00m,
                CategoryId = GetCategoryId("Развлечения"),
                Comment = "Подписка на стриминговый сервис",
                Date = new(2023, 10, 15),
                PlaceId = null,
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 7,
                Sum = 500.00m,
                CategoryId = GetCategoryId("Пассивный доход"),
                Comment = "Дивиденды от акций",
                Date = new(2023, 10, 20),
                PlaceId = null,
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 8,
                Sum = 1200.00m,
                CategoryId = GetCategoryId("Коммунальные услуги"),
                Comment = "Оплата за электричество",
                Date = new(2023, 10, 25),
                PlaceId = null,
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 9,
                Sum = 25000.00m,
                CategoryId = GetCategoryId("Образование"),
                Comment = "Оплата курсов по программированию",
                Date = new(2023, 10, 30),
                PlaceId = GetPlaceId("Учебный центр"),
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 10,
                Sum = 80.00m,
                CategoryId = GetCategoryId("Транспорт"),
                Comment = "Оплата проезда на автобусе",
                Date = new(2023, 10, 28),
                PlaceId = null,
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 11,
                Sum = 3000.00m,
                CategoryId = GetCategoryId("Электричество"),
                Comment = "Оплата за электричество за сентябрь",
                Date = new(2023, 9, 28),
                PlaceId = null,
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 12,
                Sum = 15000.00m,
                CategoryId = GetCategoryId("Одежда"),
                Comment = "Покупка новой одежды",
                Date = new(2023, 10, 12),
                PlaceId = GetPlaceId("Магазин одежды"),
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 13,
                Sum = 4000.00m,
                CategoryId = GetCategoryId("Дивиденды"),
                Comment = "Дивиденды от инвестиций в акции",
                Date = new(2023, 10, 22),
                PlaceId = null,
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 14,
                Sum = 900.00m,
                CategoryId = GetCategoryId("Развлечения"),
                Comment = "Посещение кинотеатра",
                Date = new(2023, 10, 18),
                PlaceId = GetPlaceId("Кинотеатр"),
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 15,
                Sum = 25000.00m,
                CategoryId = GetCategoryId("Фриланс"),
                Comment = "Оплата за фриланс-проект",
                Date = new(2023, 10, 15),
                PlaceId = null,
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 16,
                Sum = 60.00m,
                CategoryId = GetCategoryId("Общественный транспорт"),
                Comment = "Оплата проезда на метро",
                Date = new(2023, 10, 29),
                PlaceId = null,
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 17,
                Sum = 50000.00m,
                CategoryId = GetCategoryId("Инвестиции"),
                Comment = "Инвестиции в стартап",
                Date = new(2023, 10, 05),
                PlaceId = null,
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 18,
                Sum = 35000.00m,
                CategoryId = GetCategoryId("Премии за достижения"),
                Comment = "Премия за успешный проект",
                Date = new(2023, 10, 30),
                PlaceId = null,
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 19,
                Sum = 12000.00m,
                CategoryId = GetCategoryId("Премии"),
                Comment = "Премия за выполнение плана",
                Date = new(2023, 10, 31),
                PlaceId = null,
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 20,
                Sum = 800.00m,
                CategoryId = GetCategoryId("Мясо и рыба"),
                Comment = "Покупка мяса на ужин",
                Date = new(2023, 10, 20),
                PlaceId = GetPlaceId("Магазин продуктов"),
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 21,
                Sum = 150000.00m,
                CategoryId = GetCategoryId("Зарплата"),
                Comment = "Зарплата за октябрь",
                Date = new(2023, 10, 31),
                PlaceId = GetPlaceId("Работа"),
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 22,
                Sum = 30000.00m,
                CategoryId = GetCategoryId("Аренда недвижимости"),
                Comment = "Аренда квартиры за ноябрь",
                Date = new(2023, 11, 01),
                PlaceId = GetPlaceId("Квартира"),
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 23,
                Sum = 2500.00m,
                CategoryId = GetCategoryId("Продукты"),
                Comment = "Недельная закупка продуктов",
                Date = new(2023, 11, 03),
                PlaceId = GetPlaceId("Супермаркет"),
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 24,
                Sum = 850.00m,
                CategoryId = GetCategoryId("Фрукты и овощи"),
                Comment = "Яблоки и овощи на неделю",
                Date = new(2023, 11, 05),
                PlaceId = GetPlaceId("Магазин продуктов"),
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 25,
                Sum = 1200.00m,
                CategoryId = GetCategoryId("Кафе"),
                Comment = "Кофе с коллегами",
                Date = new(2023, 11, 07),
                PlaceId = GetPlaceId("Кофейня"),
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 26,
                Sum = 3500.00m,
                CategoryId = GetCategoryId("Бензин"),
                Comment = "Заправка автомобиля",
                Date = new(2023, 11, 12),
                PlaceId = GetPlaceId("АЗС"),
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 27,
                Sum = 700.00m,
                CategoryId = GetCategoryId("Интернет"),
                Comment = "Оплата домашнего интернета",
                Date = new(2023, 11, 15),
                PlaceId = null,
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 28,
                Sum = 4500.00m,
                CategoryId = GetCategoryId("Подарки"),
                Comment = "Подарок ко дню рождения друга",
                Date = new(2023, 11, 25),
                PlaceId = GetPlaceId("Торговый центр"),
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 29,
                Sum = 170000.00m,
                CategoryId = GetCategoryId("Зарплата"),
                Comment = "Зарплата с годовой премией",
                Date = new(2023, 11, 30),
                PlaceId = GetPlaceId("Работа"),
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 30,
                Sum = 30000.00m,
                CategoryId = GetCategoryId("Аренда недвижимости"),
                Comment = "Аренда квартиры за декабрь",
                Date = new(2023, 12, 01),
                PlaceId = GetPlaceId("Квартира"),
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 31,
                Sum = 15000.00m,
                CategoryId = GetCategoryId("Подарки"),
                Comment = "Новогодние подарки семье",
                Date = new(2023, 12, 25),
                PlaceId = GetPlaceId("Торговый центр"),
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 32,
                Sum = 5500.00m,
                CategoryId = GetCategoryId("Рестораны"),
                Comment = "Новогодний ужин",
                Date = new(2023, 12, 31),
                PlaceId = GetPlaceId("Ресторан"),
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 33,
                Sum = 8000.00m,
                CategoryId = GetCategoryId("Одежда для взрослых"),
                Comment = "Зимняя куртка",
                Date = new(2023, 12, 15),
                PlaceId = GetPlaceId("Магазин одежды"),
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 34,
                Sum = 50000.00m,
                CategoryId = GetCategoryId("Премии"),
                Comment = "Годовая премия",
                Date = new(2023, 12, 31),
                PlaceId = null,
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 35,
                Sum = 30000.00m,
                CategoryId = GetCategoryId("Фриланс"),
                Comment = "Декабрьский проект",
                Date = new(2023, 12, 20),
                PlaceId = null,
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 36,
                Sum = 1500.00m,
                CategoryId = GetCategoryId("Сладости"),
                Comment = "Конфеты и шоколад к празднику",
                Date = new(2023, 12, 29),
                PlaceId = GetPlaceId("Супермаркет"),
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 37,
                Sum = 170000.00m,
                CategoryId = GetCategoryId("Зарплата"),
                Comment = "Зарплата за январь",
                Date = new(2024, 1, 31),
                PlaceId = GetPlaceId("Работа"),
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 38,
                Sum = 30000.00m,
                CategoryId = GetCategoryId("Аренда недвижимости"),
                Comment = "Аренда квартиры за январь",
                Date = new(2024, 1, 01),
                PlaceId = GetPlaceId("Квартира"),
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 39,
                Sum = 2800.00m,
                CategoryId = GetCategoryId("Продукты"),
                Comment = "Закупка после праздников",
                Date = new(2024, 1, 05),
                PlaceId = GetPlaceId("Супермаркет"),
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 40,
                Sum = 3000.00m,
                CategoryId = GetCategoryId("Абонемент"),
                Comment = "Абонемент в спортзал на месяц",
                Date = new(2024, 1, 10),
                PlaceId = GetPlaceId("Спортзал"),
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 41,
                Sum = 850.00m,
                CategoryId = GetCategoryId("Лекарства"),
                Comment = "Препараты от простуды",
                Date = new(2024, 1, 15),
                PlaceId = GetPlaceId("Аптека"),
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 42,
                Sum = 600.00m,
                CategoryId = GetCategoryId("Кино"),
                Comment = "Вечерний сеанс",
                Date = new(2024, 1, 20),
                PlaceId = GetPlaceId("Кинотеатр"),
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 43,
                Sum = 450.00m,
                CategoryId = GetCategoryId("Такси"),
                Comment = "Поездка домой ночью",
                Date = new(2024, 1, 25),
                PlaceId = null,
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 44,
                Sum = 850.00m,
                CategoryId = GetCategoryId("Газ"),
                Comment = "Оплата газа за январь",
                Date = new(2024, 1, 20),
                PlaceId = null,
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 45,
                Sum = 150000.00m,
                CategoryId = GetCategoryId("Зарплата"),
                Comment = "Зарплата за февраль",
                Date = new(2024, 2, 29),
                PlaceId = GetPlaceId("Работа"),
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 46,
                Sum = 30000.00m,
                CategoryId = GetCategoryId("Аренда недвижимости"),
                Comment = "Аренда квартиры за февраль",
                Date = new(2024, 2, 01),
                PlaceId = GetPlaceId("Квартира"),
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 47,
                Sum = 3500.00m,
                CategoryId = GetCategoryId("Подарки"),
                Comment = "Подарок на 14 февраля",
                Date = new(2024, 2, 14),
                PlaceId = GetPlaceId("Торговый центр"),
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 48,
                Sum = 1100.00m,
                CategoryId = GetCategoryId("Кафе"),
                Comment = "Обед с друзьями",
                Date = new(2024, 2, 18),
                PlaceId = GetPlaceId("Кофейня"),
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 49,
                Sum = 1200.00m,
                CategoryId = GetCategoryId("Книги"),
                Comment = "Художественный роман",
                Date = new(2024, 2, 22),
                PlaceId = GetPlaceId("Книжный магазин"),
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 50,
                Sum = 2600.00m,
                CategoryId = GetCategoryId("Продукты"),
                Comment = "Закупка продуктов на неделю",
                Date = new(2024, 2, 05),
                PlaceId = GetPlaceId("Супермаркет"),
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 51,
                Sum = 150000.00m,
                CategoryId = GetCategoryId("Зарплата"),
                Comment = "Зарплата за март",
                Date = new(2024, 3, 31),
                PlaceId = GetPlaceId("Работа"),
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 52,
                Sum = 30000.00m,
                CategoryId = GetCategoryId("Аренда недвижимости"),
                Comment = "Аренда квартиры за март",
                Date = new(2024, 3, 01),
                PlaceId = GetPlaceId("Квартира"),
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 53,
                Sum = 12000.00m,
                CategoryId = GetCategoryId("Онлайн-курсы"),
                Comment = "Курс по архитектуре ПО",
                Date = new(2024, 3, 10),
                PlaceId = null,
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 54,
                Sum = 6000.00m,
                CategoryId = GetCategoryId("Обувь"),
                Comment = "Весенние кроссовки",
                Date = new(2024, 3, 15),
                PlaceId = GetPlaceId("Магазин одежды"),
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 55,
                Sum = 5500.00m,
                CategoryId = GetCategoryId("Стоматология"),
                Comment = "Чистка зубов",
                Date = new(2024, 3, 22),
                PlaceId = GetPlaceId("Стоматологическая клиника"),
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 56,
                Sum = 4200.00m,
                CategoryId = GetCategoryId("Концерты"),
                Comment = "Билет на весенний концерт",
                Date = new(2024, 3, 25),
                PlaceId = GetPlaceId("Концертный зал"),
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 57,
                Sum = 3100.00m,
                CategoryId = GetCategoryId("Продукты"),
                Comment = "Продукты на неделю",
                Date = new(2024, 3, 18),
                PlaceId = GetPlaceId("Супермаркет"),
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 58,
                Sum = 150000.00m,
                CategoryId = GetCategoryId("Зарплата"),
                Comment = "Зарплата за апрель",
                Date = new(2024, 4, 30),
                PlaceId = GetPlaceId("Работа"),
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 59,
                Sum = 30000.00m,
                CategoryId = GetCategoryId("Аренда недвижимости"),
                Comment = "Аренда квартиры за апрель",
                Date = new(2024, 4, 01),
                PlaceId = GetPlaceId("Квартира"),
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 60,
                Sum = 15000.00m,
                CategoryId = GetCategoryId("Билеты"),
                Comment = "Авиабилеты на майские",
                Date = new(2024, 4, 10),
                PlaceId = GetPlaceId("Онлайн-магазин"),
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 61,
                Sum = 20000.00m,
                CategoryId = GetCategoryId("Отели"),
                Comment = "Бронирование отеля",
                Date = new(2024, 4, 15),
                PlaceId = GetPlaceId("Онлайн-магазин"),
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 62,
                Sum = 4500.00m,
                CategoryId = GetCategoryId("Рестораны"),
                Comment = "Ужин с коллегами",
                Date = new(2024, 4, 18),
                PlaceId = GetPlaceId("Ресторан"),
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 63,
                Sum = 2900.00m,
                CategoryId = GetCategoryId("Продукты"),
                Comment = "Недельная закупка",
                Date = new(2024, 4, 25),
                PlaceId = GetPlaceId("Супермаркет"),
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 64,
                Sum = 160000.00m,
                CategoryId = GetCategoryId("Зарплата"),
                Comment = "Зарплата за май с индексацией",
                Date = new(2024, 5, 31),
                PlaceId = GetPlaceId("Работа"),
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 65,
                Sum = 30000.00m,
                CategoryId = GetCategoryId("Аренда недвижимости"),
                Comment = "Аренда квартиры за май",
                Date = new(2024, 5, 01),
                PlaceId = GetPlaceId("Квартира"),
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 66,
                Sum = 5000.00m,
                CategoryId = GetCategoryId("Мясо и рыба"),
                Comment = "Мясо для шашлыков",
                Date = new(2024, 5, 09),
                PlaceId = GetPlaceId("Магазин продуктов"),
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 67,
                Sum = 1500.00m,
                CategoryId = GetCategoryId("Транспорт"),
                Comment = "Поездка на дачу",
                Date = new(2024, 5, 10),
                PlaceId = null,
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 68,
                Sum = 700.00m,
                CategoryId = GetCategoryId("Кино"),
                Comment = "Премьера",
                Date = new(2024, 5, 15),
                PlaceId = GetPlaceId("Кинотеатр"),
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 69,
                Sum = 20000.00m,
                CategoryId = GetCategoryId("Бонусы"),
                Comment = "Бонус за квартальные показатели",
                Date = new(2024, 5, 20),
                PlaceId = null,
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 70,
                Sum = 160000.00m,
                CategoryId = GetCategoryId("Зарплата"),
                Comment = "Зарплата за июнь",
                Date = new(2024, 6, 30),
                PlaceId = GetPlaceId("Работа"),
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 71,
                Sum = 30000.00m,
                CategoryId = GetCategoryId("Аренда недвижимости"),
                Comment = "Аренда квартиры за июнь",
                Date = new(2024, 6, 01),
                PlaceId = GetPlaceId("Квартира"),
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 72,
                Sum = 8000.00m,
                CategoryId = GetCategoryId("Инвентарь"),
                Comment = "Велосипедные аксессуары",
                Date = new(2024, 6, 10),
                PlaceId = GetPlaceId("Онлайн-магазин"),
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 73,
                Sum = 35000.00m,
                CategoryId = GetCategoryId("Фриланс"),
                Comment = "Проект для стороннего заказчика",
                Date = new(2024, 6, 15),
                PlaceId = null,
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 74,
                Sum = 3000.00m,
                CategoryId = GetCategoryId("Продукты"),
                Comment = "Закупка продуктов",
                Date = new(2024, 6, 20),
                PlaceId = GetPlaceId("Супермаркет"),
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 75,
                Sum = 1800.00m,
                CategoryId = GetCategoryId("Напитки"),
                Comment = "Напитки на жару",
                Date = new(2024, 6, 22),
                PlaceId = GetPlaceId("Магазин продуктов"),
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 76,
                Sum = 160000.00m,
                CategoryId = GetCategoryId("Зарплата"),
                Comment = "Зарплата за июль",
                Date = new(2024, 7, 31),
                PlaceId = GetPlaceId("Работа"),
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 77,
                Sum = 30000.00m,
                CategoryId = GetCategoryId("Аренда недвижимости"),
                Comment = "Аренда квартиры за июль",
                Date = new(2024, 7, 01),
                PlaceId = GetPlaceId("Квартира"),
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 78,
                Sum = 25000.00m,
                CategoryId = GetCategoryId("Билеты"),
                Comment = "Билеты в отпуск",
                Date = new(2024, 7, 05),
                PlaceId = GetPlaceId("Онлайн-магазин"),
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 79,
                Sum = 40000.00m,
                CategoryId = GetCategoryId("Отели"),
                Comment = "Отель на море",
                Date = new(2024, 7, 10),
                PlaceId = GetPlaceId("Онлайн-магазин"),
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 80,
                Sum = 8000.00m,
                CategoryId = GetCategoryId("Рестораны"),
                Comment = "Ужины в отпуске",
                Date = new(2024, 7, 12),
                PlaceId = GetPlaceId("Ресторан"),
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 81,
                Sum = 2200.00m,
                CategoryId = GetCategoryId("Доставка еды"),
                Comment = "Заказ пиццы на дом",
                Date = new(2024, 7, 20),
                PlaceId = null,
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 82,
                Sum = 160000.00m,
                CategoryId = GetCategoryId("Зарплата"),
                Comment = "Зарплата за август",
                Date = new(2024, 8, 31),
                PlaceId = GetPlaceId("Работа"),
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 83,
                Sum = 30000.00m,
                CategoryId = GetCategoryId("Аренда недвижимости"),
                Comment = "Аренда квартиры за август",
                Date = new(2024, 8, 01),
                PlaceId = GetPlaceId("Квартира"),
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 84,
                Sum = 2800.00m,
                CategoryId = GetCategoryId("Продукты"),
                Comment = "Еженедельная закупка",
                Date = new(2024, 8, 10),
                PlaceId = GetPlaceId("Супермаркет"),
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 85,
                Sum = 45000.00m,
                CategoryId = GetCategoryId("Техника и гаджеты"),
                Comment = "Новый монитор",
                Date = new(2024, 8, 15),
                PlaceId = GetPlaceId("Онлайн-магазин"),
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 86,
                Sum = 5000.00m,
                CategoryId = GetCategoryId("Денежный подарок"),
                Comment = "Подарок от родителей на день рождения",
                Date = new(2024, 8, 20),
                PlaceId = null,
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 87,
                Sum = 1500.00m,
                CategoryId = GetCategoryId("Парковка"),
                Comment = "Парковка в центре",
                Date = new(2024, 8, 22),
                PlaceId = null,
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 88,
                Sum = 160000.00m,
                CategoryId = GetCategoryId("Зарплата"),
                Comment = "Зарплата за сентябрь",
                Date = new(2024, 9, 30),
                PlaceId = GetPlaceId("Работа"),
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 89,
                Sum = 30000.00m,
                CategoryId = GetCategoryId("Аренда недвижимости"),
                Comment = "Аренда квартиры за сентябрь",
                Date = new(2024, 9, 01),
                PlaceId = GetPlaceId("Квартира"),
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 90,
                Sum = 7000.00m,
                CategoryId = GetCategoryId("Одежда для детей"),
                Comment = "Школьная форма",
                Date = new(2024, 9, 01),
                PlaceId = GetPlaceId("Магазин одежды"),
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 91,
                Sum = 3000.00m,
                CategoryId = GetCategoryId("Продукты"),
                Comment = "Продукты на неделю",
                Date = new(2024, 9, 05),
                PlaceId = GetPlaceId("Супермаркет"),
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 92,
                Sum = 1200.00m,
                CategoryId = GetCategoryId("Лекарства"),
                Comment = "Витамины на осень",
                Date = new(2024, 9, 10),
                PlaceId = GetPlaceId("Аптека"),
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 93,
                Sum = 950.00m,
                CategoryId = GetCategoryId("Хлеб и выпечка"),
                Comment = "Свежая выпечка",
                Date = new(2024, 9, 15),
                PlaceId = GetPlaceId("Пекарня"),
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 94,
                Sum = 10000.00m,
                CategoryId = GetCategoryId("Возврат долга"),
                Comment = "Друг вернул долг",
                Date = new(2024, 9, 20),
                PlaceId = null,
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 95,
                Sum = 170000.00m,
                CategoryId = GetCategoryId("Зарплата"),
                Comment = "Зарплата за октябрь с индексацией",
                Date = new(2024, 10, 31),
                PlaceId = GetPlaceId("Работа"),
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 96,
                Sum = 30000.00m,
                CategoryId = GetCategoryId("Аренда недвижимости"),
                Comment = "Аренда квартиры за октябрь",
                Date = new(2024, 10, 01),
                PlaceId = GetPlaceId("Квартира"),
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 97,
                Sum = 1400.00m,
                CategoryId = GetCategoryId("Вода"),
                Comment = "Оплата водоснабжения",
                Date = new(2024, 10, 10),
                PlaceId = null,
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 98,
                Sum = 3200.00m,
                CategoryId = GetCategoryId("Продукты"),
                Comment = "Еженедельная закупка",
                Date = new(2024, 10, 15),
                PlaceId = GetPlaceId("Супермаркет"),
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 99,
                Sum = 6500.00m,
                CategoryId = GetCategoryId("Концерты"),
                Comment = "Билет на осенний концерт",
                Date = new(2024, 10, 20),
                PlaceId = GetPlaceId("Концертный зал"),
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 100,
                Sum = 40000.00m,
                CategoryId = GetCategoryId("Фриланс"),
                Comment = "Большой проект",
                Date = new(2024, 10, 25),
                PlaceId = null,
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 101,
                Sum = 2500.00m,
                CategoryId = GetCategoryId("Игры"),
                Comment = "Новая компьютерная игра",
                Date = new(2024, 10, 28),
                PlaceId = null,
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 102,
                Sum = 170000.00m,
                CategoryId = GetCategoryId("Зарплата"),
                Comment = "Зарплата за ноябрь",
                Date = new(2024, 11, 30),
                PlaceId = GetPlaceId("Работа"),
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 103,
                Sum = 30000.00m,
                CategoryId = GetCategoryId("Аренда недвижимости"),
                Comment = "Аренда квартиры за ноябрь",
                Date = new(2024, 11, 01),
                PlaceId = GetPlaceId("Квартира"),
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 104,
                Sum = 55000.00m,
                CategoryId = GetCategoryId("Техника и гаджеты"),
                Comment = "Покупка в Чёрную пятницу",
                Date = new(2024, 11, 29),
                PlaceId = GetPlaceId("Онлайн-магазин"),
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 105,
                Sum = 3100.00m,
                CategoryId = GetCategoryId("Продукты"),
                Comment = "Еженедельная закупка",
                Date = new(2024, 11, 15),
                PlaceId = GetPlaceId("Супермаркет"),
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 106,
                Sum = 3500.00m,
                CategoryId = GetCategoryId("Абонемент"),
                Comment = "Продление абонемента",
                Date = new(2024, 11, 10),
                PlaceId = GetPlaceId("Спортзал"),
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 107,
                Sum = 1700.00m,
                CategoryId = GetCategoryId("Доставка еды"),
                Comment = "Доставка ужина",
                Date = new(2024, 11, 18),
                PlaceId = null,
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 108,
                Sum = 200000.00m,
                CategoryId = GetCategoryId("Зарплата"),
                Comment = "Зарплата с тринадцатой",
                Date = new(2024, 12, 31),
                PlaceId = GetPlaceId("Работа"),
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 109,
                Sum = 30000.00m,
                CategoryId = GetCategoryId("Аренда недвижимости"),
                Comment = "Аренда квартиры за декабрь",
                Date = new(2024, 12, 01),
                PlaceId = GetPlaceId("Квартира"),
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 110,
                Sum = 25000.00m,
                CategoryId = GetCategoryId("Подарки"),
                Comment = "Новогодние подарки",
                Date = new(2024, 12, 28),
                PlaceId = GetPlaceId("Торговый центр"),
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 111,
                Sum = 10000.00m,
                CategoryId = GetCategoryId("Рестораны"),
                Comment = "Корпоратив",
                Date = new(2024, 12, 30),
                PlaceId = GetPlaceId("Ресторан"),
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 112,
                Sum = 80000.00m,
                CategoryId = GetCategoryId("Премии"),
                Comment = "Годовая премия",
                Date = new(2024, 12, 31),
                PlaceId = null,
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 113,
                Sum = 100000.00m,
                CategoryId = GetCategoryId("Инвестиции"),
                Comment = "Пополнение брокерского счёта",
                Date = new(2024, 12, 15),
                PlaceId = null,
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 114,
                Sum = 4500.00m,
                CategoryId = GetCategoryId("Продукты"),
                Comment = "Закупка к новогоднему столу",
                Date = new(2024, 12, 20),
                PlaceId = GetPlaceId("Супермаркет"),
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 115,
                Sum = 12000.00m,
                CategoryId = GetCategoryId("Одежда для взрослых"),
                Comment = "Праздничный наряд",
                Date = new(2024, 12, 10),
                PlaceId = GetPlaceId("Магазин одежды"),
            },
        ];

        return (operations, places);

        int GetCategoryId(string name)
        {
            return categoryDictionary[name];
        }

        int? GetPlaceId(string name)
        {
            return placeDictionary.TryGetValue(name, out var id) ? id : null;
        }
    }

    private static List<Place> SeedPlaces(int userId, int startIndex = 0)
    {
        var places = new List<Place>
        {
            new()
            {
                UserId = userId,
                Id = startIndex + 1,
                Name = "Работа",
                LastUsedDate = DateTime.Now,
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 2,
                Name = "Супермаркет",
                LastUsedDate = DateTime.Now,
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 3,
                Name = "Концертный зал",
                LastUsedDate = DateTime.Now,
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 4,
                Name = "Квартира",
                LastUsedDate = DateTime.Now,
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 5,
                Name = "Поликлиника",
                LastUsedDate = DateTime.Now,
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 6,
                Name = "Магазин одежды",
                LastUsedDate = DateTime.Now,
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 7,
                Name = "Кинотеатр",
                LastUsedDate = DateTime.Now,
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 8,
                Name = "Учебный центр",
                LastUsedDate = DateTime.Now,
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 9,
                Name = "Магазин продуктов",
                LastUsedDate = DateTime.Now,
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 10,
                Name = "Аптека",
                LastUsedDate = DateTime.Now,
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 11,
                Name = "Кофейня",
                LastUsedDate = DateTime.Now,
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 12,
                Name = "Ресторан",
                LastUsedDate = DateTime.Now,
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 13,
                Name = "АЗС",
                LastUsedDate = DateTime.Now,
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 14,
                Name = "Спортзал",
                LastUsedDate = DateTime.Now,
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 15,
                Name = "Онлайн-магазин",
                LastUsedDate = DateTime.Now,
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 16,
                Name = "Торговый центр",
                LastUsedDate = DateTime.Now,
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 17,
                Name = "Книжный магазин",
                LastUsedDate = DateTime.Now,
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 18,
                Name = "Стоматологическая клиника",
                LastUsedDate = DateTime.Now,
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 19,
                Name = "Пекарня",
                LastUsedDate = DateTime.Now,
            },
        };

        return places;
    }

    private static int SetCategoryIds(List<Category> categories, ref int currentIndex)
    {
        foreach (var category in categories)
        {
            category.Id = currentIndex++;

            if (category.SubCategories is { Count: > 0 })
            {
                SetCategoryIds(category.SubCategories, ref currentIndex);
            }
        }

        return currentIndex;
    }

    private static List<Category> GetAllCategories(List<Category> categories)
    {
        var allCategories = new List<Category>();
        GetAllCategoriesRecursive(categories, allCategories);
        return allCategories;
    }

    private static void GetAllCategoriesRecursive(List<Category> categories, List<Category> allCategories)
    {
        foreach (var category in categories)
        {
            allCategories.Add(category);

            if (category.SubCategories is { Count: > 0 })
            {
                GetAllCategoriesRecursive(category.SubCategories, allCategories);
            }
        }
    }
}
