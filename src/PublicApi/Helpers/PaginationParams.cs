namespace PublicApi.Helpers
{
    public class PaginationParams
    {
        private const int MaxPageSize = 500;
        private int _pageNumber = 0;
        public int PageNumber
        {
            get => _pageNumber;
            set => _pageNumber = value < 0 ? 0 : value;
        }
        private int _pageSize = 10;
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value < 1 ? 1 : (value > MaxPageSize ? MaxPageSize : value);
        }
    }
}
