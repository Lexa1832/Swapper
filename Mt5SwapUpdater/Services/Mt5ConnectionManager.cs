using System;
using MetaQuotes.MT5ManagerAPI;
using MetaQuotes.MT5CommonAPI;

namespace SwapUpdater.Services
{
    public class Mt5ConnectionManager
    {
        private CIMTAdminAPI _admin;

        private readonly string _sdkPath;

        public Mt5ConnectionManager(string sdkPath)
        {
            _sdkPath = sdkPath ?? throw new ArgumentNullException(nameof(sdkPath));
            var initResult = SMTManagerAPIFactory.Initialize(_sdkPath);
            if (initResult != MTRetCode.MT_RET_OK)
                throw new Exception($"Ошибка инициализации фабрики Admin API: {initResult}");
        }

        // Публичное свойство для доступа к Admin API
        public CIMTAdminAPI Admin => _admin;

        public bool Connect(string server, ulong login, string password, out string errorMessage, uint version = 0x1000)
        {
            errorMessage = null;
            try
            {
                Console.WriteLine($"Попытка создания Admin API с версией {version}");

                MTRetCode result;
                _admin = SMTManagerAPIFactory.CreateAdmin(version, out result);

                if (result != MTRetCode.MT_RET_OK || _admin == null)
                {
                    errorMessage = $"Ошибка создания Admin API: {result}";
                    Console.Error.WriteLine(errorMessage);
                    return false;
                }

                Console.WriteLine("Admin API создан успешно.");

                // Передаем 6 параметров, password_cert пустая строка, timeout 5000 мс
                result = _admin.Connect(server, login, password, "", CIMTAdminAPI.EnPumpModes.PUMP_MODE_FULL, 5000);
                if (result != MTRetCode.MT_RET_OK)
                {
                    errorMessage = $"Ошибка подключения: {result}";
                    Console.Error.WriteLine(errorMessage);
                    return false;
                }

                Console.WriteLine("Подключение к серверу прошло успешно.");

                int symbolsCount = Convert.ToInt32(_admin.SymbolTotal());
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
                if (_admin != null)
                {
                    _admin.Disconnect();
                    _admin.Dispose();
                    _admin = null;
                    Console.WriteLine("Отключение от сервера выполнено.");
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Ошибка при отключении: {ex.Message}");
            }
        }

        // Тут можешь добавить методы для работы с _admin (например, получение символов и т.п.)
    }
}
