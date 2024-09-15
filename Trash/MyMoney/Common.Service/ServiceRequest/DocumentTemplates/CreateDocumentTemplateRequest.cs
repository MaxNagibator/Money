using System.Runtime.Serialization;
using Common.Enums;

namespace ServiceRequest.DocumentTemplateGroups
{
    [DataContract]
    public class SaveDocumentTemplateGroupRequest : TokenRequest
    {
        [DataMember]
        public int? Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public DocumentTemplateValue[] DocumentTemplates { get; set; }

        [DataMember]
        public ReplaceWordValue[] ReplaceWords { get; set; }

        [DataContract]
        public class DocumentTemplateValue
        {
            [DataMember]
            public string Name { get; set; }

            [DataMember]
            public string DownloadFileName { get; set; }

            [DataMember]
            public string FileName { get; set; }
        }

        [DataContract]
        public class ReplaceWordValue
        {
            [DataMember]
            public string Key { get; set; }

            [DataMember]
            public string Value { get; set; }
        }
    }
}
