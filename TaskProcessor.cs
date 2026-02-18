using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pract2
{
    internal class TaskProcessor
    {
        // обработка данных через пул потоков
        public decimal[] ProcessDataWithThreadPool(decimal[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            int parts = 8;
            decimal[] result = new decimal[data.Length];

            // событие для ожидания завершения всех задач
            CountdownEvent done = new CountdownEvent(parts);
            Exception err = null;
            object errLock = new object();

            int partSize = data.Length / parts;

            for (int i = 0; i < parts; i++)
            {
                int start = i * partSize;
                int end = (i == parts - 1) ? data.Length : start + partSize;
                // ставим задачу в пул потоков
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    try
                    {
                        for (int j = start; j < end; j++)
                        {
                            double tmp = (double)data[j];

                            // имитируем вычислительную нагрузку
                            for (int k = 0; k < 3; k++)
                            {
                                tmp = Math.Sqrt(tmp) * Math.Log10(tmp + 1.0);
                            }

                            result[j] = (decimal)tmp;
                        }
                    }
                    catch (Exception ex)
                    {
                        lock (errLock)
                        {
                            if (err == null)
                                err = ex;
                        }
                    }
                    finally
                    {
                        // сигнал о завершении работы части
                        done.Signal();
                    }
                });
            }
            // ждем завершения всех частей
            done.Wait();
            done.Dispose();
            if (err != null)
                throw new AggregateException("threadpool error", err);

            return result;
        }

        // обработка данных через TAP task-based asynchronous pattern
        public Task<decimal[]> ProcessDataAsync(decimal[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            // используем task.run для запуска в пуле потоков
            return Task.Run(() =>
            {
                int parts = 8;
                decimal[] result = new decimal[data.Length];

                CountdownEvent done = new CountdownEvent(parts);
                Exception err = null;
                object errLock = new object();

                int partSize = data.Length / parts;
                for (int i = 0; i < parts; i++)
                {
                    int start = i * partSize;
                    int end = (i == parts - 1) ? data.Length : start + partSize;
                    ThreadPool.QueueUserWorkItem(_ =>
                    {
                        try
                        {
                            for (int j = start; j < end; j++)
                            {
                                double tmp = (double)data[j];
                                for (int k = 0; k < 3; k++)
                                {
                                    tmp = Math.Sqrt(tmp) * Math.Log10(tmp + 1.0);
                                }
                                result[j] = (decimal)tmp;
                            }
                        }
                        catch (Exception ex)
                        {
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
                }
                done.Wait();
                done.Dispose();
                if (err != null)
                    throw err;
                return result;
            });
        }

        // реализация apm (begin/end)
        public IAsyncResult BeginProcessData(decimal[] data, AsyncCallback callback, object state)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            APMResult ar = new APMResult(state);
            // ставим работу в пул потоков
            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    ar.Result = ProcessDataWithThreadPool(data);
                }
                catch (Exception ex)
                {
                    ar.Error = ex;
                }
                finally
                {
                    ar.SetCompleted();  // помечаем операцию завершенной
                    callback?.Invoke(ar); // вызываем callback, если задан
                }
            });

            return ar;
        }

        public decimal[] EndProcessData(IAsyncResult asyncResult)
        {
            APMResult ar = asyncResult as APMResult;
            if (ar == null)
                throw new ArgumentException("Некорректный объект async result");
            // ждем завершения операции
            ar.AsyncWaitHandle.WaitOne();
            if (ar.Error != null)
                throw ar.Error;
            return ar.Result;
        }

        // вспомогательный класс для apm
        private class APMResult : IAsyncResult
        {
            private ManualResetEvent waitHandle = new ManualResetEvent(false);

            public decimal[] Result;
            public Exception Error;

            public APMResult(object state)
            {
                AsyncState = state;
            }

            public void SetCompleted()
            {
                IsCompleted = true;
                waitHandle.Set();
            }

            public object AsyncState { get; private set; }
            public WaitHandle AsyncWaitHandle => waitHandle;
            public bool CompletedSynchronously => false;
            public bool IsCompleted { get; private set; }
        }
    }
}
