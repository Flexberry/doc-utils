using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FlexDocCheckLinks
{
    public class DocRef
    {
        public string ArticleFile { get; set; }
        public string ArticleDirectory { get; set; }
        public string ArticleFullFilePath { get; set; }

        public string FullRef { get; set; }
        public string RefText { get; set; }
        public string RefPath { get; set; }

    }

        class LinkChecker
        {

        private static List<string> all_articles = new List<string>();// list of all possible references (all articles in doc)
        private static List<DocRef> check_links = new List<DocRef>();// // list of references, mentioned in necessary articles, i.e. references for check
        private static List<DocRef> check_imagelinks = new List<DocRef>();// // list of images, mentioned in necessary articles, i.e. images for check

        public static List<DocRef> GetBrokenReferences(string doc_dir, string file_mask = "*.*")
        {
            GetLinkList(doc_dir, file_mask );
            return CheckLinkList(doc_dir);
        }

        // Check links in list check_links: it must be in all_articles
        private static List<DocRef> CheckLinkList(string doc_dir)
        {

            List<DocRef> broken_links = new List<DocRef>();

            // check links to other articles
            foreach (DocRef a_link in check_links)
            {                                                
                if (!string.IsNullOrEmpty(a_link.RefPath))
                {
                    // don't check internet links and anchors
                    if (!a_link.RefPath.StartsWith("http") && !a_link.RefPath.StartsWith("#"))
                    {
                        if (!all_articles.Contains(a_link.RefPath))
                        {
                            // no such article found in doc
                            broken_links.Add(a_link);
                        }

                    }
                }
                else
                {
                    // reference is empty
                    broken_links.Add(a_link);
                }
                                                                
            }

            // check links to pictures
            foreach (DocRef a_link in check_imagelinks)
            {
                if (!string.IsNullOrEmpty(a_link.RefPath))
                {                   
                    string im_file = a_link.RefPath.Replace(@"/", @"\");
                    if (im_file.StartsWith(@"\")) im_file = im_file.Substring(1);
                    try
                    {
                        if (!File.Exists(Path.Combine(doc_dir.Replace(@"\pages\products", string.Empty), im_file)))
                        {
                            // no such image found in file system
                            broken_links.Add(a_link);
                        }
                    }
                    catch
                    {
                        broken_links.Add(a_link);
                    }
                }
                else
                {
                    // reference is empty
                    broken_links.Add(a_link);
                }

            }

            return broken_links;

        }


        // Creates lists of 
        //      1) all possible references (all articles in doc)
        //      2) all references, mentioned in necessary articles, i.e. references for check
        //      3) all images, mentioned in necessary articles, i.e. images for check
        private static void GetLinkList(string doc_dir, string file_mask)
        {
            // Get all files of doc dir, because any one can be referenced
            string[] fullfilesPath =
                    Directory.GetFiles(doc_dir,"*.md",
                    SearchOption.AllDirectories);

            string[] checkFiles =
                    Directory.GetFiles(doc_dir, file_mask,
                    SearchOption.AllDirectories);

            all_articles.Clear();
            check_links.Clear();

            //Regex rgx_file_mask = new Regex(file_mask);
            foreach (string fileName in fullfilesPath)
            {
                // It is needed to read any article, because it can be referenced
                StreamReader reader = new StreamReader(fileName);
                string content = reader.ReadToEnd();
                reader.Close();

                // Add each article to full list of files that can be referenced                
                Regex pattern = new Regex(@"permalink: (?<путь>.*?)\r\n");
                if (pattern.Matches(content).Count == 1)
                    all_articles.Add(pattern.Matches(content)[0].Groups["путь"].Value.Replace("ru/",string.Empty).Replace("en/",string.Empty));// Full list of files that can be referenced


                // If file is in files for check, i.e. it's name is like file_mask, than it is necessary to get all links in the file
                //string fname = Path.GetFileName(fileName);
                //if (rgx_file_mask.IsMatch(fname))
                // If file is in files for check (thirst we get such files names), than it is necessary to get all links in the file
                if (checkFiles.Contains(fileName))
                {
                    Regex pattern_link = new Regex(@"\[(?<текст>.*?)\]\((?<путь>.*?)\)");                    
                    for (int i = 0; i <= pattern_link.Matches(content).Count - 1; i++)
                    {
                        string sRef = pattern_link.Matches(content)[i].Groups["путь"].Value;
                        string sText = pattern_link.Matches(content)[i].Groups["текст"].Value;
                        if (!content.Contains("!" + pattern_link.Matches(content)[i].Value))
                            check_links.Add(new DocRef()
                                            { ArticleFullFilePath = fileName,
                                              ArticleFile = Path.GetFileName(fileName),
                                              ArticleDirectory = Path.GetDirectoryName(fileName),
                                              RefPath = sRef,
                                              RefText = sText,
                                              FullRef = string.Format("[{0}]({1})", sText, sRef)
                                            }
                                       );
                        else
                            check_imagelinks.Add(new DocRef()
                                            {
                                                ArticleFullFilePath = fileName,
                                                ArticleFile = Path.GetFileName(fileName),
                                                ArticleDirectory = Path.GetDirectoryName(fileName),
                                                RefPath = sRef,
                                                RefText = sText,
                                                FullRef = string.Format("[{0}]({1})", sText, sRef)
                                            }
                                       );


                    }

                }               
            }
        }
    }


}
