using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DocTranslate
{
    class ArticleTranslator
    {

        // переводит статью из файла, результат перевода записывает в файл
        public void translateFile(string fileName)
        {
            StreamReader reader = new StreamReader(fileName);
            string content = reader.ReadToEnd();
            reader.Close();            

            Regex patternCodeBlock = new Regex(@"```(?<примеркода>.*?)```", RegexOptions.Singleline);

            // сначала уберём блоки кода, т.к. в них надо перевести только комментарии
            string preparedContent = patternCodeBlock.Replace(content, m => "cdblck" + m.Index);

            // сами подготовим шапку файла
            preparedContent = preparedContent.Replace("lang: ru", "lang: en \nautotranslated: true")
                                              .Replace("permalink: ru/", "permalink: en/");

            // экранируем символы, с которыми не работает переводчик Yandex
            preparedContent = preparedContent.Replace("#", "Zgl") // данный символ недопустим - переводчик Yandex падает
                            .Replace(";", "tchkzpt") // по данному символу переводчик Yandex обрезает текст
                            .Replace("&", "mprsnd") // по данному символу переводчик Yandex обрезает текст
                            .Replace("`", "pstrf"); // данный символ переводчик Yandex удаляет
           
            string translatedContent = translateLongText(preparedContent);

            // восстановим экранированные символы
            translatedContent = translatedContent.Replace("Zgl", "#")
                                                 .Replace("tchkzpt", ";")
                                                 .Replace("mprsnd", "&")
                                                 .Replace("pstrf", "`");


            // восстановим блоки кода, переведя в них комментарии
            for (int i = 0; i <= patternCodeBlock.Matches(content).Count - 1; i++)
            {
                var m = patternCodeBlock.Matches(content)[i];
                translatedContent = translatedContent.Replace("cdblck" + m.Index,
                    translateCodeBlock(m.Value));
            }

            // добавим текст, необходимый согласно Лицензии на использование Яндекс.Переводчика
            translatedContent = translatedContent + "\n\n\n # Переведено сервисом «Яндекс.Переводчик» http://translate.yandex.ru/";
           

            string shortFileName = Path.GetFileName(fileName);
            string newFile = Path.Combine(Path.GetDirectoryName(fileName), shortFileName.Replace(".ru.", ".en."));

            if (!File.Exists(newFile))
                File.AppendAllText(newFile, translatedContent);
            else
            {
                StreamWriter writer = new StreamWriter(newFile);
                writer.Write(translatedContent);
                writer.Close();
            }
        }

        private string translateLongText(string preparedContent)
        {
            YandexTranslator yt = new YandexTranslator();

            if (preparedContent.Length <= 3000)
            {                
                return yt.Translate(preparedContent, "ru-en");
            }

            // находим ближайший конец предложения (для упрощения заканчивающегося точкой)
            int lastIndexOfDot = preparedContent.Substring(0, 3000).LastIndexOf('.');
            return yt.Translate(preparedContent.Substring(0, lastIndexOfDot+1), "ru-en") + translateLongText(preparedContent.Substring(lastIndexOfDot+1));
        }

        // в блоке кода переводит комментарии, как однострочные, так и многострочные
        private string translateCodeBlock(string codeStr)
        {
            var blockComments = @"(/\*([^*]|[\r\n]|(\*+([^*/]|[\r\n])))*\*+/)";
            var lineComments = @"//(.*?)\r?\n";
            var strings = @"""((\\[^;\n]|[^"";\n])*)""";
            var verbatimStrings = @"@(""[^""]*"")+";

            Regex pattern = new Regex(blockComments + "|" + lineComments + "|" + strings + "|" + verbatimStrings, RegexOptions.Compiled);

            YandexTranslator yt = new YandexTranslator();
            return pattern.Replace(codeStr, m => yt.Translate(m.Value, "ru-en"));

        }
    }
}
