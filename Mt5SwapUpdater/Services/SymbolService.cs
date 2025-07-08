using MetaQuotes.MT5CommonAPI;
using MetaQuotes.MT5ManagerAPI;
using SwapUpdater.Models;
using SwapUpdater.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SwapUpdater.Services
{
    public class SymbolServices
    {
        private readonly CIMTManagerAPI _manager;

        public SymbolServices(CIMTManagerAPI manager)
        {
            _manager = manager ?? throw new ArgumentNullException(nameof(manager));
        }

        public void UpdateSwapsBatch(string filePath)
        {
            var csvUpdater = new CsvUpdater();
            var updates = csvUpdater.ReadCsv(filePath);
            int total = updates.Count;

            if (total == 0)
            {
                Console.WriteLine("Нет данных для обновления.");
                return;
            }

            CIMTConSymbol[] symbolsToUpdate = new CIMTConSymbol[total];
            MTRetCode[] results = new MTRetCode[total];

            int loadedSymbolsCount = 0;

            for (int i = 0; i < total; i++)
            {
                var update = updates[i];
                CIMTConSymbol symbol = _manager.SymbolCreate();

                var resGet = _manager.SymbolGet(update.SymbolName, symbol);
                if (resGet != MTRetCode.MT_RET_OK)
                {
                    Console.WriteLine($"Символ '{update.SymbolName}' не найден на сервере.");
                    symbol.Release();
                    continue;
                }

                // Вывод текущих значений до обновления
                Console.WriteLine($"[{update.SymbolName}] Текущие значения: SwapLong={symbol.SwapLong()}, SwapShort={symbol.SwapShort()}, SwapMode={symbol.SwapMode()}");

                bool changed = false;

                if (update.SwapLong.HasValue && Math.Abs(symbol.SwapLong() - update.SwapLong.Value) > 1e-9)
                {
                    var res = symbol.SwapLong(update.SwapLong.Value);
                    if (res != MTRetCode.MT_RET_OK)
                        Console.WriteLine($"Ошибка установки SwapLong для '{update.SymbolName}': {res}");
                    else changed = true;
                }

                if (update.SwapShort.HasValue && Math.Abs(symbol.SwapShort() - update.SwapShort.Value) > 1e-9)
                {
                    var res = symbol.SwapShort(update.SwapShort.Value);
                    if (res != MTRetCode.MT_RET_OK)
                        Console.WriteLine($"Ошибка установки SwapShort для '{update.SymbolName}': {res}");
                    else changed = true;
                }

                if (update.SwapMode.HasValue && symbol.SwapMode() != update.SwapMode.Value)
                {
                    var res = symbol.SwapMode(update.SwapMode.Value);
                    if (res != MTRetCode.MT_RET_OK)
                        Console.WriteLine($"Ошибка установки SwapMode для '{update.SymbolName}': {res}");
                    else changed = true;
                }

                if (changed)
                {
                    symbolsToUpdate[loadedSymbolsCount++] = symbol;
                }
                else
                {
                    Console.WriteLine($"[{update.SymbolName}] Нет изменений, обновление не требуется.");
                    symbol.Release();
                }
            }

            if (loadedSymbolsCount == 0)
            {
                Console.WriteLine("Нет изменений для обновления.");
                return;
            }

            var resBatch = _manager.SymbolUpdateBatch(symbolsToUpdate.Take(loadedSymbolsCount).ToArray(), results);

            if (resBatch == MTRetCode.MT_RET_OK)
            {
                for (int i = 0; i < loadedSymbolsCount; i++)
                {
                    var sym = symbolsToUpdate[i];
                    Console.WriteLine($"Обновление символа '{sym.Symbol()}': результат - {results[i]}");
                    // Вывод значений после обновления
                    Console.WriteLine($"[{sym.Symbol()}] Новые значения: SwapLong={sym.SwapLong()}, SwapShort={sym.SwapShort()}, SwapMode={sym.SwapMode()}");
                    sym.Release();
                }
            }
            else
            {
                Console.WriteLine($"Ошибка массового обновления символов: {resBatch}");
                for (int i = 0; i < loadedSymbolsCount; i++)
                    symbolsToUpdate[i].Release();
            }
        }
    }
}