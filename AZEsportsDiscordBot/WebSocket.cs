using System.Threading.Tasks;
using AZEsportsDiscordBot.Framework;
using Microsoft.Extensions.DependencyInjection;
using WebSocketSharp.Server;

namespace AZEsportsDiscordBot
{
    public class WebSocket : ILoadable
    {
        private WebSocketServer _server;

        public Task Load(IServiceCollection services)
        {
            // TODO load port from database/config
            _server = new WebSocketServer(15560);
            
            return Task.CompletedTask;
        }

        public Task Unload()
        {
            return Task.CompletedTask;
        }
    }
}