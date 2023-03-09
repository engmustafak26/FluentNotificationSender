using EllipticCurve.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FluentNotificationSender.Extensions
{
    internal static class ArrayExtensions
    {
        public static void ForEach<T>(this T[] source, Action<T> action)
        {
            Array.ForEach(source, action);

        }

        public static Stream ToStream(this byte[] source)
        {
            return new MemoryStream(source);
        }

        public static string ToBase64(this byte[] source)
        {
            return Convert.ToBase64String(source, 0, source.Length);
        }
    }
}
