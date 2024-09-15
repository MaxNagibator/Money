using System;
using System.Collections.Generic;
using System.Web;
using DataWorker;
using DataWorker.Classes;
using Extentions;
using ServiceRequest.Cars;
using ServiceRespone.Cars;
using ServiceResponse;
using ServiceWorker.Executor;

namespace ServiceWorker
{
    public class CarWorker
    {
        public static Response<GetCarsResponse> GetCars(GetCarsRequest request, HttpRequestBase httpRequestBase)
        {
            Func<GetCarsResponse> x =
                () =>
                {
                    var userId = request.GetUserIdByToken();
                    var result = new GetCarsResponse();
                    var isWithEvent = request.Fields != null && request.Fields.Contains(GetCarsRequest.FieldValues.Events);
                    var cars = DbCarWorker.GetCars(userId, request.CarIds, isWithEvent);
                    var rcars = new List<GetCarsResponse.CarValue>();
                    foreach (var car in cars)
                    {
                        var rcar = new GetCarsResponse.CarValue();
                        rcar.Id = car.Id;
                        rcar.Name = car.Name;
                        if (isWithEvent)
                        {
                            var rEvents = new List<GetCarsResponse.CarEventValue>();
                            foreach (var carEvent in car.Events)
                            {
                                var rEvent = new GetCarsResponse.CarEventValue();
                                rEvent.Id = carEvent.Id;
                                rEvent.Title = carEvent.Title;
                                rEvent.Type = carEvent.Type;
                                rEvent.Date = carEvent.Date.ToUnixDate();
                                rEvent.Mileage = carEvent.Mileage;
                                rEvent.Comment = carEvent.Comment;
                                rEvents.Add(rEvent);
                            }
                            rcar.Events = rEvents;
                        }
                        rcars.Add(rcar);
                    }
                    result.Cars = rcars;
                    return result;
                };
            return ServiceExecutor.Execute(x, httpRequestBase, request, ServiceNames.CarService.GetCars);
        }
        public static Response<CreateCarResponse> CreateCar(CreateCarRequest request, HttpRequestBase httpRequestBase)
        {
            Func<CreateCarResponse> x =
                () =>
                {
                    var userId = request.GetUserIdByToken();
                    var result = new CreateCarResponse();
                    var car = new Car();
                    car.Name = request.Car.Name;
                    var id = DbCarWorker.CreateCar(userId, car);
                    result.CarId = id;
                    return result;
                };
            return ServiceExecutor.Execute(x, httpRequestBase, request, ServiceNames.CarService.CreateCar);
        }
        public static Response<UpdateCarResponse> UpdateCar(UpdateCarRequest request, HttpRequestBase httpRequestBase)
        {
            Func<UpdateCarResponse> x =
                () =>
                {
                    var userId = request.GetUserIdByToken();
                    var result = new UpdateCarResponse();
                    var car = new Car();
                    car.Id = request.Car.Id;
                    car.Name = request.Car.Name;
                    DbCarWorker.UpdateCar(userId, car);
                    return result;
                };
            return ServiceExecutor.Execute(x, httpRequestBase, request, ServiceNames.CarService.UpdateCar);
        }
        public static Response<DeleteCarResponse> DeleteCar(DeleteCarRequest request, HttpRequestBase httpRequestBase)
        {
            Func<DeleteCarResponse> x =
                () =>
                {
                    var userId = request.GetUserIdByToken();
                    var result = new DeleteCarResponse();
                    var carId = request.CarId;
                    DbCarWorker.DeleteCar(userId, carId);
                    return result;
                };
            return ServiceExecutor.Execute(x, httpRequestBase, request, ServiceNames.CarService.DeleteCar);
        }

        public static Response<GetCarEventsResponse> GetCarEvents(GetCarEventsRequest request, HttpRequestBase httpRequestBase)
        {
            Func<GetCarEventsResponse> x =
                () =>
                {
                    var userId = request.GetUserIdByToken();
                    var result = new GetCarEventsResponse();
                    var carEvents = DbCarWorker.GetCarEvents(userId, request.CarEventIds);

                    var rEvents = new List<GetCarEventsResponse.CarEventValue>();
                    foreach (var carEvent in carEvents)
                    {
                        var rEvent = new GetCarEventsResponse.CarEventValue();
                        rEvent.Id = carEvent.Id;
                        rEvent.Title = carEvent.Title;
                        rEvent.Type = carEvent.Type;
                        rEvent.Date = carEvent.Date.ToUnixDate();
                        rEvent.Mileage = carEvent.Mileage;
                        rEvent.Comment = carEvent.Comment;
                        rEvents.Add(rEvent);
                    }
                    result.Events = rEvents;
                    return result;
                };
            return ServiceExecutor.Execute(x, httpRequestBase,  request, ServiceNames.CarService.GetCarEvents);
        }
        public static Response<CreateCarEventResponse> CreateCarEvent(CreateCarEventRequest request, HttpRequestBase httpRequestBase)
        {
            Func<CreateCarEventResponse> x =
                () =>
                {
                    var userId = request.GetUserIdByToken();
                    var result = new CreateCarEventResponse();

                    var carId = request.CarId;
                    var carEvent = new CarEvent();
                    carEvent.Title = request.CarEvent.Title;
                    carEvent.Date = request.CarEvent.Date.ToDateTime();
                    carEvent.Mileage = request.CarEvent.Mileage;
                    carEvent.Comment = request.CarEvent.Comment;
                    carEvent.Type = request.CarEvent.Type;

                    CheckParams.CheckMoreZero(request.CarEvent.Mileage);
                    var id = DbCarWorker.CreateCarEvent(userId, carId, carEvent);
                    result.CarEventId = id;
                    return result;
                };
            return ServiceExecutor.Execute(x, httpRequestBase,  request, ServiceNames.CarService.CreateCarEvent);
        }
        public static Response<UpdateCarEventResponse> UpdateCarEvent(UpdateCarEventRequest request, HttpRequestBase httpRequestBase)
        {
            Func<UpdateCarEventResponse> x =
                () =>
                {
                    var userId = request.GetUserIdByToken();
                    var result = new UpdateCarEventResponse();
                    var carEvent = new CarEvent();
                    carEvent.Id = request.CarEvent.Id;
                    carEvent.Title = request.CarEvent.Title;
                    carEvent.Date = request.CarEvent.Date.ToDateTime();
                    carEvent.Mileage = request.CarEvent.Mileage;
                    carEvent.Comment = request.CarEvent.Comment;
                    carEvent.Type = request.CarEvent.Type;

                    CheckParams.CheckMoreZero(request.CarEvent.Mileage);
                    DbCarWorker.UpdateCarEvent(userId, carEvent);
                    return result;
                };
            return ServiceExecutor.Execute(x, httpRequestBase,  request, ServiceNames.CarService.UpdateCarEvent);
        }
        public static Response<DeleteCarEventResponse> DeleteCarEvent(DeleteCarEventRequest request, HttpRequestBase httpRequestBase)
        {
            Func<DeleteCarEventResponse> x =
                () =>
                {
                    var userId = request.GetUserIdByToken();
                    var result = new DeleteCarEventResponse();
                    var carEventId = request.CarEventId;
                    DbCarWorker.DeleteCarEvent(userId, carEventId);
                    return result;
                };
            return ServiceExecutor.Execute(x, httpRequestBase,  request, ServiceNames.CarService.DeleteCarEvent);
        }
    }
}