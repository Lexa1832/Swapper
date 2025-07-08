namespace SwapUpdater.Models
{
    public class SymbolSwapData
    {
        public string? SymbolName { get; set; } = string.Empty;
        public double? SwapLong { get; set; }
        public double? SwapShort { get; set; }

        public uint? SwapMode { get; set; }

    }
}
