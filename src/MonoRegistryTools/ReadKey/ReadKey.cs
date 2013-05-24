// ReadKey.cs created with MonoDevelop
// User: hindlet at 1:43 P 19/08/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using Microsoft.Win32;

namespace MonoRegistryTools
{

	public class ReadKey
	{

		public ReadKey()
		{
		}

		public static int Main(string[] args)
		{
			bool success = false;
			string val = "";

			if (args == null || args.Length != 3)
			{
					PrintUsage();
					return 0;
			}

			if (args[0] == "CU")
				success = ReadKeyString(Registry.CurrentUser, args[1], args[2], out val);

			else if (args[0] == "LM")
				success = ReadKeyString(Registry.LocalMachine, args[1], args[2], out val);

			else if (args[0] == "CR")
				success = ReadKeyString(Registry.ClassesRoot, args[1], args[2], out val);

			else if (args[0] == "CU")
				success = ReadKeyString(Registry.Users, args[1], args[2], out val);

			else if (args[0] == "PD")
				success = ReadKeyString(Registry.PerformanceData, args[1], args[2], out val);

			else if (args[0] == "CC")
				success = ReadKeyString(Registry.CurrentConfig, args[1], args[2], out val);

			else if (args[0] == "DD")
				success = ReadKeyString(Registry.DynData, args[1], args[2], out val);

			else
			{
				PrintUsage();
				return 0;
			}

			if (!success)
				Console.WriteLine("Error: Could not read key");
			else
				Console.WriteLine(val);

			return 0;
		}

		public static bool ReadKeyString(RegistryKey k, string location, string key, out string rv)
		{
			RegistryKey loc = k.OpenSubKey(location);

			if (loc == null)
			{
					rv = null;
					return false;
			}

			rv = (string)loc.GetValue(key);

			k.Close();

			return rv != null;
		}



		public static void PrintUsage()
		{
			Console.WriteLine("ReadKey Usage:\n");
			Console.WriteLine("ReadKey Mode Location Key\n");
			Console.WriteLine("Mode One of:");
			Console.WriteLine("CU - CurrentUser");
			Console.WriteLine("LM - LocalMachine");
			Console.WriteLine("CR - ClassesRoot");
			Console.WriteLine("U - Users");
			Console.WriteLine("PD - PerformanceData");
			Console.WriteLine("CC - CurrentConfig");
			Console.WriteLine("DD - DynData\n");
			Console.WriteLine("For Example: ReadKey LM \"Software\\SIL\" \"Icu36Dir\" ");
		}


	}


}
