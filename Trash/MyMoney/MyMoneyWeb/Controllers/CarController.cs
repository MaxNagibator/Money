using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using Common;
using Common.Enums;
using Extentions;
using MyMoneyWeb.Helpers;
using MyMoneyWeb.Models;
using MyMoneyWeb.Structure;
using ServiceWorker;
using ServiceRequest.Cars;
using ServiceResponse;

namespace MyMoneyWeb.Controllers
{
    public class CarController : BaseController
    {
        [HttpPost]
        public ActionResult Index()
        {
            return View("Index");
        }

        [HttpGet]
        public ActionResult Get()
        {
            return Json(new { success = true, message = "huy" }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetCars()
        {
            var token = Request.GetAuthToken();
            var getDebtsRequest = new GetCarsRequest();
            getDebtsRequest.Token = token;
            var response = CarWorker.GetCars(getDebtsRequest, Request);
            if (response.Type != ResponseType.Success)
            {
                return PartialView("MyError", new ErrorModel { Message = response.ResponseMessage });
            }
            var model = new CarsModel();
            model.Cars = new List<CarsModel.CarValue>();
            foreach (var car in response.Body.Cars)
            {
                var d = new CarsModel.CarValue();
                d.Id = car.Id;
                d.Name = car.Name;
                model.Cars.Add(d);
            }
            return PartialView("CarList", model);
        }

        [HttpPost]
        public ActionResult CreateCar(string carName)
        {
            var token = Request.GetAuthToken();
            var request = new CreateCarRequest();
            request.Token = token;
            request.Car = new CreateCarRequest.CarValue();
            request.Car.Name = carName;
            var response = CarWorker.CreateCar(request, Request);
            if (response.Type != ResponseType.Success)
            {
                return Json(new { success = false, message = response.ResponseMessage });
            }
            return Json(new { success = true, message = "Oki doki" });
        }

        [HttpPost]
        public ActionResult GetCar(int carId)
        {
            var token = Request.GetAuthToken();
            var request = new GetCarsRequest();
            request.Token = token;
            request.CarIds = new List<int> { carId };
            request.Fields = new List<string> { GetCarsRequest.FieldValues.Events };
            var getCarResponse = CarWorker.GetCars(request, Request);
            if (getCarResponse.Type != ResponseType.Success)
            {
                return Json(new { success = false, message = getCarResponse.ResponseMessage });
            }
            var rcar = getCarResponse.Body.Cars.Single();

            var model = new CarDetailsModel();
            var car = new CarDetailsModel.CarValue();
            car.Id = rcar.Id;
            car.Name = rcar.Name;

            var rEvents = new List<CarDetailsModel.CarEventValue>();
            foreach (var rEvent in rcar.Events)
            {
                var carEvent = new CarDetailsModel.CarEventValue();
                carEvent.Id = rEvent.Id;
                carEvent.Title = rEvent.Title;
                carEvent.Type = rEvent.Type; 
                carEvent.Date = rEvent.Date.ToDateTime();
                carEvent.Mileage = rEvent.Mileage;
                carEvent.Comment = rEvent.Comment;
                rEvents.Add(carEvent);
            }
            car.Events = rEvents;
            model.Car = car;
            return PartialView("CarDetails", model);
        }
        
        [HttpPost]
        public ActionResult GetCarEditForm(int carId)
        {
            var token = Request.GetAuthToken();
            var request = new GetCarsRequest();
            request.Token = token;
            request.CarIds = new List<int> { carId };
            var getCarResponse = CarWorker.GetCars(request, Request);
            if (getCarResponse.Type != ResponseType.Success)
            {
                return Json(new { success = false, message = getCarResponse.ResponseMessage });
            }
            var rcar = getCarResponse.Body.Cars.Single();

            var model = new CarDetailsModel();
            var car = new CarDetailsModel.CarValue();
            car.Id = rcar.Id;
            car.Name = rcar.Name;
            model.Car = car;
            return PartialView("CarEditForm", model);
        }

        [HttpPost]
        public ActionResult UpdateCar(int carId, string carName)
        {
            var token = Request.GetAuthToken();
            var request = new UpdateCarRequest();
            request.Token = token;
            request.Car = new UpdateCarRequest.CarValue();
            request.Car.Id = carId;
            request.Car.Name = carName;
            var response = CarWorker.UpdateCar(request, Request);
            if (response.Type != ResponseType.Success)
            {
                return Json(new { success = false, message = response.ResponseMessage });
            }
            return Json(new { success = true, message = "Oki doki" });
        }

        [HttpPost]
        public ActionResult DeleteCar(int carId)
        {
            var token = Request.GetAuthToken();
            var request = new DeleteCarRequest();
            request.Token = token;
            request.CarId = carId;
            var response = CarWorker.DeleteCar(request, Request);
            if (response.Type != ResponseType.Success)
            {
                return Json(new { success = false, message = response.ResponseMessage });
            }
            return Json(new { success = true, message = "Oki doki" });
        }

        [HttpPost]
        public ActionResult CreateCarEvent(int carId, string title, DateTime date, decimal? mileage, string comment, CarEventTypes type)
        {
            var token = Request.GetAuthToken();
            var request = new CreateCarEventRequest();
            request.Token = token;
            request.CarId = carId;
            request.CarEvent = new CreateCarEventRequest.CarEventValue();
            request.CarEvent.Title = title;
            request.CarEvent.Date = date.ToUnixDate();
            request.CarEvent.Mileage = mileage;
            request.CarEvent.Comment = comment;
            request.CarEvent.Type = type;
            var response = CarWorker.CreateCarEvent(request, Request);
            if (response.Type != ResponseType.Success)
            {
                return Json(new { success = false, message = response.ResponseMessage });
            }
            return Json(new { success = true, message = "Oki doki" });
        }

        [HttpPost]
        public ActionResult GetCarEventEditForm(int carEventId)
        {
            var token = Request.GetAuthToken();
            var request = new GetCarEventsRequest();
            request.Token = token;
            request.CarEventIds = new List<int> { carEventId };
            var getCarResponse = CarWorker.GetCarEvents(request, Request);
            if (getCarResponse.Type != ResponseType.Success)
            {
                return Json(new { success = false, message = getCarResponse.ResponseMessage });
            }
            var carEvent = getCarResponse.Body.Events.Single();

            var mEvent = new CarDetailsModel.CarEventValue();
            mEvent.Id = carEvent.Id;
            mEvent.Title = carEvent.Title;
            mEvent.Type = carEvent.Type;
            mEvent.Date = carEvent.Date.ToDateTime();
            mEvent.Mileage = carEvent.Mileage;
            mEvent.Comment = carEvent.Comment;
            return PartialView("CarEventEditForm", mEvent);
        }

        [HttpPost]
        public ActionResult UpdateCarEvent(int carEventId, string title, DateTime date, decimal? mileage, string comment, CarEventTypes type)
        {
            var token = Request.GetAuthToken();
            var request = new UpdateCarEventRequest();
            request.Token = token;
            request.CarEvent = new UpdateCarEventRequest.CarEventValue();
            request.CarEvent.Id = carEventId;
            request.CarEvent.Title = title;
            request.CarEvent.Date = date.ToUnixDate();
            request.CarEvent.Mileage = mileage;
            request.CarEvent.Comment = comment;
            request.CarEvent.Type = type;
            var response = CarWorker.UpdateCarEvent(request, Request);
            if (response.Type != ResponseType.Success)
            {
                return Json(new { success = false, message = response.ResponseMessage });
            }
            return Json(new { success = true, message = "Oki doki" });
        }

        [HttpPost]
        public ActionResult DeleteCarEvent(int carEventId)
        {
            var token = Request.GetAuthToken();
            var request = new DeleteCarEventRequest();
            request.Token = token;
            request.CarEventId = carEventId;
            var response = CarWorker.DeleteCarEvent(request, Request);
            if (response.Type != ResponseType.Success)
            {
                return Json(new { success = false, message = response.ResponseMessage });
            }
            return Json(new { success = true, message = "Oki doki" });
        }
    }
}
