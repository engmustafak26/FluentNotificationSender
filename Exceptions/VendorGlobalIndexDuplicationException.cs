using System;
using System.Collections.Generic;
using System.Text;

namespace FluentNotificationSender.Exceptions
{
    public sealed class VendorGlobalIndexDuplicationException : Exception
    {
        public VendorGlobalIndexDuplicationException(string message):base(message) { }
      
    }
}
