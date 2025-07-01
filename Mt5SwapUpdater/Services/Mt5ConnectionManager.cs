using MetaQuotes.MT5CommonAPI;
using MetaQuotes.MT5ManagerAPI;
using SwapUpdater.Utils;
using System;

namespace SwapUpdater.Services
{
    public class Mt5ConnectionManager
    {
        private CIMTManagerAPI _manager;

        public CIMTManagerAPI Manager => _manager;

        public bool Connect(string server, ulong login, string password, out string errorMessage)
        {
            errorMessage = null;
            try
            {
                uint version = 0x1000; // Версия API
                MTRetCode result;

                _manager = SMTManagerAPIFactory.CreateManager(version, "", out result);

                if (result != MTRetCode.MT_RET_OK || _manager == null)
                {
                    errorMessage = $"Ошибка создания Manager API: {result}";
                    Logger.Error(errorMessage);
                    return false;
                }

                Logger.Info("Попытка подключения к серверу MT5...");

                result = _manager.Connect(
                    server,
                    login,
                    password,
                    "",
                    CIMTManagerAPI.EnPumpModes.PUMP_MODE_SYMBOLS,  // Правильный режим
                    3000);

                if (result != MTRetCode.MT_RET_OK)
                {
                    errorMessage = $"Ошибка подключения: {result}";
                    Logger.Error(errorMessage);
                    return false;
                }

                int symbolCount = Convert.ToInt32(_manager.SymbolTotal());
                if (symbolCount <= 0)
                {
                    errorMessage = "Связь установлена, но символы не получены.";
                    Logger.Warn(errorMessage);
                    return false;
                }

                Logger.Info($"Успешное подключение. Символов на сервере: {symbolCount}");
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = $"Исключение при подключении: {ex.Message}";
                Logger.Exception(errorMessage, ex);
                return false;
            }
        }

        public void Disconnect()
        {
            try
            {
                _manager?.Disconnect();
                Logger.Info("Отключение от сервера выполнено.");
            }
            catch (Exception ex)
            {
                Logger.Exception("Ошибка при отключении.", ex);
            }
        }

        public CIMTConSymbol[] GetAllSymbols()
        {
            int total = Convert.ToInt32(_manager.SymbolTotal());
            if (total <= 0)
                return Array.Empty<CIMTConSymbol>();

            var symbols = new CIMTConSymbol[total];

            for (uint i = 0; i < total; i++)
            {
                var symbol = _manager.SymbolCreate();
                if (symbol == null)
                {
                    Logger.Warn($"SymbolCreate() вернул null для индекса {i}");
                    continue;
                }

                var res = _manager.SymbolNext(i, symbol);
                if (res == MTRetCode.MT_RET_OK)
                    symbols[i] = symbol;
                else
                {
                    Logger.Warn($"Ошибка получения символа #{i}: {res}");
                    symbol.Release();
                }
            }

            return symbols;
        }
    }
}
