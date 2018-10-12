using System;
using System.IO;
using compiler;
using compiler.Constansts;

namespace console
{
    class Program
    {
        static Syntactic syntactic = new Syntactic();
        static Semantic semantic = new Semantic();

        static void Main(string[] args)
        {
            var stream = semantic.Run(File.Open(Constants.Paths.In, FileMode.Open));
            var streamReader = new StreamReader(stream);

            Console.WriteLine(streamReader.ReadToEnd());
        }
    }
}
