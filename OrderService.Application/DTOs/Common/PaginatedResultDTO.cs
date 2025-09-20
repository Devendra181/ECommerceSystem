namespace OrderService.Application.DTOs.Common
{
    public class PaginatedResultDTO<T>
    {
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public List<T> Items { get; set; } = new();
    }
}
