using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dochazka.Models
{
    public class PaginatedListViewModel<T> : List<T>
    {
        public int PageIndex { get; private set; }
        public int TotalPages { get; private set; }

        public bool HasPreviousPage
        {
            get
            {
                return PageIndex > 1;
            }
        }

        public bool HasNextPage
        {
            get
            {
                return PageIndex < TotalPages;
            }
        }

        public PaginatedListViewModel(List<T> items, int count, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            AddRange(items);
        }

        public static async Task<PaginatedListViewModel<T>> CreateAsync(IQueryable<T> source, int pageIndex, int pageSize)
        {
            var count = await source.CountAsync();
            var items = await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PaginatedListViewModel<T>(items, count, pageIndex, pageSize);
        }

        public static PaginatedListViewModel<T> Create(IQueryable<T> source, int pageIndex, int pageSize)
        {
            var count = source.Count();
            var items = source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            return new PaginatedListViewModel<T>(items, count, pageIndex, pageSize);
        }
    }
}
