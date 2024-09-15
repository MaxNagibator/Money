using System.Runtime.Serialization;

namespace ServiceRequest.DocumentTemplateGroups
{
    [DataContract]
    public class GetDocumentTemplateGroupsRequest : TokenRequest
    {
        [DataMember]
        public int? DocumentTemplateGroupId { get; set; }
    }
}
