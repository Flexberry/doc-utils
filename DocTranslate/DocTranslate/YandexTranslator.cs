namespace DocTranslate
{
    using System.IO;
    using System.Net;
    using Newtonsoft.Json;

    internal class YandexTranslator
    {
        public string Translate(string s, string lang)
        {
            // NB! Вставить ключ в App.config
            string sAPIKey = System.Configuration.ConfigurationManager.AppSettings["yandexAPIKey"];

            if (s.Length > 0)
            {
                WebRequest request = WebRequest.Create("https://translate.yandex.net/api/v1.5/tr.json/translate?"
                    + "key=" + sAPIKey
                    + "&text=" + s
                    + "&lang=" + lang);

                WebResponse response = request.GetResponse();
                using (StreamReader stream = new StreamReader(response.GetResponseStream()))
                {
                    string line;

                    if ((line = stream.ReadLine()) != null)
                    {
                        Translation translation = JsonConvert.DeserializeObject<Translation>(line);

                        s = string.Empty;

                        foreach (string str in translation.Text)
                        {
                            s += str;
                        }
                    }
                }

                return s;
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
