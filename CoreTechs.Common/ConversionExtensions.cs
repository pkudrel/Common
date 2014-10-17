﻿using System;
using System.ComponentModel;
using System.Linq;

namespace CoreTechs.Common
{
    public static class ConversionExtensions
    {
        public static void RegisterAllCustomTypeConverters()
        {
            DateTimeOffsetConverter.Register();
            EnumConverter.Register();
        }

        public static T ConvertTo<T>(this object obj)
        {
            var converted = obj.ConvertTo(typeof (T));
            return (T) converted;
        }

        public static object ConvertTo(this object obj, Type targetType)
        {
            if (targetType.IsInstanceOfType(obj))
                return obj;

            if (obj == DBNull.Value)
                obj = null;

            var targetConv = TypeDescriptor.GetConverter(targetType);
            TypeConverter sourceConv = obj != null ? TypeDescriptor.GetConverter(obj.GetType()) : null;

            var attempt2 = Attempt.Get(() => targetConv.ConvertFrom(obj));
            if (attempt2.Succeeded)
                return attempt2.Value;

            Attempt<object> attempt3 = null;
            if (sourceConv != null)
                attempt3 = Attempt.Get(() => sourceConv.ConvertTo(obj, targetType));

            if (attempt3 != null && attempt3.Succeeded)
                return attempt3.Value;

            var attempt4 = Attempt.Get(() => Convert.ChangeType(obj, targetType));
            if (attempt4.Succeeded)
                return attempt4.Value;

            if (targetType == typeof(string) && obj != null)
                return obj.ToString();

            var exceptions = new[] {attempt2, attempt3, attempt4}
                .Where(x => x != null)
                .Select(x => x.Exception).ToArray();

            throw new InvalidCastException("Conversion failed", new AggregateException(exceptions));
        }

  /*      public static object ConvertTo(this object obj, Type targetType)
        {
            if (targetType.IsInstanceOfType(obj))
                return obj;

            if (obj == DBNull.Value)
                obj = null;

            if (obj == null && (!targetType.IsValueType || targetType.IsNullable()))
                return null;

            var converter = TypeDescriptor.GetConverter(targetType);
            
            Exception changeTypeEx, convertEx;
            object result;

            try
            {
                result = converter.ConvertFrom(obj);
                return result;
            }
            catch(Exception ex)
            {
                convertEx = ex;
                // TypeDescriptor converter didn't work
            }

            if (targetType.IsNullable())
                targetType = Nullable.GetUnderlyingType(targetType);

            try
            {
                result = Convert.ChangeType(obj, targetType);
                return result;
            }
            catch(Exception ex)
            {
                changeTypeEx = ex;
                // Convert.ChangeType didn't work
            }

            if (targetType == typeof (string) && obj != null)
                return obj.ToString();

            throw new InvalidCastException("Conversion failed", new AggregateException(changeTypeEx, convertEx));
        }*/
    }
}