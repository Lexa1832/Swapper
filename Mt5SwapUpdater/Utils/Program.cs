using SwapUpdater.Services;
using SwapUpdater.Utils;
using System;

class Program
{
    static void Main(string[] args)
    {
        var server = "127.0.0.1:443";
        ulong login = 123456; // твой логин
        var password = "password";

        var mt5 = new Mt5ConnectionManager();

        if (mt5.Connect(server, login, password, out string error))
        {
            Logger.Info("Подключение успешно.");

            var symbols = mt5.GetAllSymbols();
            Logger.Info($"Всего символов на сервере: {symbols.Length}");
            foreach (var symbol in symbols)
            {
                Logger.Info($"Символ: {symbol.Symbol()}");
            }

            mt5.Disconnect();
        }
        else
        {
            Logger.Error($"Не удалось подключиться: {error}");
        }
    }
}
