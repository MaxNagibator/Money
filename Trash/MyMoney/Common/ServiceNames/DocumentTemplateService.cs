using System.ComponentModel;
using Extentions;

namespace ServiceNames
{
    [Description("Группы шаблонов документов")]
    [ServiceName("DocumentTemplateGroup")]
    public enum DocumentTemplateGroupService
    {
        [Description("Получить список групп шаблонов документов")]
        [ServiceName("DocumentTemplateGroup/GetDocumentTemplateGroups")]
        GetDocumentTemplateGroups,
        
        [Description("Создать группу шаблонов документов")]
        [ServiceName("DocumentTemplateGroup/GetDocumentTemplateGroups")]
        SaveDocumentTemplateGroup,
        
        [Description("Удалить группу шаблонов документов")]
        [ServiceName("DocumentTemplateGroup/DeleteDocumentTemplateGroup")]
        DeleteDocumentTemplateGroup,
    }
}