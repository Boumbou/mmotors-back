/*
    * this file defines the pagination parameters
    * it is used accross every repository to implement pagination for get all methods
    * it contains the following properties:
    * int pageNumber: the number of the page to retrieve
*/

namespace mmotors_back.Models
{
    public class PaginationParams
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}

