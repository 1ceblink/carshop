using carshop.Data;
using Elfie.Serialization;
using Microsoft.AspNetCore.DataProtection.KeyManagement.Internal;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Relational;
using System.Data;

namespace carshop.DBCommunicating {
    public class UserCommunicating : DB {
        private readonly string pathToCommands = string.Empty;

        public UserCommunicating() {
            pathToCommands = @"C:\Users\nanap\source\repos\carshop\carshop\callcommands.json";

        }

        public bool AddUserToDB(CarOwner userInfo) {
            if (userInfo is not null && !CheckPresence(userInfo.Mail)) {
                CallDB(CommandConstructor("INSERT INTO `user` (nickname, mail, password) VALUES(@userNick, @userMail, @userPassword);", userInfo));
                return true;
            }
            return false;
        }

        public bool DeleteUserFromDB(CarOwner userInfo) {
            if (userInfo is not null && CheckPresence(userInfo.Mail)) {
                CallDB(CommandConstructor("DELETE FROM `user` WHERE user.mail = @userMail", userInfo));
                return true;
            }
            return false;
        }

        public bool AddCarToList(CarOwner userInfo, Car carInfo) {
            if (userInfo is null || carInfo is null || CheckLimit(userInfo)) {
                return false;
            }

            int ownerId = GetUserID(userInfo);

            CallDB(CommandConstructor("INSERT INTO `car` (brand, body, type, price, owner_id) VALUES(@carBrand, @carBody, @carType, @carPrice" + $", {ownerId});", carInfo));

            return true;
        }

        public bool DeleteCarFromList(CarOwner userInfo, Car carInfo) {
            if (userInfo is null || carInfo is null) {
                return false;
            }
            int ownerId = GetUserID(userInfo);

            DataTable data = CallDB(CommandConstructor("SELECT id FROM `car` WHERE car.owner_id" + $" = {ownerId} " +
                $"AND car.brand LIKE @carBrand AND car.body LIKE @carBody", carInfo));

            if (data.Rows.Count > 0) {
                CallDB(CommandConstructor("DELETE FROM `car` WHERE car.owner_id" + $" = {ownerId} " +
                "AND car.brand LIKE @carBrand AND car.body LIKE @carBody LIMIT 1", carInfo));
                return true;
            }

            return false;
        }
        public List<Car> AllCars() {
            List<Car> allCars = new();
            DataTable data = CallDB(new MySqlCommand("SELECT car.brand, car.type, car.body, car.price, car.owner_id, user.nickname FROM car LEFT JOIN user ON car.owner_id = user.id;"));
            if (data.Rows.Count > 0) {
                foreach (DataRow row in data.Rows) {
                    string type = row["type"].ToString()!;
                    CarType? carType;
                    if (type == "truck") { carType = CarType.Truck; }
                    else if (type == "jeep") { carType = CarType.Jeep; }
                    else { carType = CarType.Sedan; }
                    allCars.Add(new Car(row["brand"].ToString()!,
                                        row["body"].ToString()!,
                                        Convert.ToDecimal(row["price"]),
                                        carType) { OwnerNickname = row["nickname"].ToString()! });
                }
            }
            return allCars;
        }

        protected override MySqlCommand CommandConstructor<T>(string sql, T data) {
            MySqlCommand command = new(sql, GetConnection());
            if (data is CarOwner userInfo) {
                command.Parameters.Add("@userNick", MySqlDbType.VarChar).Value = userInfo.Nickname;
                command.Parameters.Add("@userMail", MySqlDbType.VarChar).Value = userInfo.Mail;
                command.Parameters.Add("@userPassword", MySqlDbType.VarChar).Value = userInfo.Password;

                return command;
            } else if (data is Car carInfo) {
                command.Parameters.Add("@carBrand", MySqlDbType.VarChar).Value = carInfo.Brand;
                command.Parameters.Add("@carBody", MySqlDbType.VarChar).Value = carInfo.Body;
                command.Parameters.Add("@carType", MySqlDbType.VarChar).Value = carInfo.Type;
                command.Parameters.Add("@carPrice", MySqlDbType.Decimal).Value = carInfo.Price;

                return command;
            }
            throw new ArgumentException(nameof(data));
        }

        protected bool CheckPresence(string mail) {
            if (mail is null) {
                throw new ArgumentException(nameof(mail));
            }
            MySqlCommand command = new("SELECT * FROM `user` WHERE user.Mail LIKE @userMail", GetConnection());
            command.Parameters.Add("@userMail", MySqlDbType.VarChar).Value = mail;

            DataTable data = CallDB(command);
            return data.Rows.Count > 0;
        }

        protected int GetUserID(CarOwner userInfo) {
            // car will be added to the id 0 (administrator) if there is no required user
            if (!CheckPresence(userInfo.Mail)) {
                return default;
            }

            DataTable data = CallDB(CommandConstructor("SELECT id FROM `user` WHERE user.Mail LIKE @userMail", userInfo));
            return Convert.ToInt32(data?.Rows[0][0]);
        }

        protected bool CheckLimit(CarOwner userInfo) {
            MySqlCommand command = new MySqlCommand("SELECT * FROM user LEFT JOIN car ON(user.id = car.owner_id) WHERE (user.id" + $" = {GetUserID(userInfo)} " + "AND car.id IS NOT NULL)");

            DataTable data = CallDB(command);

            if (data.Rows.Count >= 10) {
                return true;
            }

            return false;
        }
    }
}
