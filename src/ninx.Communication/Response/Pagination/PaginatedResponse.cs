namespace ninx.Communication
{
    public class PaginatedResponse<T>
    {
        public List<T> Data { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalRecords / (double)PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
        public int? TotalAtivos { get; set; } = 0;
        public int? TotalNormal { get; set; } = 0;
        public int? TotalBaixo { get; set; } = 0;
        public int? TotalZerado { get; set; } = 0;

        public PaginatedResponse()
        {
            Data = new List<T>();
        }

        public PaginatedResponse(
            List<T> data,
            int pageNumber,
            int pageSize,
            int totalRecords,
            int? totalAtivos = 0,
            int? totalNormal = 0,
            int? totalBaixo = 0,
            int? totalZerado = 0)
        {
            Data = data;
            PageNumber = pageNumber;
            PageSize = pageSize;
            TotalRecords = totalRecords;
            TotalAtivos = totalAtivos;
            TotalNormal = totalNormal;
            TotalBaixo = totalBaixo;
            TotalZerado = totalZerado;
        }
    }
}