using System;
using System.IO;
using compiler;

namespace console
{
    class Program
    {
        static string IN_FILE_PATH = "../../java/para.me";
        static Compiler compiler = new Compiler();

        static void Main(string[] args)
        {
            var stream = compiler.Run(File.Open(IN_FILE_PATH, FileMode.Open));
            var streamReader = new StreamReader(stream);

            Console.WriteLine(streamReader.ReadToEnd());
        }
    }
}
