using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using MikaWeb.Models.AuxiliaryModels;

namespace MikaWeb.Extensions.DB
{
    public static class LinqExtensions
    {

        public static DataTable CopyToDataTable<T>(this IEnumerable<T> source)
        {
            return new ObjectShredder<T>().Shred(source, null, null);
        }

        public static DataTable CopyToDataTable<T>(this IEnumerable<T> source,
                                                    DataTable table, LoadOption? options)
        {
            return new ObjectShredder<T>().Shred(source, table, options);
        }

        public static IQueryable<T> OrderByDynamic<T>(
            this IQueryable<T> query,
            string orderByMember,
            DtOrderDir ascendingDirection)
        {
            var param = Expression.Parameter(typeof(T), "c");

            var body = orderByMember.Split('.').Aggregate<string, Expression>(param, Expression.PropertyOrField);

            var queryable = ascendingDirection == DtOrderDir.Asc ?
                (IOrderedQueryable<T>)Queryable.OrderBy(query.AsQueryable(), (dynamic)Expression.Lambda(body, param)) :
                (IOrderedQueryable<T>)Queryable.OrderByDescending(query.AsQueryable(), (dynamic)Expression.Lambda(body, param));

            return queryable;
        }

        public static IQueryable<T> WhereDynamic<T>(
            this IQueryable<T> sourceList, string query)
        {

            if (string.IsNullOrEmpty(query))
            {
                return sourceList;
            }

            try
            {
                var properties = typeof(T).GetProperties()
                    .Where(x => x.CanRead && x.CanWrite && !x.GetGetMethod().IsVirtual);

                sourceList = sourceList.Where(c =>
                    properties.Any(p => p.GetValue(c) != null && p.GetValue(c).ToString()
                        .Contains(query, StringComparison.InvariantCultureIgnoreCase)));
            }
            catch { }

            return sourceList;
        }
    }
}