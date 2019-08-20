namespace DocTranslate
{
    using System.Configuration;
    using System.IO;
    using System.Net;
    using Newtonsoft.Json;

    internal class YandexTranslator
    {
        /// <summary>
        /// Ключ Yandex Translator API.
        /// </summary>
        private string sAPIKey;

        /// <summary>
        /// Ключ Yandex Translator API.
        /// </summary>
        public string SAPIKey { get => sAPIKey; set => sAPIKey = value; }

        /// <summary>
        /// Конструктор класса YandexTranslator.
        /// </summary>
        /// <param name="sAPIKey">Ключ API Yandex Translate.</param>
        public YandexTranslator(string sAPIKey)
        {
            this.SAPIKey = sAPIKey;
        }

        public string Translate(string s, string lang)
        {
            if (s.Length > 0)
            {
                WebRequest request = WebRequest.Create("https://translate.yandex.net/api/v1.5/tr.json/translate?"
                    + "key=" + sAPIKey
                    + "&text=" + s
                    + "&lang=" + lang);

                bool useProxy = false;
                string proxyUrl = ConfigurationManager.AppSettings["ProxyUrl"];
                int proxyPort = 0;
                if (!string.IsNullOrEmpty(proxyUrl))
                {
                    useProxy = int.TryParse(ConfigurationManager.AppSettings["ProxyPort"], out proxyPort);
                }

                if (useProxy)
                {
                    request.Proxy = new WebProxy(proxyUrl, proxyPort);
                }

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
