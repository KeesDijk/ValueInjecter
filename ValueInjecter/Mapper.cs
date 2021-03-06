﻿using System;
using System.Collections.Concurrent;

namespace Omu.ValueInjecter
{
    public class Mapper
    {
        public static ConcurrentDictionary<Tuple<Type, Type>, Tuple<object, bool>> Maps = new ConcurrentDictionary<Tuple<Type, Type>, Tuple<object, bool>>();

        public static Func<object, Type, object, object> DefaultMap = (source, type, tag) => Activator.CreateInstance(type).InjectFrom(source);

        public static TResult MapDefault<TResult>(object source, object tag = null)
        {
            return (TResult)DefaultMap(source, typeof(TResult), tag);
        }

        public static TResult Map<TResult>(object source, object tag = null)
        {
            Tuple<object, bool> funct;
            var sourceType = source.GetType();

            Maps.TryGetValue(new Tuple<Type, Type>(sourceType, typeof(TResult)), out funct);

            if (funct != null)
            {
                var prms = funct.Item2 ? new[] { source, tag } : new[] { source };
                return (TResult)funct.Item1.GetType().GetMethod("Invoke").Invoke(funct.Item1, prms);
            }

            return (TResult)DefaultMap(source, typeof(TResult), tag);
        }

        public static TResult Map<TSource, TResult>(TSource source, object tag = null)
        {
            Tuple<object, bool> funct;
            Maps.TryGetValue(new Tuple<Type, Type>(typeof(TSource), typeof(TResult)), out funct);

            if (funct != null)
            {
                if (funct.Item2)
                    return ((Func<TSource, object, TResult>)funct.Item1)(source, tag);
                return ((Func<TSource, TResult>)funct.Item1)(source);
            }

            return (TResult)DefaultMap(source, typeof(TResult), tag);
        }

        public static void AddMap<TSource, TResult>(Func<TSource, TResult> func)
        {
            Maps.AddOrUpdate(new Tuple<Type, Type>(typeof(TSource), typeof(TResult)), new Tuple<object, bool>(func, false), (key, oldValue) => new Tuple<object, bool>(func, false));
        }

        public static void AddMap<TSource, TResult>(Func<TSource, object, TResult> func)
        {
            Maps.AddOrUpdate(new Tuple<Type, Type>(typeof(TSource), typeof(TResult)), new Tuple<object, bool>(func, true), (key, oldValue) => new Tuple<object, bool>(func, true));
        }

        public static void Reset()
        {
            Maps.Clear();
            DefaultMap = (source, type, tag) => Activator.CreateInstance(type).InjectFrom(source);
        }
    }
}