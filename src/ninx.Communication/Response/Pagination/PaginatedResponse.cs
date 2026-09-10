namespace ninx.Communication
{
    public class PaginatedResponse<TData, TSummary> where TSummary : class
    {
        public IReadOnlyCollection<TData> Data { get; init; } = Array.Empty<TData>();
        public int PageNumber { get; init; }
        public int PageSize { get; init; }
        public int TotalRecords { get; init; }
        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalRecords / (double)PageSize) : 0;
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
        public TSummary? Summary { get; init; }

        public PaginatedResponse(
            IEnumerable<TData> data,
            int pageNumber,
            int pageSize,
            int totalRecords,
            TSummary? summary = null)
        {
            Data = data.ToList().AsReadOnly();
            PageNumber = pageNumber;
            PageSize = pageSize;
            TotalRecords = totalRecords;
            Summary = summary;
        }
    }
    public class PaginatedResponse<TData> : PaginatedResponse<TData, object>
    {
        public PaginatedResponse(IEnumerable<TData> data, int pageNumber, int pageSize, int totalRecords)
            : base(data, pageNumber, pageSize, totalRecords, null) { }
        public PaginatedResponse(IEnumerable<TData> data, int pageNumber, int pageSize, int totalRecords, object? summary)
            : base(data, pageNumber, pageSize, totalRecords, summary) { }
    }

    public class MetricsSummary
    {
        public int TotalGeral { get; set; }
        public int TotalAtivos { get; set; }
        public int TotalInativos => TotalGeral - TotalAtivos;
        public int TotalNormal { get; set; }
        public int TotalBaixo { get; set; }
        public int TotalZerado { get; set; }
    }
}