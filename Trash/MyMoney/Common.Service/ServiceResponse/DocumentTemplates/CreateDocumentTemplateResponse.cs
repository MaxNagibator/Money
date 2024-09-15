using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ServiceRespone.DocumentTemplateGroups
{
    [DataContract]
    public class SaveDocumentTemplateGroupResponse
    {
        [DataMember]
        public int DocumentTemplateGroupId { get; set; }
    }
}
