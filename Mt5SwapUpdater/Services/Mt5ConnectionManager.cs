using System;
using System.IO;
using MetaQuotes.MT5ManagerAPI;
using MetaQuotes.MT5CommonAPI;

namespace SwapUpdater.Services
{
    public class Mt5ConnectionManager
    {
        private CIMTManagerAPI? _manager;

        public CIMTManagerAPI? Manager => _manager;

        private readonly string _sdkPath;

        public Mt5ConnectionManager(string sdkPath)
        {
            _sdkPath = sdkPath ?? throw new ArgumentNullException(nameof(sdkPath));
            var initResult = SMTManagerAPIFactory.Initialize(_sdkPath);
            if (initResult != MTRetCode.MT_RET_OK)
                throw new Exception($"Ошибка инициализации фабрики Manager API: {initResult}");
        }

        public bool Connect(string server, ulong login, string password, out string errorMessage, uint version = 0x1000)
        {
            errorMessage = null;
            try
            {
                Console.WriteLine($"Попытка создания Manager API с версией {version}");

                MTRetCode result;
                _manager = SMTManagerAPIFactory.CreateManager(version, out result);

                if (result != MTRetCode.MT_RET_OK || _manager == null)
                {
                    errorMessage = $"Ошибка создания Manager API: {result}";
                    Console.Error.WriteLine(errorMessage);
                    return false;
                }

                Console.WriteLine("Manager API создан успешно.");

                result = _manager.Connect(server, login, password, "", CIMTManagerAPI.EnPumpModes.PUMP_MODE_FULL, 5000);
                if (result != MTRetCode.MT_RET_OK)
                {
                    errorMessage = $"Ошибка подключения: {result}";
                    Console.Error.WriteLine(errorMessage);
                    return false;
                }

                Console.WriteLine("Подключение к серверу прошло успешно.");

                int symbolsCount = Convert.ToInt32(_manager.SymbolTotal());
                if (symbolsCount <= 0)
                {
                    errorMessage = "Связь установлена, но символы не получены.";
                    Console.Error.WriteLine(errorMessage);
                    return false;
                }

                Console.WriteLine($"Успешное подключение. Символов на сервере: {symbolsCount}");
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = $"Исключение при подключении: {ex.Message}";
                Console.Error.WriteLine(errorMessage);
                return false;
            }
        }

        public void Disconnect()
        {
            try
            {
                if (_manager != null)
                {
                    _manager.Disconnect();
                    _manager.Dispose();
                    _manager = null;
                    Console.WriteLine("Отключение от сервера выполнено.");
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Ошибка при отключении: {ex.Message}");
            }
        }

        public CIMTConSymbol[] GetAllSymbols()
        {
            if (_manager == null)
                return Array.Empty<CIMTConSymbol>();

            int total = Convert.ToInt32(_manager.SymbolTotal());
            if (total <= 0)
                return Array.Empty<CIMTConSymbol>();

            var symbols = new CIMTConSymbol[total];

            for (uint i = 0; i < total; i++)
            {
                var symbol = _manager.SymbolCreate();
                if (symbol == null)
                {
                    Console.Error.WriteLine($"SymbolCreate() вернул null для индекса {i}");
                    continue;
                }

                var res = _manager.SymbolNext(i, symbol);
                if (res == MTRetCode.MT_RET_OK)
                    symbols[i] = symbol;
                else
                {
                    Console.Error.WriteLine($"Ошибка получения символа #{i}: {res}");
                    symbol.Release();
                }
            }

            return symbols;
        }
    }
}
