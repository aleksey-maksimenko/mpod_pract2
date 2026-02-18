using System;
using System.Threading;

namespace Pract2
{
    internal class DataProcessor
    {
        public decimal[] ProcessDataSequential(decimal[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            decimal[] result = new decimal[data.Length];
            // последовательная обработка каждого элемента
            for (int i = 0; i < data.Length; i++)
            {
                double tmp = (double)data[i];
                // несколько вычислений подряд, чтобы нагрузка была заметнее
                for (int k = 0; k < 3; k++)
                {
                    tmp = Math.Sqrt(tmp) * Math.Log10(tmp + 1.0);
                }
                // результат обратно в decimal
                result[i] = (decimal)tmp;
            }
            return result;
        }

        public decimal[] ProcessDataParallel(decimal[] data, int threadCount)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (threadCount <= 0)
                throw new ArgumentException("количество потоков должно задаваться положительным числом!");
            decimal[] result = new decimal[data.Length];


            Thread[] threads = new Thread[threadCount];  // массив потоков
            CountdownEvent done = new CountdownEvent(threadCount); // объект для ожидания завершения всех потоков
            Exception err = null; // переменная для хранения ошибки из потоков
            object errLock = new object();
            // размер части массива для одного потока
            int part = data.Length / threadCount;
            for (int i = 0; i < threadCount; i++)
            {
                // вычисляем границы для потока
                int start = i * part;
                int end = (i == threadCount - 1) ? data.Length : start + part;
                threads[i] = new Thread(() =>
                {
                    try
                    {
                        // каждый поток обрабатывает свой диапазон
                        for (int j = start; j < end; j++)
                        {
                            double tmp = (double)data[j];
                            // та же нагрузка, что и в последовательной версии
                            for (int k = 0; k < 3; k++)
                            {
                                tmp = Math.Sqrt(tmp) * Math.Log10(tmp + 1.0);
                            }
                            // запись результата в общий массив
                            result[j] = (decimal)tmp;
                        }
                    }
                    catch (Exception ex)
                    {
                        // сохраняем первую возникшую ошибку
                        lock (errLock)
                        {
                            if (err == null)
                                err = ex;
                        }
                    }
                    finally
                    {
                        done.Signal();
                    }
                });
                threads[i].Start(); // запуск потока
            }
            // ожидаем завершения всех потоков
            done.Wait();
            done.Dispose();
            // если в потоках была ошибка, выбрасываем ее в основном
            if (err != null)
                throw new AggregateException("thread error", err);
            return result;
        }
    }
}
