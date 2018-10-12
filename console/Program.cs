using System;
using System.IO;
using compiler;

namespace console
{
    class Program
    {
        static string IN_FILE_PATH = "../../java/para.me";
        static Syntactic syntactic = new Syntactic();
        static Semantic semantic = new Semantic();

        static void Main(string[] args)
        {
            var stream = semantic.Run(File.Open(IN_FILE_PATH, FileMode.Open));
            var streamReader = new StreamReader(stream);

            Console.WriteLine(streamReader.ReadToEnd());
        }
    }
}
