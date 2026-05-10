/*
    * this file implements the pagination service
    * it contains the following method:
    * Task<PagedResults<T>> PaginateAsync<T>(IQueryable<T> query, PaginationParams paginationParams)
    * it is used to paginate the results of the get all methods
    * it takes an IQueryable<T> query and a PaginationParams object as parameters and returns a PagedResults<T> object
*/

using mmotors_back.Features.Shared.Interfaces;
using mmotors_back.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;


namespace mmotors_back.Features.Shared.Services
{
    public class PaginationService : IPaginationService
    {
        public async Task<PagedResults<T>> PaginateAsync<T>(IQueryable<T> query, PaginationParams paginationParams)
        {
            var totalCount = await query.CountAsync();
            var items = await query.Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                                   .Take(paginationParams.PageSize)
                                   .ToListAsync();

            return new PagedResults<T>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = paginationParams.PageNumber,
                PageSize = paginationParams.PageSize,
            };
        }
    }
}