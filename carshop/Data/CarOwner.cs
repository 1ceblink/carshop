namespace carshop.Data {
    public class CarOwner : IObjectInfo {
        public string Nickname { get; private set; } = "Guest";
        public string Mail { get; private set; } = string.Empty;
        public string Password { get; private set; } = string.Empty;

        public CarOwner(string nickname, string mail, string password) {
            if (nickname is not null && mail is not null && password is not null) {
                (Nickname, Mail, Password) = (nickname, mail, password);
            } else {
                throw new ArgumentException(mail, nameof(nickname));
            }
        }
    }
}