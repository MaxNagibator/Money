namespace Money.Business;

// TODO: Переместить в другой проект
public static class DatabaseSeeder
{
    public static List<DomainCategory> SeedCategories(int userId, int startIndex = 1)
    {
        List<DomainCategory> categories =
        [
            new()
            {
                Id = startIndex + 1,
                UserId = userId,
                Name = "Продукты",
                Description = "Расходы на продукты питания",
                TypeId = 1,
                Color = "#FFCC00",
            },

            new()
            {
                Id = startIndex + 2,
                UserId = userId,
                Name = "Транспорт",
                Description = "Расходы на транспорт",
                TypeId = 1,
                Color = "#0099CC",
            },

            new()
            {
                Id = startIndex + 3,
                UserId = userId,
                Name = "Развлечения",
                Description = "Расходы на развлечения и досуг",
                TypeId = 1,
                Color = "#FF3366",
            },

            new()
            {
                Id = startIndex + 4,
                UserId = userId,
                Name = "Коммунальные услуги",
                Description = "Расходы на коммунальные услуги",
                TypeId = 1,
                Color = "#66CC66",
            },

            new()
            {
                Id = startIndex + 5,
                UserId = userId,
                Name = "Одежда",
                Description = "Расходы на одежду и обувь",
                TypeId = 1,
                Color = "#FF6600",
            },

            new()
            {
                Id = startIndex + 6,
                UserId = userId,
                Name = "Здоровье",
                Description = "Расходы на медицинские услуги и лекарства",
                TypeId = 1,
                Color = "#CC33FF",
            },

            new()
            {
                Id = startIndex + 7,
                UserId = userId,
                Name = "Образование",
                Description = "Расходы на обучение и курсы",
                TypeId = 1,
                Color = "#FFCCFF",
            },

            new()
            {
                Id = startIndex + 8,
                UserId = userId,
                Name = "Зарплата",
                Description = "Основной источник дохода",
                TypeId = 2,
                Color = "#66B3FF",
            },

            new()
            {
                Id = startIndex + 9,
                UserId = userId,
                Name = "Дополнительный доход",
                Description = "Доход от фриланса и подработок",
                TypeId = 2,
                Color = "#FFB366",
            },

            new()
            {
                Id = startIndex + 10,
                UserId = userId,
                Name = "Инвестиции",
                Description = "Доход от инвестиций",
                TypeId = 2,
                Color = "#66FF66",
            },

            new()
            {
                Id = startIndex + 11,
                UserId = userId,
                Name = "Пассивный доход",
                Description = "Доход от аренды и других источников",
                TypeId = 2,
                Color = "#FF6666",
            },

            new()
            {
                Id = startIndex + 12,
                UserId = userId,
                Name = "Премии",
                Description = "Дополнительные выплаты и бонусы",
                TypeId = 2,
                Color = "#FFCC00",
            },

            new()
            {
                Id = startIndex + 13,
                UserId = userId,
                Name = "Фрукты и овощи",
                Description = "Расходы на свежие фрукты и овощи",
                TypeId = 1,
                ParentId = startIndex + 1,
                Color = "#FF9900",
            },

            new()
            {
                Id = startIndex + 14,
                UserId = userId,
                Name = "Мясо и рыба",
                Description = "Расходы на мясные и рыбные продукты",
                TypeId = 1,
                ParentId = startIndex + 1,
                Color = "#CC3300",
            },

            new()
            {
                Id = startIndex + 15,
                UserId = userId,
                Name = "Общественный транспорт",
                Description = "Расходы на проезд в общественном транспорте",
                TypeId = 1,
                ParentId = startIndex + 2,
                Color = "#007ACC",
            },

            new()
            {
                Id = startIndex + 16,
                UserId = userId,
                Name = "Такси",
                Description = "Расходы на такси",
                TypeId = 1,
                ParentId = startIndex + 2,
                Color = "#005B99",
            },

            new()
            {
                Id = startIndex + 17,
                UserId = userId,
                Name = "Кино",
                Description = "Расходы на посещение кинотеатров",
                TypeId = 1,
                ParentId = startIndex + 3,
                Color = "#FF3366",
            },

            new()
            {
                Id = startIndex + 18,
                UserId = userId,
                Name = "Концерты",
                Description = "Расходы на посещение концертов",
                TypeId = 1,
                ParentId = startIndex + 3,
                Color = "#FF6699",
            },

            new()
            {
                Id = startIndex + 19,
                UserId = userId,
                Name = "Электричество",
                Description = "Расходы на электроэнергию",
                TypeId = 1,
                ParentId = startIndex + 4,
                Color = "#FFCC00",
            },

            new()
            {
                Id = startIndex + 20,
                UserId = userId,
                Name = "Вода",
                Description = "Расходы на водоснабжение",
                TypeId = 1,
                ParentId = startIndex + 4,
                Color = "#66CCFF",
            },

            new()
            {
                Id = startIndex + 21,
                UserId = userId,
                Name = "Одежда для детей",
                Description = "Расходы на детскую одежду",
                TypeId = 1,
                ParentId = startIndex + 5,
                Color = "#FF9966",
            },

            new()
            {
                Id = startIndex + 22,
                UserId = userId,
                Name = "Одежда для взрослых",
                Description = "Расходы на одежду для взрослых",
                TypeId = 1,
                ParentId = startIndex + 5,
                Color = "#CC6600",
            },

            new()
            {
                Id = startIndex + 23,
                UserId = userId,
                Name = "Бонусы",
                Description = "Бонусы от работодателя",
                TypeId = 2,
                ParentId = startIndex + 8,
                Color = "#66B3FF",
            },

            new()
            {
                Id = startIndex + 24,
                UserId = userId,
                Name = "Фриланс",
                Description = "Доход от фриланс-проектов",
                TypeId = 2,
                ParentId = startIndex + 9,
                Color = "#FFB366",
            },

            new()
            {
                Id = startIndex + 25,
                UserId = userId,
                Name = "Аренда недвижимости",
                Description = "Доход от аренды квартир или домов",
                TypeId = 2,
                ParentId = startIndex + 11,
                Color = "#66FF66",
            },

            new()
            {
                Id = startIndex + 26,
                UserId = userId,
                Name = "Дивиденды",
                Description = "Доход от акций и инвестиций",
                TypeId = 2,
                ParentId = startIndex + 10,
                Color = "#FF6666",
            },

            new()
            {
                Id = startIndex + 27,
                UserId = userId,
                Name = "Премии за достижения",
                Description = "Премии за выполнение планов и целей",
                TypeId = 2,
                ParentId = startIndex + 12,
                Color = "#FFCC00",
            },

            new()
            {
                Id = startIndex + 28,
                UserId = userId,
                Name = "Курсы и тренинги",
                Description = "Доход от проведения курсов и тренингов",
                TypeId = 2,
                ParentId = startIndex + 9,
                Color = "#FFB3FF",
            },
        ];

        return categories;
    }

