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
            string s_test_dir = @"C:\Users\Dasha\flexberry.github.io\pages\products\flexberry-winforms\controls";
            s_test_dir = @"C:\Users\Dasha\flexberry.github.io\pages\products\flexberry-winforms";

            string[] fullfilesPath =
                    Directory.GetFiles(s_test_dir, "fw_*.ru.*",
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
                    Console.WriteLine(fileName);
                }
            }

            Console.WriteLine("Ready!!!");

        }
    }
}
