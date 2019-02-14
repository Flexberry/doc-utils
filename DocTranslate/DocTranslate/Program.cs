namespace FlexDocCheckLinks
{
    using System;
    using System.IO;
    using DocTranslate;

    internal class Program
    {
        private static void Main(string[] args)
        {
            string workingDirectory = System.Configuration.ConfigurationManager.AppSettings["workingDirectory"];
            Console.WriteLine($"Directory: {workingDirectory}");

            string[] fullFilePaths =
                    Directory.GetFiles(workingDirectory, "*.ru.md", SearchOption.AllDirectories);

            ArticleTranslator articleTranslator = new ArticleTranslator();

            foreach (string fileName in fullFilePaths)
            {
                try
                {
                    articleTranslator.TranslateFile(fileName);
                }
                catch (Exception exc)
                {
                    Console.WriteLine($"\n{fileName}\n");
                    Console.WriteLine(exc.Message);
                    Console.WriteLine(exc.StackTrace);
                }
            }

            Console.WriteLine($"Ready!!!\n Skipped old: {articleTranslator.SkippedOld}, skipped manually translated: {articleTranslator.SkippedManual}, total translated: {articleTranslator.Translated}");
        }
    }
}
