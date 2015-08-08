using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace TcpListenerRTM
{
    class LoggingToTxt
    {
        public StorageFolder Folder;
        public StorageFile File;

        public LoggingToTxt()
        {
            Setup();
        }

        public async void Setup()
        {
            Folder = ApplicationData.Current.LocalFolder;


            //połącz do pliku jesli istnieje
            try
            {
                File = await Folder.GetFileAsync("json.txt");
            }
            //utwórz gdy go nie ma
            catch
            {
                File = await Folder.CreateFileAsync("json.txt");
            }
        }

        public async Task Write(string[] jsonData)
        {
            string toWrite = jsonData.Aggregate("", (current, t) => current + (t + " "));
            string read = await FileIO.ReadTextAsync(File);
            await FileIO.WriteTextAsync(File, read + toWrite + "\r\n");
        }

        public async Task<string> Read()
        {
            string read = await FileIO.ReadTextAsync(File);
            return read;
        }
    }

}
