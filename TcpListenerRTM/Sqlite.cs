using System;
using System.Threading.Tasks;
using System.IO;
using SQLite.Net;
using System.Collections.Generic;
using System.Linq;

namespace TcpListenerRTM
{

    public class ListItem
    {
        //Ramka SQL
        public string IDUrzadzenia { get; set; }
        public string Dane { get; set; }
        public string Czas { get; set; }
        public string Data { get; set; }

    }

    class Sqlite
    {
        //pobieranie odpowiedzi z klienta sql
        public SqliteMessageHandler SqliteMessagesFromSocketToUi;

        //po polaczeniue obiekt sqlconn
        public SQLiteConnection WriteConn;

        //nazwa bazy danych
        public string DbName { get; set; }

        private string _pathToDatabase;

        private bool _isEnabled;
        public int Interval = 100;


        //pobierz dane
        public List<ListItem> GetItems()
        {
            return (from i in WriteConn.Table<ListItem>() select i).ToList();
        }

        public Sqlite()
        {
            DbName = "db.txt";
            _pathToDatabase = "";
            WriteConn = null;
            _isEnabled = false;
        }

        public Sqlite(int logInterval)
        {
            DbName = "db.s3db";
            _pathToDatabase = "";
            Interval = logInterval;
            WriteConn = null;
            _isEnabled = false;
        }

        public void Dispose()
        {
            WriteConn.Dispose();
            WriteConn.Close();
            PrintSqlDatabaseMessage("Disconnected from local database.");
            WriteConn = null;
        }

        private void PrintSqlDatabaseMessage(string s)
        {
            string timeNow = DateTime.Now.ToString(@"MM\/dd\/yyyy HH:mm:ss ");
            SqliteMessagesFromSocketToUi(this, new UiPrint(timeNow + s + "\r\n"));
        }

        public void Connect()
        {
            Task.Factory.StartNew(async () =>
            {
                try
                {
                    try
                    {
                        if (DbName == null) throw new Exception("Database name cannot be null!");
                        _pathToDatabase = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, DbName);

                    }
                    catch
                    {
                        //utworz Folder gdy błąd
                        await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFileAsync(DbName);
                        _pathToDatabase = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, DbName);
                    }

                    //polaczenie sie
                    SQLiteConnection conn =
                        new SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(),
                            _pathToDatabase);

                    conn.CreateTable<ListItem>();

                    //przypisanie obiektu do obiektu prywatnego klasy
                    WriteConn = conn;



                    PrintSqlDatabaseMessage("Connected to local database.\r\nDatabase location: " + _pathToDatabase);
                    _isEnabled = true;

                }
                catch (SQLiteException e)
                {
                    PrintSqlDatabaseMessage("Can't connect to local database.\r\n" + e.ToString());
                }
            });
        }

        public async void WriteJson(string[] json)
        {
            try
            {
                //upewnij sie ze dana ramka ma łańcuch dlugosci 4 (wedlug tego co przyjmiemy)
                if (json.Length == 4 && _isEnabled)
                {
                    //dodaj do bazy dane JSON
                    WriteConn.Insert(new ListItem()
                    {
                        IDUrzadzenia = json[0],
                        Dane = json[1],
                        Czas = json[2],
                        Data = json[3]
                    });
                    await Task.Delay(TimeSpan.FromMilliseconds(Interval));
                    PrintSqlDatabaseMessage("JSON data saved to the local database.");
                }

            }
            catch (SQLiteException e)
            {
                PrintSqlDatabaseMessage(e.ToString());
            }
        }
    }
}
