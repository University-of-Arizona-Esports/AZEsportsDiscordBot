# AZEsportsDiscordBot
The official repo for the EZ Esports Discord bot.

### Cogs
Cogs are a modular component that allow features to be loaded and unloaded at runtime.  
Creating a cog is as simple as extending the Cog class.
```c#
// This cog adds an event listener and announces a user join in the guild's default channel
public class ExampleCog : Cog
{
    // In the load method we set up anything needed for this cog to function.
    // The Cog class provides access to the discord client.
    public override void OnCogLoad()
    {
        Discord.UserJoined += HandleUserJoin;
    }

    // When the cog is unloaded, it must do its best to cleanup all traces of itself.
    // If an outside thread has a method from this cog's assembly on the stack, it will
    // prevent the assembly from being unloaded.
    public override void OnCogUnload()
    {
        Discord.UserJoined -= HandleUserJoin;
    }

    // Send a join message to the default channel
    private async Task HandleUserJoin(SocketGuildUser user)
    {
        await user.Guild.DefaultChannel.SendMessageAsync($"{user.Mention} joined the server.");
    }
}
```
When a cog is unloaded, there is a possibility that an updated version will be loaded in its place.
For this reason, the cog should try and make sure that it is eligible for garbage collection when an unload is requested.
See [How to use and debug assembly unloadability in .NET Core](https://docs.microsoft.com/en-us/dotnet/standard/assembly/unloadability)
for the full info on the conditions for an assembly to unload.
