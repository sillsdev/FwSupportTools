using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Po2Xml
{
	class Program
	{
		static int Main(string[] args)
		{
			var po2XmlConverter = new Po2XmlConverter();

			for (int i = 0; i < args.Count(); i++)
			{
				Console.WriteLine(args[i]);
				switch (args[i].ToLowerInvariant())
				{
					case "-r":
					case "--roundtrip":
						po2XmlConverter.Roundtrip = true;
						break;

					case "-o":
					case "--output":
						if (i + 1 >= args.Count())
						{
							Usage();
							return -1;
						}
						po2XmlConverter.XmlFilePath = args[i + 1];
						i++;
						break;

					default:
						po2XmlConverter.PoFilePath = args[i];
						break;
				}
			}
			if (po2XmlConverter.XmlFilePath == "-")
				po2XmlConverter.XmlFilePath = null;

			if (po2XmlConverter.PoFilePath == null)
			{
				Usage();
				return -1;
			}

			return po2XmlConverter.Run();
		}

		static void Usage()
		{
			Console.WriteLine("Po2Xml converts a .po file to XML.");
			Console.WriteLine("Usage: Po2Xml [options] POFILE ...");
			Console.WriteLine("-r | --roundtrip : generate round-tripable output.");
			Console.WriteLine("-o XMLFILE | --output XMLFILE : write output to XMLFILE; use '-' for standard output.");
		}
	}
}
