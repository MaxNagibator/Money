using System.ComponentModel;
using Extentions;

namespace ServiceNames
{
    [Description("Машины")]
    [ServiceName("Car")]
    public enum CarService
    {
        [Description("Получить машины")]
        [ServiceName("Car/GetCars")]
        GetCars,

        [Description("Создать машину")]
        [ServiceName("Car/CreateCar")]
        CreateCar,

        [Description("Обновить машину")]
        [ServiceName("Car/UpdateCar")]
        UpdateCar,

        [Description("Удалить машину")]
        [ServiceName("Car/DeleteCar")]
        DeleteCar,

        [Description("Получить события машины")]
        [ServiceName("Car/GetCarEvents")]
        GetCarEvents,

        [Description("Создать машине событие")]
        [ServiceName("Car/CreateCarEvent")]
        CreateCarEvent,

        [Description("Обновить машине событие")]
        [ServiceName("Car/UpdateCarEvent")]
        UpdateCarEvent,

        [Description("Удалить машине событие")]
        [ServiceName("Car/DeleteCarEvent")]
        DeleteCarEvent,
    }
}