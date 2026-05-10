namespace PublicApi.Helpers
{
    public class PagedList<T>
    {
        public PagedList(IReadOnlyList<T> items, int count, int pageNumber, int pageSize)
        {
            Items = items;
            CurrentPage = pageNumber;
            PageSize = pageSize;
            TotalCount = count;
            TotalPages = pageSize > 0 ? (int)Math.Ceiling(count / (double)pageSize) : 0;
        }

        public IReadOnlyList<T> Items { get; }
        public int CurrentPage { get; }
        public int TotalPages { get; }
        public int PageSize { get; }
        public int TotalCount { get; }

        public static PagedList<T> Create(IEnumerable<T> source, int pageNumber, int pageSize)
        {
            var snapshot = source as IReadOnlyCollection<T> ?? source.ToList();
            var items = snapshot.Skip(pageNumber * pageSize).Take(pageSize).ToList();
            return new PagedList<T>(items, snapshot.Count, pageNumber, pageSize);
        }
    }
}
