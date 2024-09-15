using System.Collections.Generic;
using System.IO;
using Xceed.Words.NET;

namespace ReplaceDocumentContent
{
    public static class WordReplacer
    {
        public static void Execute(string templatePath, string outPath, Dictionary<string, string> replaceWords)
        {
            using (var doc = DocX.Load(templatePath))
            {
                foreach (var wd in doc.Paragraphs)
                {
                    foreach (var replaceWord in replaceWords)
                    {
                        wd.ReplaceText("<" + replaceWord.Key + ">", replaceWord.Value);
                    }
                }
                if (File.Exists(outPath))
                {
                    File.Delete(outPath);
                }
                doc.SaveAs(outPath);
            }
        }
    }
}
