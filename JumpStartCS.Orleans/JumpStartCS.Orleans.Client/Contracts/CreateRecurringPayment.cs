using System.Runtime.Serialization;

namespace JumpStartCS.Orleans.Client.Contracts
{
    [DataContract]
    public record CreateRecurringPayment
    {
        [DataMember]
        public Guid PaymentId { get; init; }

        [DataMember] 
        public decimal PaymentAmount { get; init; }

        [DataMember]
        public int PaymentRecurrsEveryMinutes { get; init; }
    }
}
