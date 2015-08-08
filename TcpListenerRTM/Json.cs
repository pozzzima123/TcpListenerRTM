
namespace TcpListenerRTM
{
    public class Json
    {
        //Ramka IDUrządzenia#dane#czas#data
        public string Code(string raw)
        {
            string outputjSon = "{";
            string[] toJson = raw.Split('#');
            outputjSon += "\"" + toJson[0] + "\":{\"data\": \"" + toJson[1] + "\",";
            outputjSon += "\"time\": \"" + toJson[2] + "\",";
            outputjSon += "\"date\": \"" + toJson[3] + "\"}}";

            return outputjSon;
        }
        public string[] Decode(string json)
        {
            string[] output = new string[4];
            string[] toSplit = json.Split('"');
            output[0] = toSplit[1];
            output[1] = toSplit[5];
            output[2] = toSplit[9];
            output[3] = toSplit[13];
            return output;
        }

        public JsonInfo IsJson(string json)
        {
            //sprawdz czy json zawiera dane wyrazenie
            if (json.Contains("\"data\":"))
            {
                string[] check = json.Split();
                //nalezy upewnic sie czy string konczy sie w przyjetej formie(wykluczenie podwojonych danych json)
                return check.Length == 4 ? JsonInfo.Correct : JsonInfo.Incorrect;
            }
            return JsonInfo.NotJson;
        }

        public enum JsonInfo
        {
            Correct,
            Incorrect,
            NotJson
        }
    }
}
