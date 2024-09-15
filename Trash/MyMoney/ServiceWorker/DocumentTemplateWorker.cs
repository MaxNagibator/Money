using System;
using System.Collections.Generic;
using System.Web;
using DataWorker;
using DataWorker.Classes;
using ServiceRequest.DocumentTemplateGroups;
using ServiceRespone.DocumentTemplateGroups;
using ServiceResponse;
using ServiceWorker.Executor;

namespace ServiceWorker
{
    public class DocumentTemplateGroupWorker
    {
        public static Response<GetDocumentTemplateGroupsResponse> GetDocumentTemplateGroups(GetDocumentTemplateGroupsRequest request, HttpRequestBase httpRequestBase)
        {
            Func<GetDocumentTemplateGroupsResponse> x =
                () =>
                {
                    var userId = request.GetUserIdByToken();
                    var result = new GetDocumentTemplateGroupsResponse();
                    var documentTemplateGroups = DbDocumentTemplateGroupWorker.GetDocumentTemplateGroups(userId, request.DocumentTemplateGroupId);

                    var resultGroups = new List<GetDocumentTemplateGroupsResponse.DocumentTemplateGroupValue>();
                    foreach (var group in documentTemplateGroups)
                    {
                        var resultGroup = new GetDocumentTemplateGroupsResponse.DocumentTemplateGroupValue();
                        resultGroup.Id = group.Id.Value;
                        resultGroup.Name = group.Name;
                        var resultDocs = new List<GetDocumentTemplateGroupsResponse.DocumentTemplateValue>();
                        foreach (var doc in group.DocumentTemplates)
                        {
                            var resultDoc = new GetDocumentTemplateGroupsResponse.DocumentTemplateValue();
                            resultDoc.Name = doc.Name;
                            resultDoc.DownloadFileName = doc.DownloadFileName;
                            resultDoc.FileName = doc.FileName;
                            resultDocs.Add(resultDoc);
                        }
                        resultGroup.DocumentTemplates = resultDocs;

                        var resultWords = new List<GetDocumentTemplateGroupsResponse.ReplaceWordValue>();
                        foreach (var word in group.ReplaceWords)
                        {
                            var resultWord = new GetDocumentTemplateGroupsResponse.ReplaceWordValue();
                            resultWord.Key = word.Key;
                            resultWord.Value = word.Value;
                            resultWords.Add(resultWord);
                        };
                        resultGroup.ReplaceWords = resultWords;
                        resultGroups.Add(resultGroup);
                    }

                    result.DocumentTemplateGroups = resultGroups;
                    return result;
                };
            return ServiceExecutor.Execute(x, httpRequestBase, request, ServiceNames.DocumentTemplateGroupService.GetDocumentTemplateGroups);
        }

        public static Response<SaveDocumentTemplateGroupResponse> SaveDocumentTemplateGroup(SaveDocumentTemplateGroupRequest request, HttpRequestBase httpRequestBase)
        {
            Func<SaveDocumentTemplateGroupResponse> x =
                () =>
                {
                    var userId = request.GetUserIdByToken();
                    var result = new SaveDocumentTemplateGroupResponse();
                    var documentTemplateGroup = new DocumentTemplateGroup();
                    documentTemplateGroup.Id = request.Id;
                    documentTemplateGroup.Name = request.Name;
                    var documentTemplates = new List<DocumentTemplate>();
                    foreach (var doc in request.DocumentTemplates)
                    {
                        var resultDoc = new DocumentTemplate();
                        resultDoc.Name = doc.Name;
                        resultDoc.DownloadFileName = doc.DownloadFileName;
                        resultDoc.FileName = doc.FileName;
                        documentTemplates.Add(resultDoc);
                    }
                    documentTemplateGroup.DocumentTemplates = documentTemplates;

                    var words = new Dictionary<string, string>();
                    foreach (var word in request.ReplaceWords)
                    {
                        words.Add(word.Key, word.Value);
                    };
                    documentTemplateGroup.ReplaceWords = words;

                    var id = DbDocumentTemplateGroupWorker.SaveDocumentTemplateGroup(userId, documentTemplateGroup);
                    result.DocumentTemplateGroupId = id;
                    return result;
                };
            return ServiceExecutor.Execute(x, httpRequestBase, request, ServiceNames.DocumentTemplateGroupService.SaveDocumentTemplateGroup);
        }

        public static Response<DeleteDocumentTemplateGroupResponse> DeleteDocumentTemplateGroup(DeleteDocumentTemplateGroupRequest request, HttpRequestBase httpRequestBase)
        {
            Func<DeleteDocumentTemplateGroupResponse> x =
                () =>
                {
                    var userId = request.GetUserIdByToken();
                    var result = new DeleteDocumentTemplateGroupResponse();
                    var DocumentTemplateGroupId = request.DocumentTemplateGroupId;
                    DbDocumentTemplateGroupWorker.DeleteDocumentTemplateGroup(userId, DocumentTemplateGroupId);
                    return result;
                };
            return ServiceExecutor.Execute(x, httpRequestBase, request, ServiceNames.DocumentTemplateGroupService.DeleteDocumentTemplateGroup);
        }
    }
}