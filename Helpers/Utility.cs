using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FluentNotificationSender.Helpers
{
    public static class Utility
    {
        public static string ConvertFileToBase64(string path)
        {
            Byte[] source = File.ReadAllBytes(path);
            return Convert.ToBase64String(source, 0, source.Length);
        }
    }
}
