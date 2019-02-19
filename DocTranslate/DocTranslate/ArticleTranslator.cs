namespace DocTranslate
{
    using System;
    using System.IO;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Класс, содержащий в себе методы разбора/сбора статьи и передачи частей в переводчик.
    /// </summary>
    internal class ArticleTranslator
    {
        /// <summary>
        /// Ключ Yandex Translator API.
        /// </summary>
        private string sAPIKey;

        /// <summary>
        /// Число пропущенных файлов (не изменялись с последнего перевода).
        /// </summary>
        private int skippedOld = 0;

        /// <summary>
        /// Число пропущенных файлов (`autotranslated: false`).
        /// </summary>
        private int skippedManual = 0;

        /// <summary>
        /// Число переведённых файлов.
        /// </summary>
        private int translated = 0;

        /// <summary>
        /// Число пропущенных файлов (не изменялись с последнего перевода).
        /// </summary>
        public int SkippedOld { get => this.skippedOld; set => this.skippedOld = value; }

        /// <summary>
        /// Число пропущенных файлов (`autotranslated: false`).
        /// </summary>
        public int SkippedManual { get => this.skippedManual; set => this.skippedManual = value; }

        /// <summary>
        /// Число переведённых файлов.
        /// </summary>
        public int Translated { get => this.translated; set => this.translated = value; }

        /// <summary>
        /// Ключ Yandex Translator API.
        /// </summary>
        public string SAPIKey { get => sAPIKey; set => sAPIKey = value; }

        /// <summary>
        /// Конструктор класса ArticleTranslator.
        /// </summary>
        /// <param name="sAPIKey">Ключ API Yandex Translate.</param>
        public ArticleTranslator(string sAPIKey)
        {
            this.SAPIKey = sAPIKey;
        }

        /// <summary>
        /// Переводит статью из файла, результат перевода записывает в файл.
        /// </summary>
        /// <param name="fileName">Имя файла.</param>
        public void TranslateFile(string fileName)
        {
            string shortFileName = Path.GetFileName(fileName);
            string newFile = Path.Combine(Path.GetDirectoryName(fileName), shortFileName.Replace(".ru.", ".en."));
            StreamReader reader;
            string content;

            // Если уже существует файл .en.md.
            if (File.Exists(newFile))
            {
                reader = new StreamReader(newFile);
                content = reader.ReadToEnd();
                reader.Close();

                // Если .en.md изменён позже, чем .ru.md, то пропускаем.
                if (File.GetLastWriteTimeUtc(newFile) > File.GetLastWriteTimeUtc(fileName))
                {
                    this.SkippedOld++;
                    return;
                }

                Regex autotranslated = new Regex(@"\nautotranslated: *false", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                if (autotranslated.Match(content).Success)
                {
                    this.SkippedManual++;
                    Console.WriteLine($"File {newFile} is marked `autotranslated: false`. Skipping...");
                    return;
                }
            }

            reader = new StreamReader(fileName);
            content = reader.ReadToEnd();
            reader.Close();
            Console.WriteLine($"Translating: {shortFileName}");
            Regex patternCodeBlock = new Regex(@"```(?<примеркода>.*?)```", RegexOptions.Singleline);

            // Сначала уберём блоки кода, т.к. в них надо перевести только комментарии.
            string preparedContent = patternCodeBlock.Replace(content, m => "cdblck" + m.Index);

            // Сами подготовим шапку файла.
            preparedContent = preparedContent.Replace("lang: ru", "lang: en \nautotranslated: true")
                                              .Replace("permalink: ru/", "permalink: en/");

            // Экранируем символы, с которыми не работает переводчик Yandex.
            preparedContent = preparedContent.Replace("#", "Zgl") // Данный символ недопустим - переводчик Yandex падает.
                            .Replace(";", "tchkzpt") // По данному символу переводчик Yandex обрезает текст.
                            .Replace("&", "mprsnd") // По данному символу переводчик Yandex обрезает текст.
                            .Replace("`", "pstrf"); // Данный символ переводчик Yandex удаляет.

            string translatedContent = this.TranslateLongText(preparedContent);

            // Восстановим экранированные символы.
            translatedContent = translatedContent.Replace("Zgl", "#")
                                                 .Replace("tchkzpt", ";")
                                                 .Replace("mprsnd", "&")
                                                 .Replace("pstrf", "`");

            // Восстановим блоки кода, переведя в них комментарии.
            for (int i = 0; i <= patternCodeBlock.Matches(content).Count - 1; i++)
            {
                var m = patternCodeBlock.Matches(content)[i];
                translatedContent = translatedContent.Replace(
                    "cdblck" + m.Index,
                    this.TranslateCodeBlock(m.Value));
            }

            // Добавим текст, необходимый согласно Лицензии на использование Яндекс.Переводчик.
            translatedContent = translatedContent + "\n\n\n # Переведено сервисом «Яндекс.Переводчик» http://translate.yandex.ru/";

            if (!File.Exists(newFile))
            {
                File.AppendAllText(newFile, translatedContent);
            }
            else
            {
                StreamWriter writer = new StreamWriter(newFile);
                writer.Write(translatedContent);
                writer.Close();
            }

            this.Translated++;
        }

        /// <summary>
        /// Передаёт содержимое в переводчик.
        /// </summary>
        /// <param name="preparedContent">Подготовленное содержимое.</param>
        /// <returns>Переведённое содержимое</returns>
        private string TranslateLongText(string preparedContent)
        {
            YandexTranslator yandexTranslator = new YandexTranslator(SAPIKey);

            if (preparedContent.Length <= 3000)
            {
                return yandexTranslator.Translate(preparedContent, "ru-en");
            }

            // Находим ближайший конец предложения (для упрощения заканчивающегося точкой).
            int lastIndexOfDot = preparedContent.Substring(0, 3000).LastIndexOf('.');
            string firstHalf = preparedContent.Substring(0, lastIndexOfDot + 1);
            string secondHalf = preparedContent.Substring(lastIndexOfDot + 1);
            if (firstHalf != string.Empty && secondHalf != string.Empty)
            {
                return yandexTranslator.Translate(firstHalf, "ru-en") + this.TranslateLongText(secondHalf);
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Метод, осуществляющий перевод комментариев в коде.
        /// </summary>
        /// <param name="codeStr">Блок кода.</param>
        /// <returns>Блок кода с переведёнными комментариями.</returns>
        private string TranslateCodeBlock(string codeStr)
        {
            var blockComments = @"(/\*([^*]|[\r\n]|(\*+([^*/]|[\r\n])))*\*+/)";
            var lineComments = @"//(.*?)\r?\n";
            var strings = @"""((\\[^;\n]|[^"";\n])*)""";
            var verbatimStrings = @"@(""[^""]*"")+";
            var htmlComment = @"(?=<!--)([\s\S]*?)-->";

            Regex pattern = new Regex(blockComments + "|" + lineComments + "|" + strings + "|" + verbatimStrings + "|" + htmlComment, RegexOptions.Compiled);

            codeStr = codeStr.Replace("#", "Zgl") // Данный символ недопустим - переводчик Yandex падает.
                .Replace(";", "tchkzpt") // По данному символу переводчик Yandex обрезает текст.
                .Replace("&", "mprsnd") // По данному символу переводчик Yandex обрезает текст.
                .Replace("`", "pstrf"); // Данный символ переводчик Yandex удаляет.

            YandexTranslator yandexTranslator = new YandexTranslator(SAPIKey);
            string res = pattern.Replace(codeStr, m => yandexTranslator.Translate(m.Value, "ru-en"));

            // Восстановим экранированные символы.
            res = res.Replace("Zgl", "#")
                                        .Replace("tchkzpt", ";")
                                        .Replace("mprsnd", "&")
                                        .Replace("pstrf", "`");
            return res;
        }
    }
}
