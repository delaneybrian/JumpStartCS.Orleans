using JumpStartCS.Orleans.Grains.Abstractions;
using JumpStartCS.Orleans.Grains.State;
using Orleans.Concurrency;
using Orleans.Runtime;

namespace JumpStartCS.Orleans.Grains.Grains
{
    [StatelessWorker]
    public class StatlessTransferProcessingGrain : Grain, IStatlessTransferProcessingGrain
    {
        private readonly ITransactionClient _transactionClient;
        private readonly IPersistentState<TransferState> _transferState;

        public StatlessTransferProcessingGrain(
           ITransactionClient transactionClient,
           [PersistentState("transfer", "tableStorage")] IPersistentState <TransferState> transferState)
        {
            _transactionClient = transactionClient;
            _transferState = transferState;
        }

        public async Task ProcessTransfer(Guid fromAccountId, Guid toAccountId, decimal amount)
        {
            var fromAccountGrain = GrainFactory.GetGrain<ICheckingAccountGrain>(fromAccountId);
            var toAccountGrain = GrainFactory.GetGrain<ICheckingAccountGrain>(toAccountId);

            await _transactionClient.RunTransaction(TransactionOption.Create, async () =>
            {
                await toAccountGrain.Credit(amount);
                await fromAccountGrain.Debit(amount);
            });

            _transferState.State.TransferCount += 1;

            await _transferState.WriteStateAsync();
        }
    }
}
