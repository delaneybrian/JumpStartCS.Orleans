namespace JumpStartCS.Orleans.Grains.Abstractions
{
    public interface IAtmGrain : IGrainWithGuidKey
    {
        [Transaction(TransactionOption.Create)]
        public Task Initialise(decimal openingBalance);

        [Transaction(TransactionOption.CreateOrJoin)]
        public Task Withdraw(Guid checkingAccountId, decimal amount);

        [Transaction(TransactionOption.Create)]
        Task<decimal> GetBalance();
    }
}
