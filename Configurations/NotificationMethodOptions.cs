using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using FluentNotificationSender.Abstractions;
using FluentNotificationSender.Interfaces;

namespace FluentNotificationSender.Configurations
{
    public sealed class NotificationMethodOptions<T> where T : Vendor, INotificationMethod
    {
        public NotificationMethodOptions()
        {
            Vendors = new List<T>();
        }
        public NotificationMethodOptions(T vendor) : this()
        {
            Vendors.Add(vendor);
        }
        public List<T> Vendors { get; private set; }

        [JsonIgnore]
        internal T[] ActiveVendors => Vendors.Where(v => v.IsActive).ToArray();

        internal void SafeAddActiveVendor(T vendor)
        {
            vendor.IsActive = true;

            if (Vendors.IndexOf(vendor) >= 0)
                return;

            Vendors.Add(vendor);
        }

    }
}
