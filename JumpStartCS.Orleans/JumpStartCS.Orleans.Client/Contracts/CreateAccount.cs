using System.Runtime.Serialization;

namespace JumpStartCS.Orleans.Client.Contracts
{
    [DataContract]
    public record CreateAccount
    {
        [DataMember]
        public decimal OpeningBalance { get; init; }
    }
}
