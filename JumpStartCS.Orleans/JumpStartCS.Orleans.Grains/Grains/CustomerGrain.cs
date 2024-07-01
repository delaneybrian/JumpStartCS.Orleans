using JumpStartCS.Orleans.Grains.Abstractions;
using JumpStartCS.Orleans.Grains.Events;
using JumpStartCS.Orleans.Grains.State;
using Orleans.Runtime;
using Orleans.Streams;

namespace JumpStartCS.Orleans.Grains.Grains
{
    public class CustomerGrain : Grain, ICustomerGrain, IAsyncObserver<BalanceChangeEvent>
    {
        private readonly IPersistentState<CustomerState> _customerState;

        public CustomerGrain(
            [PersistentState("customer", "tableStorage")] IPersistentState<CustomerState> customerState)
        {
            _customerState = customerState;
        }

        public override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            var streamProvider = this.GetStreamProvider("StreamProvider");

            foreach(var checkingAccountId in _customerState.State.CheckingAccountBalanceById.Keys)
            {
                var streamId = StreamId.Create("BalanceStream", checkingAccountId);

                var stream = streamProvider.GetStream<BalanceChangeEvent>(streamId);

                var handles = await stream.GetAllSubscriptionHandles();

                foreach (var handle in handles)
                {
                    await handle.ResumeAsync(this);
                }
            }
        }

        public async Task AddCheckingAccount(Guid checkingAccountId)
        {
            _customerState.State.CheckingAccountBalanceById.Add(checkingAccountId, 0);

            var streamProvider = this.GetStreamProvider("StreamProvider");

            var streamId = StreamId.Create("BalanceStream", checkingAccountId);

            var stream = streamProvider.GetStream<BalanceChangeEvent>(streamId);

            await stream.SubscribeAsync(this);

            await _customerState.WriteStateAsync();
        }

        public async Task<decimal> GetNetWorth()
        {
            return _customerState.State.CheckingAccountBalanceById.Values.Sum();
        }

        public Task OnCompletedAsync()
        {
            return Task.CompletedTask;
        }

        public Task OnErrorAsync(Exception ex)
        {
            return Task.CompletedTask;
        }

        public async Task OnNextAsync(BalanceChangeEvent item, StreamSequenceToken? token = null)
        {
            var checkingAccountBalancesById = _customerState.State.CheckingAccountBalanceById;

            if (checkingAccountBalancesById.ContainsKey(item.CheckingAccountId))
            {
                checkingAccountBalancesById[item.CheckingAccountId] = item.Balance;
            }
            else
            {
                checkingAccountBalancesById.Add(item.CheckingAccountId, item.Balance);
            }

            await _customerState.WriteStateAsync();
        }
    }
}
