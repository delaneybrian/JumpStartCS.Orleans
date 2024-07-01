namespace JumpStartCS.Orleans.Grains.Abstractions
{
    public interface IStatlessTransferProcessingGrain : IGrainWithIntegerKey
    {
        Task ProcessTransfer(Guid fromAccountId, Guid toAccountId, decimal amount);
    }
}
