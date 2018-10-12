using System.IO;
using Compiler;

namespace Console
{
    internal class Program
    {
        private static string _inFilePath = "../../java/para.me";
        private static Syntactic _syntactic = new Syntactic();
        private static Semantic _semantic = new Semantic();

        private static void Main()
        {
            //check if file exists to avoid 
            if (!File.Exists(_inFilePath))
            {
                //abort mission if it doesn't
                System.Console.WriteLine("File does not exist.");
                return;
            }

            //go ahead and open the file if it exists.
            var stream = _semantic.Run(File.Open(_inFilePath, FileMode.Open));

            var fileContent = "";

            //use the using keyword to dispose the objects when you are done with them. 
            //or you can use streamReader.Dispose(); if you prefer to do so.
            using (var streamReader = new StreamReader(stream))
            {
                fileContent = streamReader.ReadToEnd();
            }

            System.Console.WriteLine(fileContent);
        }
    }
}
