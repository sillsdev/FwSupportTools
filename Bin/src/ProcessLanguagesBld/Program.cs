using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProcessLanguagesBld
{
	class Program
	{
		/// <summary>
		/// Entry point.
		/// Needs to be called with 2 arguments: the path to the FW root folder (e.g. C:\FW-WW)
		/// and the path to the FW Output folder including build config (e.g. C:\FW-WW\Output\Release).
		/// </summary>
		/// <param name="args">Command line arguments</param>
		/// <returns>Zero if successful, otherwise number of .po files that led to an error.</returns>
		static int Main(string[] args)
		{
			if (args.Length != 2)
			{
				Console.WriteLine("ERROR: ProcessLanguagesBld.exe usage:");
				Console.WriteLine("ProcessLanguagesBld.exe <FW-path> <FW-Output-Release-path>");
				Console.WriteLine(args.Length + " argument(s) supplied. Must supply exactly 2 arguments.");
				return -1;
			}

			// Set up main object to process .po files:
			var langProc = new LanguageProcessor();

			langProc.FwRoot = args[0];
			langProc.FwOutput = args[1];

			return langProc.Run();
		}
	}
}
