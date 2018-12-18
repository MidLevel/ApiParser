using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace ApiParser
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 4) return;

            string inputDll = Path.GetFullPath(args[0]);
            string inputXml = Path.GetFullPath(args[1]);

            string outputYaml = Path.GetFullPath(args[2]);
            string outputFolder = Path.GetFullPath(args[3]);

            if (!Directory.Exists(outputFolder))
                Directory.CreateDirectory(outputFolder);
            else
                Directory.GetFiles(outputFolder).ToList().ForEach(x => File.Delete(Path.Combine(outputFolder, x)));

            Assembly assembly = Assembly.UnsafeLoadFrom(inputDll);

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(inputXml);

            Console.WriteLine("Parsing XmlDocs");
            XmlDocTree xmlDocTree = XmlDocTree.Parse(xmlDocument);
            Console.WriteLine("Parsing DocTree");
            DocTree docTree = DocTree.Parse(assembly, xmlDocTree);
            Console.WriteLine("Printing DocTree");
            Printer.Print(docTree, outputYaml, outputFolder);
        }
    }
}
