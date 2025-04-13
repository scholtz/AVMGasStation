namespace AVMGasStation.Model.GasConfiguration
{
    /// <summary>
    /// Version 1 of the configuration uses the or between all items and all items in the list are whitelisted.
    /// 
    /// All other assets, apps or addresses are excluded.
    /// </summary>
    public class GasConfigurationV1 : GasConfiguration
    {
        /// <summary>
        /// Whitelisted assets
        /// </summary>
        public List<ulong> Assets { get; set; } = new List<ulong> { };
        /// <summary>
        /// Whitelisted apps
        /// </summary>
        public List<ulong> Apps { get; set; } = new List<ulong> { };
        /// <summary>
        /// Whitelisted addresses
        /// </summary>
        public List<string> Addresses { get; set; } = new List<string> { };
    }
}
