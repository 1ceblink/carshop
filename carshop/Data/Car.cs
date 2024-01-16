namespace carshop.Data {
    public class Car : IObjectInfo {
        public string Brand { get; private set; } = "unknown";
        public string Body { get; private set; } = "unknown";
        public decimal Price { get; private set; } = 0;
        public CarType? Type { get; private set; } = CarType.Sedan;
        public string OwnerNickname { get; set; } = "unknown";

        public Car(string brand, string body, decimal price, CarType? type) {
                if (brand is not null && body is not null && type is not null) {
                (Brand, Body, Price, Type) = (brand, body, price, type);
            } else {
                throw new ArgumentNullException();
            }
        }
    }
    public enum CarType {
        Truck, Jeep, Sedan
    }
}
