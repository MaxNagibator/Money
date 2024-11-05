using Refit;

namespace Money.Api.Constracts.Operations
{
    public class OperationDTOFilter
    {
        [AliasAs("dateFrom")]
        public DateTime? DateFrom { get; set; }

        [AliasAs("dateTo")]
        public DateTime? DateTo { get; set; }

        [AliasAs("categoryIds")]
        [Query(CollectionFormat.Multi)]
        public List<int> CategoryIds { get; set; } = [];

        [AliasAs("comment")]
        public string? Comment { get; set; }

        [AliasAs("place")]
        public string? Place { get; set; }
    }
}