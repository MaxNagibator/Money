using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Common.Enums;
using MyMoneyWeb.Models;
using MyMoneyWeb.Structure;
using ReplaceDocumentContent;
using ServiceRequest.DocumentTemplateGroups;
using ServiceResponse;
using ServiceWorker;

namespace MyMoneyWeb.Controllers
{
    public class DocumentTemplateGroupController : BaseController
    {
        [HttpPost]
        public ActionResult GetDocumentTemplateGroups()
        {
            var token = Request.GetAuthToken();
            var request = new GetDocumentTemplateGroupsRequest();
            request.Token = token;
            var response = DocumentTemplateGroupWorker.GetDocumentTemplateGroups(request, Request);
            if (response.Type != ResponseType.Success)
            {
                return PartialView("MyError", new ErrorModel { Message = response.ResponseMessage });
            }
            var model = new DocumentTemplateGroupsModel();
            model.DocumentTemplateGroups = new List<DocumentTemplateGroupModel>();
            foreach (var templateGroup in response.Body.DocumentTemplateGroups)
            {
                var d = new DocumentTemplateGroupModel();
                d.Id = templateGroup.Id;
                d.Name = templateGroup.Name;
                d.DocumentTemplates = new List<DocumentTemplateModel>();
                model.DocumentTemplateGroups.Add(d);
                foreach (var template in templateGroup.DocumentTemplates)
                {
                    var t = new DocumentTemplateModel();
                    t.Name = template.Name;
                    t.Type = FileController.GetFileType(template.DownloadFileName);
                    t.DownloadFileName = template.DownloadFileName;
                    t.FileName = template.FileName;
                    d.DocumentTemplates.Add(t);
                }
                d.ReplaceWords = new List<ReplaceWordModel>();
                foreach (var word in templateGroup.ReplaceWords.OrderBy(x => x.Key))
                {
                    var w = new ReplaceWordModel();
                    w.Key = word.Key;
                    w.Value = word.Value;
                    d.ReplaceWords.Add(w);
                }
            }
            return PartialView("DocumentTemplateGroupList", model);
        }

        [HttpPost]
        public ActionResult SaveDocumentTemplateGroup(DocumentTemplateGroupModel model)
        {
            var token = Request.GetAuthToken();
            var request = new SaveDocumentTemplateGroupRequest();
            request.Token = token;
            request.Id = model.Id;
            request.Name = model.Name;
            var documentTemplates = new List<SaveDocumentTemplateGroupRequest.DocumentTemplateValue>();
            if (model.DocumentTemplates != null)
            {
                foreach (var documentTemplate in model.DocumentTemplates)
                {
                    var docTemplate = new SaveDocumentTemplateGroupRequest.DocumentTemplateValue();
                    docTemplate.Name = documentTemplate.Name;
                    docTemplate.FileName = documentTemplate.FileName;
                    docTemplate.DownloadFileName = documentTemplate.DownloadFileName;
                    documentTemplates.Add(docTemplate);
                }
            }
            request.DocumentTemplates = documentTemplates.ToArray();

            var replaceWords = new List<SaveDocumentTemplateGroupRequest.ReplaceWordValue>();
            if (model.ReplaceWords != null)
            {
                foreach (var word in model.ReplaceWords)
                {
                    var replaceWord = new SaveDocumentTemplateGroupRequest.ReplaceWordValue();
                    replaceWord.Key = word.Key;
                    replaceWord.Value = word.Value;
                    replaceWords.Add(replaceWord);
                }
            }
            request.ReplaceWords = replaceWords.ToArray();
            var response = DocumentTemplateGroupWorker.SaveDocumentTemplateGroup(request, Request);
            if (response.Type != ResponseType.Success)
            {
                return PartialView("MyError", new ErrorModel { Message = response.ResponseMessage });
            }

            return Json(new { success = true, id = response.Body.DocumentTemplateGroupId });
        }

        [HttpPost]
        public ActionResult DeleteDocumentTemplateGroup(int documentTemplateGroupId)
        {
            var token = Request.GetAuthToken();
            var request = new DeleteDocumentTemplateGroupRequest();
            request.Token = token;
            request.DocumentTemplateGroupId = documentTemplateGroupId;
            var response = DocumentTemplateGroupWorker.DeleteDocumentTemplateGroup(request, Request);
            if (response.Type != ResponseType.Success)
            {
                return Json(new { success = true, message = response.Message });
            }

            return Json(new { success = true });
        }

