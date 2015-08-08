using System;
using System.Text;
using MySql.Data.MySqlClient;

namespace TcpListenerRTM
{
    public static class StringHelper
    {
        // Convert byte array to string 
        public static byte[] ToUtf8ByteArray(this string str)
        {
            Encoding encoding = new UTF8Encoding();
            return encoding.GetBytes(str);
        }
    }

    class LoggingMysql
    {
        //Database Info:
        //Latin1
        //table: dane
        //table fields:
        //IDUrzadzenia, Dane, Czas, Data

        //wiadomosci z mysql
        public event MysqlMessagesHandler MysqlMessagesToUi;

        private MySqlConnection _connection;
        private MySqlDataReader _reader;
        private MySqlCommand _categoriesCommand;

        //przechowuje dane do logowania(w czasie wysylania nalezy ponownie to zrobic by miec pewnosc)
        public string DataToConnect { private get; set; }

        //flaga wewnetrzna
        private bool _isEnabled;
        //minimalny interwał wysyłania
        public int Interval=500;

        public LoggingMysql()
        {
            DataToConnect = "";
            _isEnabled = false;
        }
        public LoggingMysql(int logInterval)
        {
            DataToConnect = "";
            _isEnabled = false;
            Interval = logInterval;
        }

        private string UTF8toLatin(string source)
        {
            Encoding latin1 = Encoding.GetEncoding("Latin1");
            //konwertuj do UTF8         
            byte[] input = source.ToUtf8ByteArray();
            return latin1.GetString(input, 0, input.Length);
        }


        public void MysqlConnect(string mysqlServerAddr, string mysqlServerPort,
            string dbName, string mysqlUsername, string mysqlUserpasswd)
        {
                //try to connect, if error send error message
                string latin1Command = UTF8toLatin("server=" + mysqlServerAddr + ";port="
                + mysqlServerPort + ";database=" + dbName + ";uid=" + mysqlUsername + ";password=" + mysqlUserpasswd
                + ";Charset=utf8;");

                //Prepare a new _connection
                _connection = new MySqlConnection(latin1Command);

                try
                {
                    //open mysql client
                    _connection.Open();
                    //for next connects
                    DataToConnect = latin1Command;
                    _isEnabled = true;
                    SendMessage("Succesfuly connected to remote Database.");
                }
                catch (MySqlException e)
                {
                    //catch errors to window
                    //SendMessage(e.ToString());
                    SendMessage("Can't connect to remote Database. Please check your login data." + $"\r\n{e}");
                }
        }


        public void JsonSend(string[] jSon)
        {
            if (DataToConnect == null || !_isEnabled) return;
            try
            {

                if (jSon.Length != 4) throw new Exception("This is not property jSon data.");
                //Prepare a new _connection
                _connection = new MySqlConnection(DataToConnect);

                //open mysql client
                _connection.Open();

                string latin1Command1 = UTF8toLatin
                    ($"insert into dane(IDUrzadzenia, dane, czas, data) values('{jSon[0]}','{jSon[1]}','{jSon[2]}','{jSon[3]}');");
                _categoriesCommand = new MySqlCommand(latin1Command1, _connection);

                _reader = _categoriesCommand.ExecuteReader();
                while (_reader.Read())
                {
                }
                _reader.Dispose();

                SendMessage("Log sent to Remote database.");
            }
            catch
            {
                SendMessage("Can't connect to remote database! Check all.");
            }
        }

        public void Dispose()
        {
            SendMessage("Disconnected from remote database.");
            _connection.Dispose();
        }

        private void SendMessage(string s)
        {
            string day = DateTime.Now.ToString(@"MM\/dd\/yyyy HH:mm:ss ");
            MysqlMessagesToUi?.Invoke(this, new UiPrint(day + s + "\r\n"));
        }
    }
}
