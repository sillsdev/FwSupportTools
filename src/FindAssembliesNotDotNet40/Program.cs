/* Utility to identify .NET DLL assemblies that are built under a .NET framework
 * other than 4.0.
 * To use, simply copy the built .exe into a folder whose DLLs you wish to examine
 * and then double-click it.
 * Alternatively, run from the command line with the folder you want to examine
 * given as the first command line argument.
 */

using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace FindAssembliesNotDotNet40
{
	class Program
	{
		static void Main(string[] args)
		{
			var folder = ".";
			if (args.Length >= 1)
				folder = args[0];

			var di = new DirectoryInfo(folder);

			var files = di.GetFiles("*.dll");
			foreach (var file in files)
			{
				Assembly assembly;
				try
				{
					assembly = Assembly.LoadFile(file.FullName);
				}
				catch (Exception e)
				{
					Console.WriteLine(file + " - ERROR: " + e.Message);
					continue;
				}
				if (!assembly.ImageRuntimeVersion.StartsWith("v4."))
					Console.WriteLine(file + " - Version " + assembly.ImageRuntimeVersion);
			}
			MessageBox.Show("Finished.");
		}
	}
}
