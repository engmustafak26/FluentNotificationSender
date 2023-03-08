using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using FluentNotificationSender.Helpers;

namespace FluentNotificationSender.Extensions
{
    internal static class ObjectExtensions
    {
        static JsonSerializerSettings serializerSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.All,
            ContractResolver = new JsonBindNonPublicPropertiesContractResolver()
        };

        public static T DeepClone<T>(this T source)
        {

            var sourceString = JsonConvert.SerializeObject(source, serializerSettings);
            return JsonConvert.DeserializeObject<T>(sourceString, serializerSettings);
        }
    }
}
