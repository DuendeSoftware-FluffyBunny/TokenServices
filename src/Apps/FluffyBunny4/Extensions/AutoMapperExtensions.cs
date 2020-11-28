using AutoMapper;
using System;
using System.Linq.Expressions;

namespace FluffyBunny4.Extensions
{
    public static class AutoMapperExtensions
    {
        public static IMappingExpression<TSource, TDestination>
            Ignore<TSource, TDestination>(this IMappingExpression<TSource, TDestination> map,
                                          Expression<Func<TDestination, object>> selector)
        {
            map.ForMember(selector, config => config.Ignore());
            return map;
        }
    }
}
