using System;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using System.Collections.Generic;

namespace TcpListenerRTM
{

    public class TcpServer
    {

        private StreamSocketListener _socketTcpServer;

        //wysylanie danych od socketu
        public event TcpMessagesFromSocketHandler TcpMessagesFromSocketToUi;
        //wysyalnie danych z informacjami o statusie serwer
        public event TcpMessagesFromListenerHandler TcpMessagesFromListenerToUi;
        //wysyłanie JSON do SQLite/Mysql
        public event JsonDataConnectedHandler JsonWrite;
        //wysylanie listy podlaczonych IP
        public event ConnectedSocketsHandler ToLbConSockIp;

        //obsługa json
        private LoggingToTxt _logJson;
        public bool LogJsonEnableToTxt;

        //datareaders' list, every socket get own index
        private List<DataReader> _dataReaderix;

        //lista pdolaczonych socketów
        private List<StreamSocket> _socketsix;

        //lista podlaczonych ip
        private List<string> _socketsIpix;

        //json check
        private Json _jsonCheck = new Json();

        private bool _isStopping;


        public TcpServer()
        {
            LogJsonEnableToTxt = false;
            _dataReaderix = new List<DataReader>();
            _socketsix = new List<StreamSocket>();
            _socketsIpix = new List<string>();
            _socketTcpServer = new StreamSocketListener();
        }




        public async Task ServerOpen(int serverPort)
        {
            _isStopping = false;
            _socketTcpServer = new StreamSocketListener();
            _socketTcpServer.Control.KeepAlive = false;
            _socketTcpServer.Control.NoDelay = true;
            _socketTcpServer.ConnectionReceived += SocketTcpServerConnectionReceived;
            try
            {
                //start log session
                _logJson = new LoggingToTxt();


                //Enable tcplistener
                await _socketTcpServer.BindServiceNameAsync(serverPort.ToString());

                PrintServerSideMessage("Server started...");

            }
            catch (Exception e)
            {
                PrintServerSideMessage("Can't start server!\r\n" + e.ToString());
            }
        }


        public void StopServer()
        {
            _isStopping = true;
            try
            {
                //zamknij wszystkie sockety(muszą dostać informacje o disconnect)
                for (int i = 0; i < _socketsix.Count; i++)
                {
                    _dataReaderix[i].Dispose();
                    _socketsix[i].Dispose();
                }
                _socketsix?.Clear();
                _dataReaderix?.Clear();
                _socketsIpix?.Clear();

                ToLbConSockIp?.Invoke(this, new ListWrite(_socketsIpix));
                PrintServerSideMessage("Server closed.");
                _socketTcpServer.Dispose();
                _logJson = null;
            }
            catch (Exception e)
            {
                PrintServerSideMessage("Server error during closing." + e.ToString());
            }

        }

        private void PrintServerSideMessage(string whatToTell)
        {
            string timeNow = DateTime.Now.ToString(@"MM\/dd\/yyyy HH:mm:ss ");
            TcpMessagesFromListenerToUi?.Invoke(this, new UiPrint(timeNow + whatToTell + "\r\n"));
        }

        private void PrintSocketMessage(string whatToTell)
        {
            string timeNow = DateTime.Now.ToString(@"MM\/dd\/yyyy HH:mm:ss ");
            TcpMessagesFromSocketToUi?.Invoke(this, new UiPrint(timeNow + whatToTell + "\r\n"));
        }

        private void SocketTcpServerConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            if (_isStopping) return;
            //sprawdź nowe połączenie, jeśli dany socket zawiera te same IP co już połączony socket
            //rozłącz starego, połącz nowego
            for (int i = 0; i < _socketsix.Count; i++)
            {
                if (_socketsix[i] == null) continue;
                if (!_socketsix[i].Information.RemoteHostName.DisplayName.Contains
                    (args.Socket.Information.RemoteHostName.DisplayName)) continue;
                DisposeSocket(i);
            }

            //dodaj IP nowo połączonego socketu
            if (!_socketsIpix.Contains(args.Socket.Information.RemoteHostName.DisplayName))
            {
                //LIST
                _socketsIpix.Add(args.Socket.Information.RemoteHostName.DisplayName);
                ToLbConSockIp?.Invoke(this, new ListWrite(_socketsIpix));
                //////
                PrintServerSideMessage($"Connected with {args.Socket.Information.RemoteHostName.DisplayName}");
            }
            //jeśli lista zawiera IP nowego socketu znaczy , że łączy się ponownie po nieprawidłowym disconnect
            else if(_socketsIpix.Contains(args.Socket.Information.RemoteHostName.DisplayName))
            {
                PrintServerSideMessage($"Connection reset with {args.Socket.Information.RemoteHostName.DisplayName}");
            }

