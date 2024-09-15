using System;
using System.Configuration;
using System.IO;
using System.Web.Mvc;
using Common.Enums;

namespace MyMoneyWeb.Controllers
{
    [Authorize]
    public class FileController : BaseController
    {
        [HttpPost]
        public ActionResult Upload()
        {
            var file = Request.Files["file"];
            if (file == null)
            {
                return Json(new { success = false });
            }
            var fileExt = Path.GetExtension(file.FileName);
            var fileName = Guid.NewGuid() + fileExt;
            file.SaveAs(Path.Combine(ConfigurationManager.AppSettings["FilesStoragePath"], fileName));

            var fileType = GetFileType(fileName);

            return Json(new { success = true, filename = fileName, fileType = fileType });
        }

        public static FileTypes GetFileType(string fileName)
        {
            var fileType = FileTypes.Unknown;
            if(fileName.IndexOf('.') == -1)
            {
                return fileType;
            }

            var fileExt = fileName.Substring(fileName.IndexOf('.')).ToLower();
            if (fileExt == ".doc" || fileExt == ".docx")
            {
                fileType = FileTypes.Word;
            }
            else if (fileExt == ".xls" || fileExt == ".xlsx")
            {
                fileType = FileTypes.Excel;
            }
            return fileType;
        }
    }

}
