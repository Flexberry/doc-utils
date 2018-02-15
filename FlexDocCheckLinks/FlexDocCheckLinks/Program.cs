using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlexDocCheckLinks
{
    class Program
    {
        static void Main(string[] args)
        {
            string doc_dir = @"C:\Users\Dasha\flexberry.github.io\pages\products";            
            string file_mask = "*.*";// @"fw_*.ru.*";

            var t = LinkChecker.GetBrokenReferences(doc_dir, file_mask);
            foreach (DocRef a in t)
            {
                Console.WriteLine(string.Format("Статья: {0} \r\n Не найдено: {1} \r\n Ссылка указана: {2} \r\n\r\n", a.ArticleFile, a.RefPath, a.FullRef));                
            }

            Console.ReadLine();

        }
    }
}
