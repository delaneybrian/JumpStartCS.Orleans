namespace JumpStartCS.Orleans.Grains.State
{
    [GenerateSerializer]
    public record CustomerState
    {
        [Id(0)]
        public Dictionary<Guid, decimal> CheckingAccountBalanceById { get; set; } = new Dictionary<Guid, decimal>();
    }
}
