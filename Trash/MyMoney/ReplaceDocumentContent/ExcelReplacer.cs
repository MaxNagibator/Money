using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClosedXML.Excel;

namespace ReplaceDocumentContent
{
    public static class ExcelReplacer
    {
        public static void Execute(string templatePath, string outPath, Dictionary<string, string> replaceWords)
        {
            var workBook = new XLWorkbook(templatePath);
            foreach (var w in workBook.Worksheets)
            {
                foreach (var c in w.CellsUsed())
                {
                    ReplaceFormulaWithTextValue(c);
                }
            }

            void ReplaceFormulaWithTextValue(IXLCell cell)
            {
                foreach (var replaceWord in replaceWords)
                {
                    var val = cell.Value.ToString();
                    if (val.Contains("<" + replaceWord.Key + ">"))
                    {
                        cell.Value = val.Replace("<" + replaceWord.Key + ">", replaceWord.Value);
                    }
                }
            }

            if (File.Exists(outPath))
            {
                File.Delete(outPath);
            }

            workBook.SaveAs(outPath);
        }
    }
}
