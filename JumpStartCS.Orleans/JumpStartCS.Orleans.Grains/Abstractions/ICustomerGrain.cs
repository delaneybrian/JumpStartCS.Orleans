namespace JumpStartCS.Orleans.Grains.Abstractions
{
    public interface ICustomerGrain : IGrainWithGuidKey
    {
        Task AddCheckingAccount(Guid checkingAccountId);

        Task<decimal> GetNetWorth();
    }
}
