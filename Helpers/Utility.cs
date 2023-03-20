using System;
using System.Net;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Net.Mail;

namespace FluentNotificationSender.Helpers
{
    public static class Utility
    {
        private static Type[] types = null;
        public static string ConvertFileToBase64(string path)
        {
            Byte[] source = File.ReadAllBytes(path);
            return Convert.ToBase64String(source, 0, source.Length);
        }

        public static Type GetType(string typeString)
        {
            if (types == null)
            {
                types = typeof(int).Assembly.GetTypes()
                                   .Union(typeof(SmtpClient).Assembly.GetTypes())
                                   .Union(System.AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes()))
                                   .ToArray();
            }

            var matchedType = types.FirstOrDefault(x => string.Compare(x.FullName, typeString, StringComparison.OrdinalIgnoreCase) == 0);
            return matchedType;

        }
    }

}
