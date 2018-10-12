using System.IO;
using Compiler;

namespace Console
{
    internal class Program
    {
        private static string _inFilePath = "../../java/para.me";
        private static Syntactic _syntactic = new Syntactic();
        private static Semantic _semantic = new Semantic();

        private static void Main(string[] args)
        {
            var stream = _semantic.Run(File.Open(_inFilePath, FileMode.Open));
            var streamReader = new StreamReader(stream);

            System.Console.WriteLine(streamReader.ReadToEnd());
        }
    }
}
