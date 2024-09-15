using System.Collections.Generic;
using System.Runtime.Serialization;
using Common.Enums;

namespace ServiceRespone.DocumentTemplateGroups
{
    [DataContract]
    public class GetDocumentTemplateGroupsResponse
    {
        [DataMember]
        public List<DocumentTemplateGroupValue> DocumentTemplateGroups { get; set; }

        [DataContract]
        public class DocumentTemplateGroupValue
        {
            [DataMember]
            public int Id { get; set; }

            [DataMember]
            public string Name { get; set; }

            [DataMember]
            public List<DocumentTemplateValue> DocumentTemplates { get; set; }

            [DataMember]
            public List<ReplaceWordValue> ReplaceWords { get; set; }
        }

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
