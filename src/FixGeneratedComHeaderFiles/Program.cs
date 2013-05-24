using System;
using System.IO;
using System.Text.RegularExpressions;

namespace FixGeneratedComHeaderFiles
{
	class Program
	{
		static int Main(string[] args)
		{
			if(args.Length != 2)
			{
				Console.WriteLine("{0} arguments passed to FixGenComHeaderFiles, 2 expected.", args.Length);
				ShowUsage();
				return 1; //invalid arguments
			}
			var inputFile = args[0];
			if(!File.Exists(inputFile))
			{
				ShowUsage();
				return 2; //no input file
			}
			var outputFile = args[1];
			var inputContents = File.ReadAllText(inputFile);
			var results = Regex.Replace(inputContents, "EXTERN_C const (?<group1>IID|CLSID|LIBID|DIID) (?<group2>IID|CLSID|LIBID|DIID)_(?<group3>..*);",
						  "#define ${group2}_${group3} __uuidof(${group3})");
			try
			{
				File.WriteAllText(outputFile, results);
			}
			catch (Exception)
			{
				return 3; //error writing file
			}
			return 0; //successful return
		}

		private static void ShowUsage()
		{
			Console.WriteLine("usage: FixGenComHeaderFiles [input file path] [output file path]");
			Console.WriteLine("\tError codes: [1]incorrect argument numbers\r\n\t[2]input file does not exist\r\n\t[3]Failed to write to output path");
		}
	}
}
