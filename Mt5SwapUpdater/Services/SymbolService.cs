// Обрати внимание на Admin API
using MetaQuotes.MT5CommonAPI;
using MetaQuotes.MT5ManagerAPI;
using SwapUpdater.Models;
using SwapUpdater.Utils;
using System;
using System.Linq;

namespace SwapUpdater.Services
{
    public class SymbolAdminServices
    {
        private readonly CIMTAdminAPI _admin;

        public SymbolAdminServices(CIMTAdminAPI admin)
        {
            _admin = admin ?? throw new ArgumentNullException(nameof(admin));
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

            // Массив символов, которые нужно обновить, и массив для результатов обновления
            CIMTConSymbol[] symbolsToUpdate = new CIMTConSymbol[total];
            MTRetCode[] results = new MTRetCode[total];

            int loadedSymbolsCount = 0;

            for (int i = 0; i < total; i++)
            {
                var update = updates[i];
                CIMTConSymbol symbol = _admin.SymbolCreate();

                // Получаем символ с сервера по имени
                var resGet = _admin.SymbolGet(update.SymbolName, symbol);
                if (resGet != MTRetCode.MT_RET_OK)
                {
                    Console.WriteLine($"Символ '{update.SymbolName}' не найден на сервере.");
                    symbol.Release();
                    continue;
                }

                Console.WriteLine($"[До обновления] {symbol.Symbol()}: SwapLong={symbol.SwapLong()}, SwapShort={symbol.SwapShort()}, SwapMode={symbol.SwapMode()}");

                bool changed = false;

                if (update.SwapLong.HasValue && Math.Abs(symbol.SwapLong() - update.SwapLong.Value) > 1e-9)
                {
                    var res = symbol.SwapLong(update.SwapLong.Value);
                    if (res != MTRetCode.MT_RET_OK)
                        Console.WriteLine($"Ошибка установки SwapLong для '{update.SymbolName}': {res}");
                    else
                        changed = true;
                }

                if (update.SwapShort.HasValue && Math.Abs(symbol.SwapShort() - update.SwapShort.Value) > 1e-9)
                {
                    var res = symbol.SwapShort(update.SwapShort.Value);
                    if (res != MTRetCode.MT_RET_OK)
                        Console.WriteLine($"Ошибка установки SwapShort для '{update.SymbolName}': {res}");
                    else
                        changed = true;
                }

                if (update.SwapMode.HasValue && symbol.SwapMode() != update.SwapMode.Value)
                {
                    var res = symbol.SwapMode(update.SwapMode.Value);
                    if (res != MTRetCode.MT_RET_OK)
                        Console.WriteLine($"Ошибка установки SwapMode для '{update.SymbolName}': {res}");
                    else
                        changed = true;
                }

                if (changed)
                {
                    symbolsToUpdate[loadedSymbolsCount++] = symbol; // добавляем для обновления
                }
                else
                {
                    Console.WriteLine($"Изменений для символа '{update.SymbolName}' нет.");
                    symbol.Release(); // освобождаем, если изменений нет
                }
            }

            if (loadedSymbolsCount == 0)
            {
                Console.WriteLine("Нет изменений для обновления.");
                return;
            }

            // Выполняем массовое обновление
            var resBatch = _admin.SymbolUpdateBatch(symbolsToUpdate.Take(loadedSymbolsCount).ToArray(), results);

            if (resBatch == MTRetCode.MT_RET_OK)
            {
                for (int i = 0; i < loadedSymbolsCount; i++)
                {
                    Console.WriteLine($"[После обновления] {symbolsToUpdate[i].Symbol()}: результат обновления - {results[i]}");

                    if (results[i] == MTRetCode.MT_RET_OK)
                    {
                        Console.WriteLine($"Новые параметры: SwapLong={symbolsToUpdate[i].SwapLong()}, SwapShort={symbolsToUpdate[i].SwapShort()}, SwapMode={symbolsToUpdate[i].SwapMode()}");
                    }

                    symbolsToUpdate[i].Release(); // освобождаем символы после обновления
                }
            }
            else
            {
                Console.WriteLine($"Ошибка массового обновления символов: {resBatch}");
                // освобождаем символы в любом случае
                for (int i = 0; i < loadedSymbolsCount; i++)
                    symbolsToUpdate[i].Release();
            }
        }
    }
}
