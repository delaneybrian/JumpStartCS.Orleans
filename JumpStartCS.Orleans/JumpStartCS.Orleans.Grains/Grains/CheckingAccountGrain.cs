using JumpStartCS.Orleans.Grains.Abstractions;
using JumpStartCS.Orleans.Grains.Events;
using JumpStartCS.Orleans.Grains.State;
using Orleans.Concurrency;
using Orleans.Runtime;
using Orleans.Transactions.Abstractions;

namespace JumpStartCS.Orleans.Grains.Grains
{
    [Reentrant]
    public class CheckingAccountGrain : Grain, ICheckingAccountGrain, IRemindable
    {
        private readonly ITransactionClient _transactionClient;
        private readonly ITransactionalState<BalanceState> _balanceTransactionalState;
        private readonly IPersistentState<CheckingAccountState> _checkingAccountState;

        public CheckingAccountGrain(
            ITransactionClient transactionClient,
            [TransactionalState("balance")] ITransactionalState<BalanceState> balanceTransactionalState,
            [PersistentState("checkingAccount", "blobStorage")] IPersistentState<CheckingAccountState> checkingAccountState)
        {
            _transactionClient = transactionClient;
            _balanceTransactionalState = balanceTransactionalState;
            _checkingAccountState = checkingAccountState;
        }

        public async Task AddReccuringPayment(Guid id, decimal amount, int reccursEveryMinutes)
        {
            _checkingAccountState.State.RecurringPayments.Add(new RecurringPayment
            {
                PaymentId = id,
                PaymentAmount = amount,
                OccursEveryMinutes = reccursEveryMinutes
            });

            await _checkingAccountState.WriteStateAsync();

            await this.RegisterOrUpdateReminder($"RecurringPayment:::{id}",
                TimeSpan.FromMinutes(reccursEveryMinutes), TimeSpan.FromMinutes(reccursEveryMinutes));
        }

        public async Task ReceiveReminder(string reminderName, TickStatus status)
        {
            if (reminderName.StartsWith("RecurringPayment"))
            {
                var reccuringPaymentId = Guid.Parse(reminderName.Split(":::").Last());

                var reccuringPayment = _checkingAccountState.State.RecurringPayments
                    .Single(x => x.PaymentId == reccuringPaymentId);

                await _transactionClient.RunTransaction(TransactionOption.Create, async () =>
                {
                    await Debit(reccuringPayment.PaymentAmount);
                });
            }
        }

        public async Task Credit(decimal amount)
        {
            await _balanceTransactionalState.PerformUpdate(state =>
            {
                var currentBalance = state.Balance;

                var newBalance = currentBalance + amount;

                state.Balance = newBalance;
            });

            var streamProvider = this.GetStreamProvider("StreamProvider");

            var streamId = StreamId.Create("BalanceStream", this.GetGrainId().GetGuidKey());

            var stream = streamProvider.GetStream<BalanceChangeEvent>(streamId);

            await stream.OnNextAsync(new BalanceChangeEvent()
            {
                CheckingAccountId = this.GetGrainId().GetGuidKey(),
                Balance = await GetBalance()
            });
        }

        public async Task Debit(decimal amount)
        {
            await _balanceTransactionalState.PerformUpdate(state =>
            {
                var currentBalance = state.Balance;

                var newBalance = currentBalance - amount;

                state.Balance = newBalance;
            });

            var streamProvider = this.GetStreamProvider("StreamProvider");

            var streamId = StreamId.Create("BalanceStream", this.GetGrainId().GetGuidKey());

            var stream = streamProvider.GetStream<BalanceChangeEvent>(streamId);

            await stream.OnNextAsync(new BalanceChangeEvent()
            {
                CheckingAccountId = this.GetGrainId().GetGuidKey(),
                Balance = await GetBalance()
            });
        }

        public async Task<decimal> GetBalance()
        {
            return await _balanceTransactionalState.PerformRead(state => state.Balance);
        }

        public async Task Initialise(decimal openingBalance)
        {
            _checkingAccountState.State.OpenedAtUtc = DateTime.UtcNow;

            _checkingAccountState.State.AccountType = "Default";

            _checkingAccountState.State.AccountId = this.GetGrainId().GetGuidKey();

            await _balanceTransactionalState.PerformUpdate(state =>
            { 
                state.Balance = openingBalance;
            });

            await _checkingAccountState.WriteStateAsync();
        }

        public async Task FireAndForgetWork()
        {
            await Task.Delay(TimeSpan.FromSeconds(2));

            throw new NotSupportedException("This work cannot be done");
        }

        public async Task CancelableWork(GrainCancellationToken grainCancellationToken, long workDurationSeconds)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(workDurationSeconds), grainCancellationToken.CancellationToken);
            }
            catch (TaskCanceledException _)
            {
                return;
            }
        }
    }
}
