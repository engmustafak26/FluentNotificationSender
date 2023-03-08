using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNotificationSender.Interfaces;

namespace FluentNotificationSender.Abstractions
{
    public abstract class Vendor : INotificationMethod
    {
        private protected Vendor()
        {
        }
        internal abstract Task<NotificationResult>[] SendAsync();

        [JsonIgnore]
        internal bool UseAsDefault { get; set; }

        internal bool IsActive { get; set; }

    }
}
