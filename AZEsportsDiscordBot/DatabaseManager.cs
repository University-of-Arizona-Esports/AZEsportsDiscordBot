using System.Threading.Tasks;
using AZEsportsDiscordBot.Framework;
using Microsoft.Extensions.DependencyInjection;

namespace AZEsportsDiscordBot
{
    public class DatabaseManager : ILoadable
    {
        public Task Load(IServiceCollection services)
        {
            return Task.CompletedTask;
        }

        public Task Unload()
        {
            return Task.CompletedTask;
        }
    }
}