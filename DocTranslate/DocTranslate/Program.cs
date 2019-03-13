namespace FlexDocCheckLinks
{
    using System;
    using System.IO;
    using DocTranslate;

    internal class Program
    {
        private static void Main(string[] args)
        {
            bool force = false;
            if (args.Length < 2 || args.Length > 3)
            {
                throw new Exception("Incorrect args. First arg should be working directory. Second arg should be API key. Third arg (optional) word 'force' if force translation is required.");
            }

            if (args.Length == 3)
            {
                force = args[2] == "force";
                if (force)
                {
                    Console.WriteLine("WARN: Force mode on!");
                } else
                {
                    Console.WriteLine($"WARN: Expected 'force' as third arg, got {args[2]}");
                }
            }

            string workingDirectory = args[0];
            string sAPIKey = args[1];
            Console.WriteLine($"Directory: {workingDirectory}");

            string[] fullFilePaths = Directory.GetFiles(workingDirectory, "*.ru.md", SearchOption.AllDirectories);

            ArticleTranslator articleTranslator = new ArticleTranslator(sAPIKey, force);

            int counter = 0;

            foreach (string fileName in fullFilePaths)
            {
                Console.WriteLine($"{counter} out of {fullFilePaths.Length}");
                try
                {
                    articleTranslator.TranslateFile(fileName);
                    counter++;
                }
                catch (Exception exc)
                {
                    Console.WriteLine($"{Environment.NewLine}{fileName}{Environment.NewLine}");
                    Console.WriteLine(exc.Message);
                    Console.WriteLine(exc.StackTrace);
                    break;
                }
            }

            Console.WriteLine($"Ready!!!{Environment.NewLine} Skipped old: {articleTranslator.SkippedOld}, skipped manually translated: {articleTranslator.SkippedManual}, total translated: {articleTranslator.Translated}");
        }
    }
}
