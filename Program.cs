using System;
using System.Diagnostics;


namespace Pract2
{
    internal class Program
    {
        static void Main(string[] args)
        {
            const int size = 10_000_000;

            Console.WriteLine("Генерация данных...");
            decimal[] data = GenData(size);

            DataProcessor seqProc = new DataProcessor();
            TaskProcessor taskProc = new TaskProcessor();

            Console.WriteLine("Запуск последовательной обработки...");
            long seqTime = Measure(() => seqProc.ProcessDataSequential(data), out decimal[] seqRes);
            Console.WriteLine("Запуск ThreadPool обработки...");
            long poolTime = Measure(() => taskProc.ProcessDataWithThreadPool(data), out decimal[] poolRes);
            Console.WriteLine("Запуск TAP обработки...");
            long tapTime = Measure(() => taskProc.ProcessDataAsync(data).Result, out decimal[] tapRes);
            Console.WriteLine("Запуск APM обработки...");
            long apmTime = Measure(() =>
            {
                IAsyncResult ar = taskProc.BeginProcessData(data, null, null);
                return taskProc.EndProcessData(ar);
            }, out decimal[] apmRes);

            bool same =
                PerformanceMeter.CompareResults(seqRes, poolRes) &&
                PerformanceMeter.CompareResults(seqRes, tapRes) &&
                PerformanceMeter.CompareResults(seqRes, apmRes);

            Console.WriteLine();
            Console.WriteLine("=== Результаты обработки ===");
            Console.WriteLine($"Размер данных: {size} элементов");
            Console.WriteLine($"Последовательная обработка: {seqTime} мс");
            Console.WriteLine($"ThreadPool обработка: {poolTime} мс");
            Console.WriteLine($"TAP обработка: {tapTime} мс");
            Console.WriteLine($"APM обработка: {apmTime} мс");
            Console.WriteLine($"Ускорение ThreadPool: {(double)seqTime / poolTime:F2}x");
            Console.WriteLine($"Ускорение TAP: {(double)seqTime / tapTime:F2}x");
            Console.WriteLine($"Ускорение APM: {(double)seqTime / apmTime:F2}x");
            Console.WriteLine($"Результаты совпадают: {(same ? "Да" : "Нет")}");

            Console.WriteLine("\nНажмите любую клавишу...");
            Console.ReadKey();
        }

        static decimal[] GenData(int size)
        {
            decimal[] data = new decimal[size];
            Random rnd = new Random(42);
            for (int i = 0; i < size; i++)
            {
                double v = 1.0 + rnd.NextDouble() * 999.0;
                data[i] = (decimal)v;
            }

            return data;
        }

        static long Measure(Func<decimal[]> action, out decimal[] res)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            res = action();
            sw.Stop();
            return sw.ElapsedMilliseconds;
        }
    }
}
