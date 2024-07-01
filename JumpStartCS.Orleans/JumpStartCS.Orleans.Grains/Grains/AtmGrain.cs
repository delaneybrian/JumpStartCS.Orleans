using JumpStartCS.Orleans.Grains.Abstractions;
using JumpStartCS.Orleans.Grains.State;
using Microsoft.Extensions.Logging;
using Orleans.Concurrency;
using Orleans.Runtime;
using Orleans.Transactions.Abstractions;

namespace JumpStartCS.Orleans.Grains.Grains
{
    [Reentrant]
    public class AtmGrain : Grain, IAtmGrain, IIncomingGrainCallFilter
    {
        private readonly ITransactionalState<AtmState> _atmTransactionalState;
        private readonly ILogger<AtmGrain> _logger;

        public AtmGrain(
            ILogger<AtmGrain> logger,
            [TransactionalState("atm")] ITransactionalState<AtmState> atmTransactionalState)
        {
            _atmTransactionalState = atmTransactionalState;
            _logger = logger;
        }

        public async Task Initialise(decimal openingBalance)
        {
            await _atmTransactionalState.PerformUpdate(state =>
            {
                state.Balance = openingBalance;

                state.Id = this.GetGrainId().GetGuidKey();
            });
        }

        public async Task<decimal> GetBalance()
        {
            return await _atmTransactionalState.PerformRead((state) => state.Balance);
        }

        public async Task Withdraw(Guid checkingAccountId, decimal amount)
        {
            var checkingAccountGrain = GrainFactory.GetGrain<ICheckingAccountGrain>(checkingAccountId);

            await _atmTransactionalState.PerformUpdate(state =>
            {
                var currentAtmBalance = state.Balance;

                var updatedBalance = currentAtmBalance - amount;

                state.Balance = updatedBalance;
            });
        }

        public async Task Invoke(IIncomingGrainCallContext context)
        {
            _logger.LogInformation($"Incoming ATM Grain Filter: Recived grain call on '{context.Grain}' to '{context.MethodName}' method");

            await context.Invoke();
        }
    }
}
