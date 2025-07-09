using System;
using SwapUpdater.Services;

public class Program
{
    static void Main()
    {
        string sdkPath = @"C:\MetaTrader5SDK\Libs";
        var mt5Manager = new Mt5ConnectionManager(sdkPath);

        string server = "142.91.121.108:443";
        ulong login = 12893028;
        string password = "!4RpOtFb";

        string csvPath = "test.csv";

        if (mt5Manager.Connect(server, login, password, out var err))
        {
            Console.WriteLine("✅ Подключились!");

            // Теперь передаем Admin API в сервис символов
            var symbolService = new SymbolAdminServices(mt5Manager.Admin);
            symbolService.UpdateSwapsBatch(csvPath);

            mt5Manager.Disconnect();
            Console.WriteLine("🔌 Отключились от сервера.");
        }
        else
        {
            Console.WriteLine("❌ Ошибка подключения: " + err);
        }
    }
}
