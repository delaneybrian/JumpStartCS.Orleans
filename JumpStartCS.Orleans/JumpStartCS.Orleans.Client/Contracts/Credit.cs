using System.Runtime.Serialization;

namespace JumpStartCS.Orleans.Client.Contracts
{
    [DataContract]
    public record Credit
    {
        [DataMember]
        public decimal Amount { get; init; }
    }
}
