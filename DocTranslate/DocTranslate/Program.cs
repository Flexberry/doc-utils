﻿namespace FlexDocCheckLinks
{
    using System;
    using System.IO;
    using DocTranslate;

    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                throw new Exception("Missing args: path to files and/or Yandex API key");
            }

            string workingDirectory = args[0];
            string sAPIKey = args[1];
            Console.WriteLine($"Directory: {workingDirectory}");

            string[] fullFilePaths = Directory.GetFiles(workingDirectory, "*.ru.md", SearchOption.AllDirectories);

            ArticleTranslator articleTranslator = new ArticleTranslator(sAPIKey);

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
