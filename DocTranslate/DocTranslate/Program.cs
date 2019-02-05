using DocTranslate;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlexDocCheckLinks
{
    class Program
    {
        static void Main(string[] args)
        {
            string s_test_dir = @"";
            Console.WriteLine($"Directory: {s_test_dir}");

            string[] fullfilesPath =
                    Directory.GetFiles(s_test_dir, "*.ru.*",
                    SearchOption.AllDirectories);

            ArticleTranslator transl = new ArticleTranslator();

            foreach (string fileName in fullfilesPath)
            {
                try
                {
                    transl.translateFile(fileName);
                }
                catch (Exception exc)
                {
                    Console.WriteLine(exc.Message);
                    Console.WriteLine(fileName);
                }
            }

            Console.WriteLine($"Ready!!!\n Skipped old: {transl.skippedOld}, skipped manually translated: {transl.skippedManual}, total translated: {transl.translated}");

        }
    }
}
