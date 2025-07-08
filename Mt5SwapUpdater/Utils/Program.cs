using System;
using SwapUpdater.Services;
using SwapUpdater.Utils;

public class Program
{
    static void Main()
    {
        string sdkPath = @"C:\MetaTrader5SDK\Libs";
        var mt5Manager = new Mt5ConnectionManager(sdkPath);

        string server = "142.91.121.108:443";
        ulong login = 12893028;
        string password = "!4RpOtFb";

        string csvPath = "test.csv"; // убедись, что файл лежит рядом с .exe

        if (mt5Manager.Connect(server, login, password, out var err))
        {
            Console.WriteLine("✅ Подключились!");

            var symbols = mt5Manager.GetAllSymbols();
            Console.WriteLine($"🔍 Получено символов с сервера: {symbols.Length}");

            // Передаём именно CIMTManagerAPI, а не обёртку
            var symbolService = new SymbolServices(mt5Manager.Manager);
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
