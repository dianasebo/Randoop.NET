namespace Randoop.RandoopContracts
{
    public class ContractState
    {
        public ContractStateEnum PostconditionState { get; set; }
        public ContractStateEnum InvariantState { get; set; }
        public ContractStateEnum StaticInvariantState { get; set; }
    }
}
