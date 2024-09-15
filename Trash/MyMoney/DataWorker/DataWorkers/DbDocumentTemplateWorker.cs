using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Common.Enums;
using Common.Exceptions;
using DataWorker.Classes;
using Newtonsoft.Json;

namespace DataWorker
{
    public class DbDocumentTemplateGroupWorker
    {
        public static List<DocumentTemplateGroup> GetDocumentTemplateGroups(int userId, int? documentTemplateGroupId = null)
        {
            using (var context = new Data.DataContext())
            {
                var dbDocumentTemplateGroups = context.DocumentTemplateGroups.Where(x => x.UserId == userId);
                if (documentTemplateGroupId != null)
                {
                    dbDocumentTemplateGroups.Where(x => x.DocumentTemplateGroupId == documentTemplateGroupId.Value);
                }

                var documentTemplateGroupIds = dbDocumentTemplateGroups.Select(x => x.DocumentTemplateGroupId);
                var dbDocumentTemplateList = context.DocumentTemplates.Where(x => documentTemplateGroupIds.Contains(x.DocumentTemplateGroupId)).ToList();

                var dbDocumentTemplateGroupList = dbDocumentTemplateGroups.OrderBy(x => x.Id).ToList();
                var documentTemplateGroups = new List<DocumentTemplateGroup>();
                foreach (var dbDocumentTemplateGroup in dbDocumentTemplateGroupList)
                {
                    var documentTemplateGroup = new DocumentTemplateGroup();
                    documentTemplateGroup.Id = dbDocumentTemplateGroup.DocumentTemplateGroupId;
                    documentTemplateGroup.Name = dbDocumentTemplateGroup.Name;
                    var documentTemplates = new List<DocumentTemplate>();
                    var dbDocumentTemplates = dbDocumentTemplateList.Where(x => x.DocumentTemplateGroupId == dbDocumentTemplateGroup.DocumentTemplateGroupId).ToList();
                    foreach (var dbDocumentTemplate in dbDocumentTemplates)
                    {
                        var documentTemplate = new DocumentTemplate();
                        documentTemplate.Name = dbDocumentTemplate.Name;
                        documentTemplate.FileName = dbDocumentTemplate.FileName;
                        documentTemplate.DownloadFileName = dbDocumentTemplate.DownloadFileName;
                        documentTemplates.Add(documentTemplate);
                    }
                    documentTemplateGroup.DocumentTemplates = documentTemplates;
                    documentTemplateGroup.ReplaceWords = JsonConvert.DeserializeObject<Dictionary<string, string>>(dbDocumentTemplateGroup.ReplaceWords);
                    documentTemplateGroups.Add(documentTemplateGroup);
                }

                return documentTemplateGroups;
            }
        }

        public static int SaveDocumentTemplateGroup(int userId, DocumentTemplateGroup documentTemplateGroup)
        {
            using (var context = new Data.DataContext())
            {
                Data.DocumentTemplateGroup dbDocumentTemplateGroup;
                int documentTemplateGroupId;
                if (documentTemplateGroup.Id == null)
                {
                    //todo need optimization in future
                    documentTemplateGroupId = context.DocumentTemplateGroups.Where(x => x.UserId == userId).Select(x => x.DocumentTemplateGroupId).DefaultIfEmpty(0).Max() + 1;

                    dbDocumentTemplateGroup = new Data.DocumentTemplateGroup();
                    dbDocumentTemplateGroup.DocumentTemplateGroupId = documentTemplateGroupId;
                    dbDocumentTemplateGroup.UserId = userId;
                    context.DocumentTemplateGroups.Add(dbDocumentTemplateGroup);

                    foreach (var documentTemplate in documentTemplateGroup.DocumentTemplates)
                    {
                        var dbDocumentTemplate = new Data.DocumentTemplate();
                        dbDocumentTemplate.DocumentTemplateGroupId = documentTemplateGroupId;
                        dbDocumentTemplate.Name = documentTemplate.Name;
                        dbDocumentTemplate.FileName = documentTemplate.FileName;
                        dbDocumentTemplate.DownloadFileName = documentTemplate.DownloadFileName;
                        context.DocumentTemplates.Add(dbDocumentTemplate);
                    }
                }
                else
                {
                    documentTemplateGroupId = documentTemplateGroup.Id.Value;

                    dbDocumentTemplateGroup = context.DocumentTemplateGroups.FirstOrDefault(x => x.UserId == userId && x.DocumentTemplateGroupId == documentTemplateGroup.Id);
                    if (dbDocumentTemplateGroup == null)
                    {
                        throw new MessageException("группа документов не найдена");
                    }

                    var dbDocumentTemplates = context.DocumentTemplates.Where(x => x.DocumentTemplateGroupId == documentTemplateGroupId).ToList();
                    foreach (var documentTemplate in documentTemplateGroup.DocumentTemplates)
                    {
                        var dbDocumentTemplate = dbDocumentTemplates.FirstOrDefault(x => x.FileName == documentTemplate.FileName);
                        if (dbDocumentTemplate == null)
                        {
                            dbDocumentTemplate = new Data.DocumentTemplate();
                            dbDocumentTemplate.DocumentTemplateGroupId = documentTemplateGroupId;
                            dbDocumentTemplate.FileName = documentTemplate.FileName;
                            context.DocumentTemplates.Add(dbDocumentTemplate);
                        }
                        dbDocumentTemplate.Name = documentTemplate.Name;
                        dbDocumentTemplate.DownloadFileName = documentTemplate.DownloadFileName;
                    }
                    var currentFileNames = documentTemplateGroup.DocumentTemplates.Select(x => x.FileName).ToList();
                    var dbDocumentTemplatesForDelete = dbDocumentTemplates.Where(x => !currentFileNames.Contains(x.FileName)).ToList();
                    context.DocumentTemplates.RemoveRange(dbDocumentTemplatesForDelete);
                }

                dbDocumentTemplateGroup.Name = documentTemplateGroup.Name;
                dbDocumentTemplateGroup.ReplaceWords = JsonConvert.SerializeObject(documentTemplateGroup.ReplaceWords);
                context.SaveChanges();

                return documentTemplateGroupId;
            }
        }

        public static void DeleteDocumentTemplateGroup(int userId, int documentTemplateGroupId)
        {
            using (var context = new Data.DataContext())
            {
                var dbDocumentTemplateGroup = context.DocumentTemplateGroups.SingleOrDefault(x => x.DocumentTemplateGroupId == documentTemplateGroupId && x.UserId == userId);
                if (dbDocumentTemplateGroup == null)
                {
                    throw new MessageException("DocumentTemplateGroup not found");
                }
                var dbDocumentTemplates = context.DocumentTemplates.Where(x => x.DocumentTemplateGroupId == documentTemplateGroupId);
                context.DocumentTemplates.RemoveRange(dbDocumentTemplates);
                context.DocumentTemplateGroups.Remove(dbDocumentTemplateGroup);
                context.SaveChanges();
            }
        }
    }
}
