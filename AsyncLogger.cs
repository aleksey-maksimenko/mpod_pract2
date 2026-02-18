using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pract2
{
    internal class AsyncLogger
    {
        private static readonly string FileName = "log.txt";

        // асинхронная запись через tap
        public static Task LogAsync(string message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));
            byte[] data = Encoding.UTF8.GetBytes(message + Environment.NewLine);
            FileStream fs = new FileStream(
                FileName,
                FileMode.Append,
                FileAccess.Write,
                FileShare.Read,
                4096,
                FileOptions.Asynchronous
            );
            // записываем данные асинхронно
            return fs.WriteAsync(data, 0, data.Length)
                     .ContinueWith(t => fs.Dispose());
        }

        // асинхронная запись через APM с callback
        public static void LogWithCallback(string message, Action callback)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));
            byte[] data = Encoding.UTF8.GetBytes(message + Environment.NewLine);
            FileStream fs = new FileStream(
                FileName,
                FileMode.Append,
                FileAccess.Write,
                FileShare.Read,
                4096,
                FileOptions.Asynchronous
            );
            // начинаем асинхронную запись
            fs.BeginWrite(data, 0, data.Length, ar =>
            {
                try
                {
                    fs.EndWrite(ar);
                }
                finally
                {
                    fs.Dispose();
                    callback?.Invoke();
                }
            }, null);
        }
    }
}
