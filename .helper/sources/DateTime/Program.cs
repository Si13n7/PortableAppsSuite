using System;

namespace DTime
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
                Console.Write(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss,fff zzz"));
            else
                Console.Write(DateTime.Now.ToString(string.Join(" ", args)));
        }
    }
}