        [HttpGet]
        public FileResult DownloadOriginal(string fileName, string downloadFileName)
        {
            var path = Path.Combine(ConfigurationManager.AppSettings["FilesStoragePath"], fileName);
            var contentType = GetContentType(fileName);
            return File(path, contentType, downloadFileName);
        }

        [HttpGet]
        public FileResult DownloadPrepared(string fileName, int groupId, bool pdf = false)
        {
            var token = Request.GetAuthToken();
            var request = new GetDocumentTemplateGroupsRequest();
            request.Token = token;
            request.DocumentTemplateGroupId = groupId;
            var response = DocumentTemplateGroupWorker.GetDocumentTemplateGroups(request, Request);
            var docTemplateGroup = response.Body.DocumentTemplateGroups.First();
            var contentType = GetContentType(fileName);
            var preparedPath = Path.Combine(ConfigurationManager.AppSettings["FilesStoragePath"], "Prepared");
            if (!Directory.Exists(preparedPath))
            {
                Directory.CreateDirectory(preparedPath);
            }

            var newFileNamePrefix = Guid.NewGuid() + "_";
            var newFile = Path.Combine(ConfigurationManager.AppSettings["FilesStoragePath"], "Prepared", newFileNamePrefix + "1_" + fileName);
            var newFileForReturn = Path.Combine(ConfigurationManager.AppSettings["FilesStoragePath"], "Prepared", newFileNamePrefix + "2_" + fileName);
            var path = Path.Combine(ConfigurationManager.AppSettings["FilesStoragePath"], fileName);
            var docTemplate = docTemplateGroup.DocumentTemplates.First(x => x.FileName == fileName);
            var downloadFileName = docTemplate.DownloadFileName;
            var replaceWords = docTemplateGroup.ReplaceWords.ToDictionary(x => x.Key, x => x.Value);
            foreach (var word in docTemplateGroup.ReplaceWords)
            {
                downloadFileName = downloadFileName.Replace("<" + word.Key + ">", word.Value);
            }

            var fileType = FileController.GetFileType(fileName);

            if (fileType == FileTypes.Word)
            {
                WordReplacer.Execute(path, newFile, replaceWords);
            }
            else if (fileType == FileTypes.Excel)
            {
                ExcelReplacer.Execute(path, newFile, replaceWords);
            }
            else
            {
                return File(fileName, contentType, downloadFileName);
            }

            if (pdf)
            {
                var newFilePdf = newFile + ".pdf";
                if (fileType == FileTypes.Word)
                {
                    // interop ещё поколдовать над порегать для iis, думаю там траблы, что приложение х32, а винда х64
                    //var app = new Microsoft.Office.Interop.Word.Application();
                    //var wkb = app.Documents.Open(newFile);
                    //wkb.ExportAsFixedFormat(newFilePdf, Microsoft.Office.Interop.Word.WdExportFormat.wdExportFormatPDF);

                    var document = new Spire.Doc.Document();
                    document.LoadFromFile(newFile);
                    document.SaveToFile(newFilePdf, Spire.Doc.FileFormat.PDF);

                    downloadFileName = downloadFileName.Replace(".docx", ".pdf");
                }
                if (fileType == FileTypes.Excel)
                {
                    // interop ещё поколдовать над порегать для iis, думаю там траблы, что приложение х32, а винда х64
                    //var app = new Microsoft.Office.Interop.Excel.Application();
                    //var wkb = app.Workbooks.Open(newFile);
                    //wkb.ExportAsFixedFormat(Microsoft.Office.Interop.Excel.XlFixedFormatType.xlTypePDF, newFilePdf);

                    var  workbook = new Spire.Xls.Workbook();
                    workbook.LoadFromFile(newFile);
                    workbook.SaveToFile(newFilePdf, Spire.Xls.FileFormat.PDF);

                    downloadFileName = downloadFileName.Replace(".xlsx", ".pdf");
                }

                newFile = newFilePdf;
            }

            // подпорка, сразу почему то бракованный фаил возвращает
            System.IO.File.Copy(newFile, newFileForReturn);
            return File(newFileForReturn, contentType, downloadFileName);
        }

        private static string GetContentType(string fileName)
        {
            var fileExt = Path.GetExtension(fileName);
            var contentType = "";
            if (fileExt == ".doc")
            {
                contentType = "application/msword";
            }
            else if (fileExt == ".docx")
            {
                contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
            }
            else if (fileExt == ".xls")
            {
                contentType = "application/vnd.ms-excel";
            }
            else if (fileExt == ".xlsx")
            {
                contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            }
            return contentType;
        }
    }
}
