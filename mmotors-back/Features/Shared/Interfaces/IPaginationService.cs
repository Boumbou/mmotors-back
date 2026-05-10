/*
    * this file define the interface for the pagination service
    * it contains the following method:
    * Task<PagedResults<T>> PaginateAsync<T>(IQueryable<T> query, PaginationParams paginationParams)
    * it is used to paginate the results of the get all methods in the repositories
    * it takes an IQueryable<T> query and a PaginationParams object as parameters and returns a PagedResults<T> object
*/
using mmotors_back.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;


namespace mmotors_back.Features.Shared.Interfaces
{
    public interface IPaginationService
    {
        Task<PagedResults<T>> PaginateAsync<T>(IQueryable<T> query, PaginationParams paginationParams);
    }
}