            //etap sprawdzania czy w liscie socketow jest puste miejsce, 
            //jesli tak przypisz socket ktoremus indexowi
            for (int findNull = 0; findNull < _socketsix.Count; findNull++)
            {
                if (_socketsix[findNull] != null) continue;


                //dodaj nowa pozycje do listy otwierajac nowy socket
                _socketsix[findNull] = args.Socket;
                //konfiguracja odbierania danych
                _dataReaderix[findNull] = new DataReader(_socketsix[findNull].InputStream)
                {
                    UnicodeEncoding = UnicodeEncoding.Utf8,
                    ByteOrder = ByteOrder.LittleEndian,
                    InputStreamOptions = InputStreamOptions.Partial
                };
                //czekaj na dane
                WaitForData(_socketsix[findNull], findNull);
                //zakończ konfigurowanie nowego połączenia jeśli przypisano socketowi miejsce
                return;
            }

            //dodaj nowa pozycje do listy otwierajac nowy socket
            _socketsix.Add(args.Socket);
            //konfiguracja odbierania danych
            _dataReaderix.Add(new DataReader(_socketsix[_socketsix.Count - 1].InputStream)
            {
                UnicodeEncoding = UnicodeEncoding.Utf8,
                ByteOrder = ByteOrder.LittleEndian,
                InputStreamOptions = InputStreamOptions.Partial
            });
            //czekaj na dane
            WaitForData(_socketsix[_socketsix.Count - 1], _socketsix.Count - 1);
        }

        private async void WaitForData(StreamSocket socket, int indexSocket)
        {
            try
            {
                if (_isStopping) return;
                int strLength = 4096; //maksymalny rozmiar wiadomosci
                uint numStrBytes = await _dataReaderix[indexSocket].LoadAsync((uint)strLength); //odczytaj długośc odczytanej wiadomości
                string msgFromSockets = _dataReaderix[indexSocket].ReadString(numStrBytes);
                bool toDisconnect = false; //zmienna, która pozwala na kontynuowanie/zerwanie pętli

                //odebrano wiadomość
                if (msgFromSockets != "")
                {
                    //sprawdzanie czy wiadomość jest Json
                    Json.JsonInfo checkMsg = _jsonCheck.IsJson(msgFromSockets);

                    if (checkMsg == Json.JsonInfo.Correct)
                    {
                        string[] ramka = new Json().Decode(msgFromSockets);

                        if (LogJsonEnableToTxt) await _logJson.Write(ramka);

                        //wyslij dane do bazy
                        JsonWrite?.Invoke(this, new DataWrite(ramka));
                    }
                    else if (checkMsg == Json.JsonInfo.NotJson)
                    {
                        PrintSocketMessage(" Received " + "(from " + socket.Information.RemoteHostName.DisplayName
                                           + "): " + msgFromSockets);
                    }
                }
                //jesli socket wysyla znak do rozłączenia wyczyśc socket
                else if (msgFromSockets == "")
                {
                    //usuń socket z listy i usuń go
                    if (_socketsIpix.Contains(socket.Information.RemoteHostName.DisplayName))
                    {
                        DisposeSocket(indexSocket);

                        //sygnał do rozłączenia
                        toDisconnect = true;
                    }
                }
                //czekaj na dane
                if (toDisconnect != true) WaitForData(socket, indexSocket);
            }
            catch (Exception e)
            {
                //jesli string zawiera informacje o nagłym rozłączeniue usuń socketa
                if (e.ToString().Contains("An existing connection was forcibly closed by the remote host."))
                {
                    DisposeSocket(indexSocket);
                    return;
                }

                //sprobuj podtrzymac socket, jeśli się nie da rozłacz
                try
                {
                    WaitForData(socket, indexSocket);
                }
                catch
                {
                    DisposeSocket(indexSocket);
                }  
            }
                
        }


        //usuwanie adresu IP ktory sie odlaczyl
        private void ConnectedIpDelete(string socketIp)
        {
            //LIST
            if (!_socketsIpix.Contains(socketIp)) return; //sprawdz czy na pewno dany socket jest na liscie IP
            _socketsIpix?.Remove(socketIp);
            ToLbConSockIp?.Invoke(this, new ListWrite(_socketsIpix));
            /////
            PrintServerSideMessage(socketIp + " was disconnected");
        }

        //dezaktywuj streamsocket , datareader
        private void DisposeSocket(int index)
        {
            ConnectedIpDelete(_socketsix[index].Information.RemoteHostName.DisplayName);
            _dataReaderix[index]?.Dispose();
            _dataReaderix[index] = null;
            _socketsix[index]?.Dispose();
            _socketsix[index] = null;
        }

   } 
}
