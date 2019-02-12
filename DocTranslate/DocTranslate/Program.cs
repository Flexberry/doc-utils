namespace FlexDocCheckLinks
{
    using System;
    using System.IO;
    using DocTranslate;

    internal class Program
    {
        private static void Main(string[] args)
        {
            string s_test_dir = System.Configuration.ConfigurationSettings.AppSettings["workingDirectory"];
            Console.WriteLine($"Directory: {s_test_dir}");

            string[] fullfilesPath =
                    Directory.GetFiles(s_test_dir, "*.ru.*", SearchOption.AllDirectories);

            ArticleTranslator transl = new ArticleTranslator();

            foreach (string fileName in fullfilesPath)
            {
                try
                {
                    transl.TranslateFile(fileName);
                }
                catch (Exception exc)
                {
                    Console.WriteLine($"{fileName}\n");
                    Console.WriteLine(exc.Message);
                    Console.WriteLine(exc.StackTrace);
                }
            }

            Console.WriteLine($"Ready!!!\n Skipped old: {transl.SkippedOld}, skipped manually translated: {transl.SkippedManual}, total translated: {transl.Translated}");
        }
    }
}
