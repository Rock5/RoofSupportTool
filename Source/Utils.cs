using System;
using System.Linq.Expressions;

namespace RoofSupportTool
{
    public class Utils
    {
        public static Func<TObject, TValue> GetFieldAccessor<TObject, TValue>(string fieldName)
        {
            var param = Expression.Parameter(typeof(TObject), "arg");
            var member = Expression.Field(param, fieldName);
            var lambda = Expression.Lambda(typeof(Func<TObject, TValue>), member, param);
            var compiled = (Func<TObject, TValue>)lambda.Compile();
            return compiled;
        }
    }
}
