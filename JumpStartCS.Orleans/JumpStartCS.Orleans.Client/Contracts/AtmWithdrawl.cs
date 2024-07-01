using System.Runtime.Serialization;

namespace JumpStartCS.Orleans.Client.Contracts
{
    [DataContract]
    public record AtmWithdrawl
    {
        [DataMember]
        public Guid CheckingAccountId { get; init; }

        [DataMember]
        public decimal Amount { get; init; }
    }
}
