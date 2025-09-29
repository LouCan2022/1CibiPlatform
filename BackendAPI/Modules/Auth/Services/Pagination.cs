namespace Auth.Services;

public class Pagination : IPagination
{
    public (int iPage, int totalPages, double iPageSize, int totalRecords) CalculateTotalRecordsAndPages<T>(IQueryable<T> query, int page, int pageSize)
    {
        // Checking page and make sure that page will not go for 0 because it would produce error
        var currentPage = page < 1 ? 1 : page;

        // Checking pageSize and make sure that pageSize will not go for 0 because it would produce error
        var currentPageSize = pageSize < 1 ? 1 : pageSize;

        // Count the total records of Data
        var totalRecords = query.Count();

        // To produce Total Pages
        var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

        // Making sure that Page will not go outside of totalpages
        if (currentPage > totalPages)
        {
            currentPage = totalPages;
        }

        return (currentPage, totalPages, currentPageSize, totalRecords);
    }
}
