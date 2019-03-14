using System;
using System.Text;
using System.IO;

namespace AluminumFoil
{
    public class Logging
    {
        public static FileLogger logger;
        public static void SetupLogging()
        {
            Logging.logger = new FileLogger();
            Console.SetOut(Logging.logger);
            Console.WriteLine("AluminumFoil Started");
        }

        public static void StopLogging()
        {
            if (Logging.logger != null)
            {
                Logging.logger.Dispose();
            }
        }
    }

    public class FileLogger : TextWriter, IDisposable
    {
        public override Encoding Encoding => throw new NotImplementedException();

        public override void WriteLine(string value)
        {

            Writer.WriteLine(DateTime.Now + value);
            
        }

        private readonly StreamWriter Writer;

        public FileLogger()
        {
            Writer = new StreamWriter("./AluminumFoilLog.txt");
            Writer.AutoFlush = true;
        }


        void IDisposable.Dispose()
        {
            Writer.Dispose();
        }
    }
}
