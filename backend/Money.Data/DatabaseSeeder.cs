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
                ],
            },

            new()
            {
                UserId = userId,
                Name = "Здоровье",
                Description = "Расходы на медицинские услуги и лекарства",
                TypeId = 1,
                Color = "#CC33FF",
            },

            new()
            {
                UserId = userId,
                Name = "Образование",
                Description = "Расходы на обучение и курсы",
                TypeId = 1,
                Color = "#FFCCFF",
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
        ];

        return (operations, places);

        int GetCategoryId(string name) => categoryDictionary[name];
        int? GetPlaceId(string name) => placeDictionary.TryGetValue(name, out var id) ? id : null;
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
