using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TcpListenerRTM
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    /// 

    public sealed partial class MainPage : Page
    {


        //obiekt -> operacje na danych wprowadzanych/wyświetlanych przez GUI
        public PrepareUiData Prepare = new PrepareUiData(2000);

        //Listenerserver
        public TcpServer Tcpserver = new TcpServer();

        //sqlite
        private Sqlite _sql = new Sqlite();

        //mysql client
        private LoggingMysql _mysqlclient = new LoggingMysql();

        //wiadomości z mysql
        private string _sMysqlMessages = "";

        //wiadomości z TCP Listener Server
        private string _sTcpMessagesFromListener = "";

        //wiadomości z TCP Sockets
        private string _sTcpMessagesFromSockets = "";

        //jSON Dane do zapisu
        private string[] _sJsonArray = new string[4];

        //jSON Dane do UI
        private List<string> _sJsonix = new List<string>();


        //lista podłączonych IP
        private List<string> _connectedIPtoLbix = new List<string>();


        public MainPage()
        {
            this.InitializeComponent();
        }


        //Wyświetlaj wiadomości wysłane przez jakiś socket
        private async void GetMessagesFromSockets(object source, UiPrint e)
        {
            _sTcpMessagesFromSockets = e.GetMessage();

            await Dispatcher.TryRunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, GetMessagesFromSocketsDispatcher);
        }

        private void GetMessagesFromSocketsDispatcher()
        {

            tcpMessagesFromSocketsTb.Text += _sTcpMessagesFromSockets;
            tcpMessagesFromSocketsTb.Text = Prepare.BufferStrCheck(tcpMessagesFromSocketsTb.Text);
            //goto last line
            svTcpMessagesFromSockets.ChangeView(null, tcpMessagesFromSocketsTb.ActualHeight, null, true);
        }
        //////////////////////////////////////////////////////////////////////


        //Wyświetlaj wiadomości wysłane przez Listener(obsługę socketów) 
        private async void GetMessagesFromListener(object source, UiPrint e)
        {
            _sTcpMessagesFromListener = e.GetMessage();

            await Dispatcher.TryRunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, GetMessagesFromListenerDispatcher);
        }

        private void GetMessagesFromListenerDispatcher()
        {
            try
            {
                tcpMessagesFromListenerTb.Text += _sTcpMessagesFromListener;
                tcpMessagesFromListenerTb.Text = Prepare.BufferStrCheck(tcpMessagesFromListenerTb.Text);

                //goto last line
                svTcpMessagesFromListener.ChangeView(null, tcpMessagesFromListenerTb.ActualHeight, null, true);
            }
            catch
            {
                // ignored
            }
        }
        //////////////////////////////////////////////////////////////////

        //Wyświetlaj wiadomości wysłane przez Mysql Client
        private async void GetMessagesFromMysql(object source, UiPrint e)
        {
            _sMysqlMessages = e.GetMessage();

            await Dispatcher.TryRunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, GetMessagesFromMysqlDispatcher);
        }

        private void GetMessagesFromMysqlDispatcher()
        {

            mysqlMessagesTb.Text += _sMysqlMessages;
            mysqlMessagesTb.Text = Prepare.BufferStrCheck(mysqlMessagesTb.Text);

            //goto last line
            svReceivedTextMysql.ChangeView(null, mysqlMessagesTb.ActualHeight, null, true);
        }
        //////////////////////////////////////////////////////////////////

        //Wyświetlaj wiadomości wysłane przez Mysql Client
        private async void SqliteMessages(object source, UiPrint e)
        {
            _sMysqlMessages = e.GetMessage();

            await Dispatcher.TryRunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, SqliteMessagesDispatcher);
        }

        private void SqliteMessagesDispatcher()
        {

            mysqlMessagesTb.Text += _sMysqlMessages;
            mysqlMessagesTb.Text = Prepare.BufferStrCheck(mysqlMessagesTb.Text);

            //goto last line
            svReceivedTextMysql.ChangeView(null, mysqlMessagesTb.ActualHeight, null, true);
        }
        //////////////////////////////////////////////////////////////////

        //Wysyłanie danych json do bazy lokalnej, zdalenj
        private async void JsonWriter(object source, DataWrite e)
        {
            _sJsonArray = e.GetMessage();
            await Dispatcher.TryRunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, JsonWriterDispatcher);
        }

        private void JsonWriterDispatcher()
        {

            //UI Data
            _sJsonix.Add(Prepare.JsonToUiLb(_sJsonArray));

            if (_sJsonix.Count > 5) _sJsonix.RemoveRange(0, 1);
            jSonDataLb.ItemsSource = null;
            jSonDataLb.ItemsSource = _sJsonix;
            svJsonDataLb.ChangeView(null, jSonDataLb.ActualHeight, null, true);

            //jesli wcisnieto przycisk Enable
            if (toDbEnableButton.IsEnabled) return;
            //jesli zaznaczono wysylanie do lokalnej bazy
            if (rbMysql.IsChecked == true)
            {
                Task.Factory.StartNew(() => _mysqlclient.JsonSend(_sJsonArray));
            }
            //jesli zaznaczono wysyłanie do zdalnej bazy
            if (rbSqlite.IsChecked == true)
            {
                Task.Factory.StartNew(()=> _sql.WriteJson(_sJsonArray));
            }
        }
        //////////////////////////////////////////////////////////////////

        //Wysyłanie listty podlaczonych socketow do ListBox
        private async void ToLbIpWriter(object source, ListWrite e)
        {
            _connectedIPtoLbix = e.GetMessage();

            await Dispatcher.TryRunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, ToLbIpWriterDispatcher);
        }

        private void ToLbIpWriterDispatcher()
        {
            //IP to listbox
            connectedSocketsLb.ItemsSource = null;
            connectedSocketsLb.ItemsSource = _connectedIPtoLbix;
            
            
        }
        //////////////////////////////////////////////////////////////////

        private void startTcpButton_Click(object sender, RoutedEventArgs e)
        {
            whatIpAddress.IsEnabled = false;
            whatPort.IsEnabled = false;
            stopTcpButton.IsEnabled = true;
            startTcpButton.IsEnabled = false;
            TcpInit();
        }

        private void MysqlInit()
        {
            if (toDbEnableButton.IsEnabled) return;
            try
            {
                if (Prepare.IsIpCorrect(mysqlServerAddr.Text) != true)
                    throw new Exception("Mysql: IP Address is invalid.");

                if (Prepare.ValidPort(mysqlPort.Text) != true)
                    throw new Exception("Mysql: Port is invalid.");

                //przekazanie danych do taska
                string sMysqlServerAddr = mysqlServerAddr.Text;
                string sMysqlPort = mysqlPort.Text;
                string sMysqlDbName = mysqlDbName.Text;
                string sMysqlDbUsername = mysqlDbUsername.Text;
                string sMysqlDbUserPasswd = mysqlDbUserPasswd.Text;
                
                //startuj mysql
                Task.Factory.StartNew(() =>
                {
                    _mysqlclient.MysqlMessagesToUi += new MysqlMessagesHandler(GetMessagesFromMysql);
                    _mysqlclient.MysqlConnect(sMysqlServerAddr, sMysqlPort,
                     sMysqlDbName, sMysqlDbUsername, sMysqlDbUserPasswd);
                });

            }
            catch (Exception tellMeSthWrong)
            {
                mysqlMessagesTb.Text += tellMeSthWrong.ToString() + "\r\n";
            }
        }


        private void TcpInit()
        {
            
                string sWhatPort = whatPort.Text;
                Task.Factory.StartNew(async () =>
                {
                    try
                    {
                        //inicjalizacja eventów
                        Tcpserver.TcpMessagesFromSocketToUi += new TcpMessagesFromSocketHandler(GetMessagesFromSockets); // Wyświetlenie ilości połączonych urządzeń
                        Tcpserver.TcpMessagesFromListenerToUi += new TcpMessagesFromListenerHandler(GetMessagesFromListener);
                        Tcpserver.JsonWrite += new JsonDataConnectedHandler(JsonWriter);
                        Tcpserver.ToLbConSockIp += new ConnectedSocketsHandler(ToLbIpWriter);
                        //sprawdzenie poprawności portu
                        if (!Prepare.ValidPort(sWhatPort))
                        {
                            throw new Exception("Invalid port!");
                        }
                        await Tcpserver.ServerOpen(Int32.Parse(sWhatPort));
                    }
                    catch (Exception e)
                    {
                        tcpMessagesFromListenerTb.Text += e.ToString() + "\r\n";
                    }
                });
        }

        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            startTcpButton.IsEnabled = true;
            stopTcpButton.IsEnabled = false;
            whatPort.IsEnabled = true;
            whatIpAddress.IsEnabled = true;

            Task.Factory.StartNew(() =>
            {
                Tcpserver.StopServer();
                Tcpserver = new TcpServer();
            });

            connectedSocketsLb.ItemsSource = null;
        }

        private void cleanRIM_Click(object sender, RoutedEventArgs e)
        {
            tcpMessagesFromListenerTb.Text = "";
        }

        private void cleanMSG_Click(object sender, RoutedEventArgs e)
        {
            tcpMessagesFromSocketsTb.Text = "";
        }

        private void mysqldBEnableButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                toDbEnableButton.IsEnabled = false;
                mysqldBDisableButton.IsEnabled = true;

                mysqlServerAddr.IsEnabled = false;
                mysqlPort.IsEnabled = false;
                mysqlDbName.IsEnabled = false;
                mysqlDbUsername.IsEnabled = false;
                mysqlDbUserPasswd.IsEnabled = false;

                rbMysql.IsEnabled = false;
                rbSqlite.IsEnabled = false;

                sqlDbName.IsEnabled = false;


                cleanMYSQL.IsEnabled = true;

                if (rbMysql.IsChecked == true)
                {
                    MysqlInit();
                }
                else if (rbMysql.IsChecked != true)
                {
                    //przypisz standardową nazwę bazy jeśli pole tekstowe jest puste
                    if (sqlDbName.Text != null) _sql.DbName = sqlDbName.Text;

                    //podłącz
                    Task.Factory.StartNew(() =>
                    {
                        _sql.SqliteMessagesFromSocketToUi += new SqliteMessageHandler(SqliteMessages);
                        _sql.Connect();
                    });
                        
                }

            }
            catch
            {
                // ignored
            }
        }

        private void mysqldBDisableButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                toDbEnableButton.IsEnabled = true;
                mysqldBDisableButton.IsEnabled = false;

                mysqlServerAddr.IsEnabled = true;
                mysqlPort.IsEnabled = true;
                mysqlDbName.IsEnabled = true;
                mysqlDbUsername.IsEnabled = true;
                mysqlDbUserPasswd.IsEnabled = true;
                cleanMYSQL.IsEnabled = false;

                rbMysql.IsEnabled = true;
                rbSqlite.IsEnabled = true;

                sqlDbName.IsEnabled = true;

                if (rbMysql.IsChecked == true)
                {
                    _mysqlclient.Dispose();
                    _mysqlclient = new LoggingMysql();
                }

                if (rbSqlite.IsChecked == true)
                {
                    _sql.Dispose();
                    _sql = new Sqlite();
                }

            }
            catch
            {
                // ignored
            }
        }

        private void cleanMYSQL_Click(object sender, RoutedEventArgs e)
        {
            mysqlMessagesTb.Text = "";
        }

        private void rbMysql_Click(object sender, RoutedEventArgs e)
        {
            mysqlServerAddr.IsEnabled = true;
            mysqlPort.IsEnabled = true;
            mysqlDbName.IsEnabled = true;
            mysqlDbUsername.IsEnabled = true;
            mysqlDbUserPasswd.IsEnabled = true;

            mysqlServerAddr.Visibility = Visibility.Visible;
            mysqlServerAddrTxb.Visibility = Visibility.Visible;

            mysqlPort.Visibility = Visibility.Visible;
            mysqlPortTxb.Visibility = Visibility.Visible;

            mysqlDbName.Visibility = Visibility.Visible;
            mysqlDbNameTxb.Visibility = Visibility.Visible;

            mysqlDbUsername.Visibility = Visibility.Visible;
            mysqlDbUsernameTxb.Visibility = Visibility.Visible;

            mysqlDbUserPasswd.Visibility = Visibility.Visible;
            mysqlDbUserPasswdTb.Visibility = Visibility.Visible;

            sqlDbName.Visibility = Visibility.Collapsed;
            sqlDbNameTxb.Visibility = Visibility.Collapsed;
        }

        private void rbSqlite_Click(object sender, RoutedEventArgs e)
        {
            mysqlServerAddr.IsEnabled = false;
            mysqlPort.IsEnabled = false;
            mysqlDbName.IsEnabled = false;
            mysqlDbUsername.IsEnabled = false;
            mysqlDbUserPasswd.IsEnabled = false;

            mysqlServerAddr.Visibility = Visibility.Collapsed;
            mysqlServerAddrTxb.Visibility = Visibility.Collapsed;

            mysqlPort.Visibility = Visibility.Collapsed;
            mysqlPortTxb.Visibility = Visibility.Collapsed;

            mysqlDbName.Visibility = Visibility.Collapsed;
            mysqlDbNameTxb.Visibility = Visibility.Collapsed;

            mysqlDbUsername.Visibility = Visibility.Collapsed;
            mysqlDbUsernameTxb.Visibility = Visibility.Collapsed;

            mysqlDbUserPasswd.Visibility = Visibility.Collapsed;
            mysqlDbUserPasswdTb.Visibility = Visibility.Collapsed;

            sqlDbName.Visibility = Visibility.Visible;
            sqlDbNameTxb.Visibility = Visibility.Visible;
        }

        private void txtButtEnableLog_Click(object sender, RoutedEventArgs e)
        {
            Tcpserver.LogJsonEnableToTxt = true;
            txtButtEnableLog.Visibility = Visibility.Collapsed;
            txtButtDisableLog.Visibility = Visibility.Visible;
        }

        private void txtButtDisableLog_Click(object sender, RoutedEventArgs e)
        {
            Tcpserver.LogJsonEnableToTxt = false;
            txtButtDisableLog.Visibility = Visibility.Collapsed;
            txtButtEnableLog.Visibility = Visibility.Visible;
        }

        private void cleanJSON_Click(object sender, RoutedEventArgs e)
        {
            jSonDataLb.ItemsSource = null;
            _sJsonix.Clear();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //zdobyj lokalny adres IP
            whatIpAddress.Text = Prepare.GetIpAddress();
        }
    }


}