    public static (List<DomainPayment> payments, List<DomainPlace> places) SeedPayments(int userId, int startIndex = 1, int placeStartIndex = 1, int categoryStartIndex = 1)
    {
        List<DomainPayment> payments =
        [
            new()
            {
                UserId = userId,
                Id = startIndex + 1,
                Sum = 150000.00m,
                CategoryId = categoryStartIndex + 8,
                Comment = "Зарплата за сентябрь",
                Date = new DateTime(2023, 9, 30),
                PlaceId = placeStartIndex + 1,
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 2,
                Sum = 2000.00m,
                CategoryId = categoryStartIndex + 1,
                Comment = "Покупка продуктов в супермаркете",
                Date = new DateTime(2023, 10, 01),
                PlaceId = placeStartIndex + 2,
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 3,
                Sum = 5000.00m,
                CategoryId = categoryStartIndex + 18,
                Comment = "Билет на концерт",
                Date = new DateTime(2023, 10, 05),
                PlaceId = placeStartIndex + 3,
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 4,
                Sum = 30000.00m,
                CategoryId = categoryStartIndex + 25,
                Comment = "Аренда квартиры за октябрь",
                Date = new DateTime(2023, 10, 01),
                PlaceId = placeStartIndex + 4,
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 5,
                Sum = 10000.00m,
                CategoryId = categoryStartIndex + 6,
                Comment = "Посещение врача",
                Date = new DateTime(2023, 10, 10),
                PlaceId = null,
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 6,
                Sum = 705.00m,
                CategoryId = categoryStartIndex + 3,
                Comment = "Подписка на стриминговый сервис",
                Date = new DateTime(2023, 10, 15),
                PlaceId = null,
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 7,
                Sum = 500.00m,
                CategoryId = categoryStartIndex + 11,
                Comment = "Дивиденды от акций",
                Date = new DateTime(2023, 10, 20),
                PlaceId = null,
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 8,
                Sum = 1200.00m,
                CategoryId = categoryStartIndex + 4,
                Comment = "Оплата за электричество",
                Date = new DateTime(2023, 10, 25),
                PlaceId = null,
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 9,
                Sum = 25000.00m,
                CategoryId = categoryStartIndex + 7,
                Comment = "Оплата курсов по программированию",
                Date = new DateTime(2023, 10, 30),
                PlaceId = null,
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 10,
                Sum = 80.00m,
                CategoryId = categoryStartIndex + 2,
                Comment = "Оплата проезда на автобусе",
                Date = new DateTime(2023, 10, 28),
                PlaceId = null,
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 11,
                Sum = 3000.00m,
                CategoryId = categoryStartIndex + 19,
                Comment = "Оплата за электричество за сентябрь",
                Date = new DateTime(2023, 9, 28),
                PlaceId = null,
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 12,
                Sum = 15000.00m,
                CategoryId = categoryStartIndex + 5,
                Comment = "Покупка новой одежды",
                Date = new DateTime(2023, 10, 12),
                PlaceId = placeStartIndex + 6,
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 13,
                Sum = 4000.00m,
                CategoryId = categoryStartIndex + 26,
                Comment = "Дивиденды от инвестиций в акции",
                Date = new DateTime(2023, 10, 22),
                PlaceId = null,
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 14,
                Sum = 900.00m,
                CategoryId = categoryStartIndex + 3,
                Comment = "Посещение кинотеатра",
                Date = new DateTime(2023, 10, 18),
                PlaceId = placeStartIndex + 7,
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 15,
                Sum = 25000.00m,
                CategoryId = categoryStartIndex + 24,
                Comment = "Оплата за фриланс-проект",
                Date = new DateTime(2023, 10, 15),
                PlaceId = null,
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 16,
                Sum = 60.00m,
                CategoryId = categoryStartIndex + 15,
                Comment = "Оплата проезда на метро",
                Date = new DateTime(2023, 10, 29),
                PlaceId = null,
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 17,
                Sum = 50000.00m,
                CategoryId = categoryStartIndex + 10,
                Comment = "Инвестиции в стартап",
                Date = new DateTime(2023, 10, 05),
                PlaceId = null,
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 18,
                Sum = 35000.00m,
                CategoryId = categoryStartIndex + 27,
                Comment = "Премия за успешный проект",
                Date = new DateTime(2023, 10, 30),
                PlaceId = null,
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 19,
                Sum = 12000.00m,
                CategoryId = categoryStartIndex + 12,
                Comment = "Премия за выполнение плана",
                Date = new DateTime(2023, 10, 31),
                PlaceId = null,
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 20,
                Sum = 800.00m,
                CategoryId = categoryStartIndex + 14,
                Comment = "Покупка мяса на ужин",
                Date = new DateTime(2023, 10, 20),
                PlaceId = placeStartIndex + 8,
            },
        ];

        return (payments, SeedPlaces(userId, placeStartIndex));
    }

    private static List<DomainPlace> SeedPlaces(int userId, int startIndex = 1)
    {
        List<DomainPlace> places =
        [
            new()
            {
                UserId = userId,
                Id = startIndex + 1,
                Name = "Офис",
                Description = "Место работы",
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 2,
                Name = "Супермаркет",
                Description = "Магазин продуктов",
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 3,
                Name = "Концертный зал",
                Description = "Место проведения концертов",
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 4,
                Name = "Квартира",
                Description = "Арендуемая квартира",
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 5,
                Name = "Спортзал",
                Description = "Место для занятий спортом",
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 6,
                Name = "Кафе",
                Description = "Место для отдыха и питания",
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 7,
                Name = "Библиотека",
                Description = "Место для чтения и учебы",
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 8,
                Name = "Магазин электроники",
                Description = "Магазин для покупки техники",
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 9,
                Name = "Ресторан",
                Description = "Место для ужинов и встреч",
            },

            new()
            {
                UserId = userId,
                Id = startIndex + 10,
                Name = "Парикмахерская",
                Description = "Место для стрижки и ухода за волосами",
            },
        ];

        return places;
    }
}
