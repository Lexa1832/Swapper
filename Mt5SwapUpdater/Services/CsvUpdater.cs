using System;
using System.Globalization;
using System.IO;
using System.Linq;
using MetaQuotes.MT5CommonAPI;
using MetaQuotes.MT5ManagerAPI;
using SwapUpdater.Models;
using SwapUpdater.Services;

namespace SwapUpdater.Utils
{
    public class CsvUpdater
    {
        /// <summary>
        /// Чтение CSV-файла с данными о свопах.
        /// Ожидается разделитель ';' и формат: Symbol;SwapLong;SwapShort;SwapMode
        /// </summary>
        public List<SymbolSwapData> ReadCsv(string filePath)
        {
            var result = new List<SymbolSwapData>();
            foreach (var line in File.ReadLines(filePath).Skip(1)) // Пропускаем заголовок
            {
                var parts = line.Split(';');
                if (parts.Length < 4) continue;

                var symbol = new SymbolSwapData
                {
                    SymbolName = parts[0].Trim(),
                    SwapLong = double.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var swapLong) ? swapLong : (double?)null,
                    SwapShort = double.TryParse(parts[2], NumberStyles.Any, CultureInfo.InvariantCulture, out var swapShort) ? swapShort : (double?)null,
                    SwapMode = uint.TryParse(parts[3], out var swapMode) ? swapMode : (uint?)null
                };

                if (!string.IsNullOrWhiteSpace(symbol.SymbolName) &&
                    (symbol.SwapLong.HasValue || symbol.SwapShort.HasValue || symbol.SwapMode.HasValue))
                {
                    result.Add(symbol);
                }
            }
            return result;
        }
    }

}