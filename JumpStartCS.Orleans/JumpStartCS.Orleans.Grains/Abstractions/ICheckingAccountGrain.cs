using Orleans.Concurrency;

namespace JumpStartCS.Orleans.Grains.Abstractions
{
    public interface ICheckingAccountGrain : IGrainWithGuidKey
    {
        [Transaction(TransactionOption.Create)]
        Task Initialise(decimal openingBalance);

        [Transaction(TransactionOption.Create)]
        Task<decimal> GetBalance();

        [Transaction(TransactionOption.CreateOrJoin)]
        Task Debit(decimal amount);

        [Transaction(TransactionOption.CreateOrJoin)]
        Task Credit(decimal amount);

        Task AddReccuringPayment(Guid id, decimal amount, int reccursEveryMinutes);

        Task CancelableWork(GrainCancellationToken grainCancellationToken, long workDurationSeconds);

        [OneWay]
        Task FireAndForgetWork();
    }
}
