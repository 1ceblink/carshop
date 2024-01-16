using carshop.Data;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using NuGet.Common;
using System.Data;

namespace carshop.DBCommunicating {
    public abstract class DB {
        private readonly string pathToDBSettings;
        private MySqlConnection? connection;
        private MySqlDataAdapter adapter;
        private DataTable data;

        protected DB () {
            pathToDBSettings = @"C:\\Users\\nanap\\source\\repos\\carshop\\carshop\\dbsetting.json";
            var dbsettings = JsonConvert.DeserializeAnonymousType(File.ReadAllText(pathToDBSettings), new {
                Server = "",
                Port = "",
                Username = "",
                Password = "",
                Database = ""
            });
            if (dbsettings is { Server: not null, Port: not null, Username: not null, Password: not null, Database: not null }) {
                connection = new($"server={dbsettings!.Server};port={dbsettings.Port};username={dbsettings.Username};password={dbsettings.Password};database={dbsettings.Database}");
            }
            (adapter, data) = (new(), new());
        }
 
        private void OpenConnection() {
            if (connection?.State == ConnectionState.Closed) {
                connection.Open();
            }
        }

        private void CloseConnection() {
            if (connection?.State == ConnectionState.Open) {
                connection.Close();
            }
        }

        protected MySqlConnection? GetConnection() => connection;

        protected DataTable CallDB(MySqlCommand command) {
            lock (data) {
                if (command is not null) {
                    data.Clear();
                    OpenConnection();
                    try {
                        adapter.SelectCommand = command;
                        command.Connection = connection;
                        adapter.Fill(data);
                    }
                    finally {
                        CloseConnection();
                    }

                    return data;
                } else {
                    throw new ArgumentNullException(nameof(command));
                }
            }       
        }

        protected abstract MySqlCommand CommandConstructor<T>(string sql, T objectInfo) where T : IObjectInfo;
    }
}