using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Enums;
using Common.Exceptions;
using DataWorker.Classes;

namespace DataWorker
{
    public class DbCarWorker
    {
        public static List<Car> GetCars(int userId, List<int> carIds = null, bool isWithEvent = false)
        {
            using (var context = new Data.DataContext())
            {
                var dbCars = context.Cars.Where(x => x.UserId == userId);
                if (carIds != null)
                {
                    dbCars = dbCars.Where(x => carIds.Contains(x.CarId));
                }
                List<Data.CarEvent> dbEventList = null;
                if (isWithEvent)
                {
                    var selCarIds = dbCars.Select(x => x.CarId);
                    dbEventList = context.CarEvents.Where(x => x.UserId == userId && selCarIds.Contains(x.CarId)).ToList();
                }
                var dbCarList = dbCars.OrderBy(x => x.Id).ToList();
                var cars = new List<Car>();
                foreach (var dbCar in dbCarList)
                {
                    var car = new Car();
                    car.Id = dbCar.CarId;
                    car.Name = dbCar.Name;
                    if (dbEventList != null)
                    {
                        var dbCarEvents = dbEventList.Where(x => x.CarId == dbCar.CarId).OrderByDescending(x => x.Date).ToList();
                        var carEventList = new List<CarEvent>();
                        foreach (var dbCarEvent in dbCarEvents)
                        {
                            var carEvent = new CarEvent();
                            carEvent.Id = dbCarEvent.EventId;
                            carEvent.Type = (CarEventTypes)dbCarEvent.Type;
                            carEvent.Title = dbCarEvent.Title;
                            carEvent.Date = dbCarEvent.Date;
                            carEvent.Mileage = dbCarEvent.Mileage;
                            carEvent.Comment = dbCarEvent.Comment;
                            carEventList.Add(carEvent);
                        }
                        car.Events = carEventList;
                    }
                    cars.Add(car);
                }
                return cars;
            }
        }

        public static int CreateCar(int userId, Car car)
        {
            using (var context = new Data.DataContext())
            {
                //todo need optimization in future
                var carId = context.Cars.Where(x => x.UserId == userId).Select(x => x.CarId).DefaultIfEmpty(0).Max() + 1;

                var dbCar = new Data.Car();
                dbCar.CarId = carId;
                dbCar.UserId = userId;
                dbCar.Name = car.Name;
                context.Cars.Add(dbCar);
                context.SaveChanges();
                return carId;
            }
        }

        public static void UpdateCar(int userId, Car car)
        {
            using (var context = new Data.DataContext())
            {
                var dbCategory = context.Cars.SingleOrDefault(x => x.CarId == car.Id && x.UserId == userId);
                if (dbCategory == null)
                {
                    throw new MessageException("car not found");
                }
                dbCategory.Name = car.Name;
                context.SaveChanges();
            }
        }

        public static void DeleteCar(int userId, int carId)
        {
            using (var context = new Data.DataContext())
            {
                var dbCar = context.Cars.SingleOrDefault(x => x.CarId == carId && x.UserId == userId);
                if (dbCar == null)
                {
                    throw new MessageException("car not found");
                }
                var hasEvents = context.CarEvents.Any(x => x.CarId == carId && x.UserId == userId);
                if (hasEvents)
                {
                    throw new MessageException("has events");
                }
                context.Cars.Remove(dbCar);
                context.SaveChanges();
            }
        }

        public static int CreateCarEvent(int userId, int carId, CarEvent carEvent)
        {
            using (var context = new Data.DataContext())
            {
                var dbCar = context.Cars.SingleOrDefault(x => x.CarId == carId && x.UserId == userId);
                if (dbCar == null)
                {
                    throw new MessageException("car not found");
                }

                //todo need optimization in future
                var carEventId = context.CarEvents.Where(x => x.UserId == userId).Select(x => x.EventId).DefaultIfEmpty(0).Max() + 1;

                var dbCarEvent = new Data.CarEvent();
                dbCarEvent.CarId = carId;
                dbCarEvent.UserId = userId;
                dbCarEvent.EventId = carEventId;

                dbCarEvent.Title = carEvent.Title;
                dbCarEvent.Type = (int)carEvent.Type;
                dbCarEvent.Date = carEvent.Date;
                dbCarEvent.Mileage = carEvent.Mileage;
                dbCarEvent.Comment = carEvent.Comment;

                context.CarEvents.Add(dbCarEvent);
                context.SaveChanges();
                return carId;
            }
        }

        public static void UpdateCarEvent(int userId, CarEvent carEvent)
        {
            using (var context = new Data.DataContext())
            {
                var dbCarEvent = context.CarEvents.SingleOrDefault(x => x.EventId == carEvent.Id && x.UserId == userId);
                if (dbCarEvent == null)
                {
                    throw new MessageException("carevent not found");
                }
                dbCarEvent.Title = carEvent.Title;
                dbCarEvent.Type = (int)carEvent.Type;
                dbCarEvent.Date = carEvent.Date;
                dbCarEvent.Mileage = carEvent.Mileage;
                dbCarEvent.Comment = carEvent.Comment;
                context.SaveChanges();
            }
        }

        public static void DeleteCarEvent(int userId, int carEventId)
        {
            using (var context = new Data.DataContext())
            {
                var dbCarEvent = context.CarEvents.SingleOrDefault(x => x.EventId == carEventId && x.UserId == userId);
                if (dbCarEvent == null)
                {
                    throw new MessageException("carevent not found");
                }
                context.CarEvents.Remove(dbCarEvent);
                context.SaveChanges();
            }
        }

        public static List<CarEvent> GetCarEvents(int userId, List<int> carEventIds)
        {
            using (var context = new Data.DataContext())
            {
                var dbEventList = context.CarEvents.Where(x => x.UserId == userId && carEventIds.Contains(x.EventId)).ToList();
                var carEventList = new List<CarEvent>();
                foreach (var dbCarEvent in dbEventList)
                {
                    var carEvent = new CarEvent();
                    carEvent.Id = dbCarEvent.EventId;
                    carEvent.Type = (CarEventTypes)dbCarEvent.Type;
                    carEvent.Title = dbCarEvent.Title;
                    carEvent.Date = dbCarEvent.Date;
                    carEvent.Mileage = dbCarEvent.Mileage;
                    carEvent.Comment = dbCarEvent.Comment;
                    carEventList.Add(carEvent);
                }
                return carEventList;
            }
        }
    }
}
