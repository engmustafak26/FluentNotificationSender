using FirebaseAdmin.Messaging;
using FluentNotificationSender.Interfaces;
using FluentNotificationSender.MobileNotifications;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FluentNotificationSender.Abstractions
{
    public abstract class MobilePushNotificationVendor : Vendor<MobileNotificationMessage>, IMobileNotificationMethod
    {
        private protected MobilePushNotificationVendor()
        {
        }

        internal abstract override Task<FluentNotificationResult>[] SendAsync();
    

    }
}
