/*
    * this file defines the paged results class
    * it is used to return paged results from the repositories
    * it contains the following properties:
    * IEnumerable<T> Items: the items of the current page
    * int TotalCount: the total count of items in the database
    * int PageNumber: the number of the current page
    * int PageSize: the size of the page
    * int TotalPages: the total number of pages
*/

namespace mmotors_back.Models
{
    public class PagedResults<T>
    {
        public IEnumerable<T> Items { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    }
}