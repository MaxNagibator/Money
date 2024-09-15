using System.Runtime.Serialization;

namespace ServiceRequest.DocumentTemplateGroups
{
    [DataContract]
    public class DeleteDocumentTemplateGroupRequest : TokenRequest
    {
        [DataMember]
        public int DocumentTemplateGroupId { get; set; }
    }
}
