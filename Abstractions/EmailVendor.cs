﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNotificationSender.Emails;
using FluentNotificationSender.Interfaces;

namespace FluentNotificationSender.Abstractions
{
    public abstract class EmailVendor : Vendor<EmailMessage>, IEmailNotificationMethod
    {
        private protected EmailVendor()
        {
        }

        internal abstract override Task<FluentNotificationResult>[] SendAsync();



    }
}
