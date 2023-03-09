using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNotificationSender.Configurations;
using FluentNotificationSender.Interfaces;

namespace FluentNotificationSender.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddFluentNotificationSender(this IServiceCollection services, Action<NotificationOptions> configure)
        {
            NotificationOptions options = new NotificationOptions();
            configure(options);

            var descriptor = services.FirstOrDefault(s => s.ServiceType == typeof(Func<NotificationOptions, IFluentNotificationService>));
            if (descriptor != null) services.Remove(descriptor);
            services.AddSingleton((Func<NotificationOptions, IFluentNotificationService>)(options => new FluentNotificationService(options)));
            services.AddSingleton(typeof(IFluentNotificationService), services => services.GetRequiredService<Func<NotificationOptions, IFluentNotificationService>>()(options));

            return services;
        }
    }
}
