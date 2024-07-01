namespace JumpStartCS.Orleans.Grains.State
{
    [GenerateSerializer]
    public record TransferState
    {
        [Id(0)]
        public int TransferCount { get; set; }
    }
}
