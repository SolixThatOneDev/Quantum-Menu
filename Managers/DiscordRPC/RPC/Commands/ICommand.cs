using Quantum.Managers.DiscordRPC.RPC.Payload;

namespace Quantum.Managers.DiscordRPC.RPC.Commands
{
    internal interface ICommand
    {
        IPayload PreparePayload(long nonce);
    }
}

