namespace AVMGasStation.Model
{
    public class GasStationConfiguration
    {
        public string FundingAccount { get; set; } = String.Empty;
        public Dictionary<string, ulong> ChainGenesis2AppId { get; set; } = new Dictionary<string, ulong>();
    }
}
