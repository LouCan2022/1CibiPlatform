namespace Auth.Services;

public interface IPagination
{
    public (int iPage, int totalPages, double iPageSize, int totalRecords) CalculateTotalRecordsAndPages<T>(
        IQueryable<T> query,
        int page,
        int pageSize);
}
