using carshop.DBCommunicating;
using Microsoft.AspNetCore.Mvc;

namespace carshop.Controllers {
    public class carsController : Controller {
        private readonly UserCommunicating userComm = new();

        public ViewResult List() {
            return View("Pages/Cars/List.cshtml", userComm.AllCars());
        }
        public ViewResult AddCar() {
            return View("Pages/Cars/AddCar.cshtml");
        }
        public ViewResult OpResult(string Mail, string Password, string Brand, string Body, decimal Price, Data.CarType Type) {
            bool result = userComm.AddCarToList(new Data.CarOwner("temp", Mail, Password), new Data.Car(Brand, Body, Price, Type));

            return View("Pages/Cars/OpResult.cshtml", Convert.ToInt32(result));
        }
    }
}
