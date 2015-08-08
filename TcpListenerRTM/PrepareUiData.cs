using System;
using System.Linq;
using Windows.Networking;
using Windows.Networking.Connectivity;

namespace TcpListenerRTM
{

    public class PrepareUiData
    {
        //jak dlugi musi byc string by nalezalo go wyczyscic
        public int MaxSizeStr { get; }


        public PrepareUiData(int howBigStrToUi)
        {
            MaxSizeStr = howBigStrToUi;
        }


        //pobierz adres IP lokalny
        public string GetIpAddress()
        {
            ConnectionProfile icp = NetworkInformation.GetInternetConnectionProfile();

            if (icp?.NetworkAdapter == null) return "Error!";
            HostName hostname =
                NetworkInformation.GetHostNames()
                    .SingleOrDefault(
                        hn =>
                            hn.IPInformation?.NetworkAdapter != null && 
                            hn.IPInformation.NetworkAdapter.NetworkAdapterId
                            == icp.NetworkAdapter.NetworkAdapterId);

            return hostname != null ? hostname.CanonicalName : "Error!";
        }

        //sprawdz czy port jest dobry
        public bool ValidPort(string s)
        {
            try
            {
                int portValidate = Int32.Parse(s);
                return portValidate >= 0 && portValidate <= 65535;
            }
            catch
            {
                return false;
            }
        }


        //sprawdzanie poprawnosci adresu IP
        public bool IsIpCorrect(string ip)
        {
            try
            {
                string checkStr = ip.Replace(".", "");
                int checkInt;

                //sprawdz czy mozna skonwertowac calosc na stringa
                bool result = Int32.TryParse(checkStr, out checkInt);
                if (result)
                {
                    string[] checkLengthString = ip.Split('.');
                    int[] checkLengthInt = new int[4];

                    for (int i = 0; i < checkLengthInt.Length; i++)
                    {
                        checkLengthInt[i] = Int32.Parse(checkLengthString[i]);
                        //jesli adres ma nieprawidlowa wartosc zwroc falsz
                        if (checkLengthInt[i] < 0 || checkLengthInt[i] > 255) return false;
                    }

                    //jesli wszystko okej pozwol
                    return true;
                }

                //wyjatek, localhost
                if (ip == "localhost") return true;
                return false;
            }
            catch
            {
                return false;
            }
        }

        //czyszczenie zbyt dlugich stringow
        public string BufferStrCheck(string checkMe)
        {

            if (checkMe.Length > MaxSizeStr)
            {
                char[] checkMeArray = checkMe.ToCharArray();
                int strToDelete = checkMe.Length - MaxSizeStr;
                string output = "";
                for (int i = (strToDelete * 10); i < checkMeArray.Length; i++)
                {
                    output += checkMeArray[i];
                }
                return output;
            }
            return checkMe;
        }

        public string JsonToUiLb(string[] json)
        {
            //make string[] to string
            return json.Aggregate("", (current, s) => current + (s + " "));
        }
    }

}
