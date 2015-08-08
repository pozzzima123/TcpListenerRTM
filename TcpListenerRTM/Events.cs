using System;
using System.Collections.Generic;

namespace TcpListenerRTM
{


    //wiadomosci TCP
    public delegate void TcpMessagesFromSocketHandler(object source, UiPrint e);
    public delegate void TcpMessagesFromListenerHandler(object source, UiPrint e);

    //wiadomosci do bazy
    public delegate void MysqlMessagesHandler(object source, UiPrint e);
    public delegate void SqliteMessageHandler(object source, UiPrint e);

    //manipulacja danymi jSON
    public delegate void JsonDataConnectedHandler(object source, DataWrite e);

    //lista adresow ip socketów
    public delegate void ConnectedSocketsHandler(object source, ListWrite e);


    public class UiPrint : EventArgs
    {
        private readonly string _message;
        public UiPrint(string text)
        {
            _message = text;
        }
        public string GetMessage()
        {
            return _message;
        }
    }

    public class DataWrite : EventArgs
    {
        private readonly string[] _message;
        public DataWrite(string[] text)
        {
            _message = text;
        }
        public string[] GetMessage()
        {
            return _message;
        }
    }

    public class ListWrite : EventArgs
    {
        private readonly List<string> _message;
        public ListWrite(List<string> text)
        {
            _message = text;
        }
        public List<string> GetMessage()
        {
            return _message;
        }
    }
}
