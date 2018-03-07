using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileTracking.Services.Middlewares
{
    public static class WebSocketConnectionManagerMiddlewareExtensions
    {
        public static IApplicationBuilder UseWebSocketManager(this IApplicationBuilder builder)
        {
            var webSocketOptions = new WebSocketOptions()
            {
                KeepAliveInterval = TimeSpan.FromSeconds(20),
                ReceiveBufferSize = 4 * 1024,
            };
            builder.UseWebSockets(webSocketOptions);
            return builder.UseMiddleware<WebSocketConnectionManagerMiddleware>();
        }
    }
}
