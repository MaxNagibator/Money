using System.Collections.Generic;

namespace DataWorker.Classes
{
    public class DocumentTemplateGroup
    {
        public int? Id { get; set; }

        public string Name { get; set; }

        public List<DocumentTemplate> DocumentTemplates { get; set; }

        public Dictionary<string, string> ReplaceWords { get; set; }
    }
}
