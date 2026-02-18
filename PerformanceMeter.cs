using System;
using System.Diagnostics;

namespace Pract2
{
    internal class PerformanceMeter
    {
        public static (long timeMs, decimal[] result) MeasureExecutionTime(Func<decimal[]> action, string name)
        {
            // проверка делегата
            if (action == null)
                throw new ArgumentNullException(nameof(action));
            // выполняем переданную операцию с фиксацией времени
            Stopwatch sw = new Stopwatch();
            sw.Start();
            decimal[] res = action();
            sw.Stop();
            // выводим время выполнения
            long ms = sw.ElapsedMilliseconds;
            Console.WriteLine($"{name}: {ms} мс");
            return (ms, res);
        }

        public static bool CompareResults(decimal[] r1, decimal[] r2, decimal tol = 0.0001m)
        {
            // проверка массивов
            if (r1 == null || r2 == null)
                return false;
            if (r1.Length != r2.Length)
                return false;
            // поэлементное сравнение с заданной точностью
            for (int i = 0; i < r1.Length; i++)
            {
                decimal diff = Math.Abs(r1[i] - r2[i]);
                if (diff > tol)
                    return false;
            }
            return true;
        }
    }
}
