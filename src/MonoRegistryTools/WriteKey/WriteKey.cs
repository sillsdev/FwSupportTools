// WriteKey.cs created with MonoDevelop
// User: hindlet at 1:56 P 19/08/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using Microsoft.Win32;

namespace MonoRegistryTools
{


	public class WriteKey
	{

		public WriteKey()
		{
		}

		public static int Main(string[] args)
		{
			bool success = false;

			if (args == null || args.Length != 4)
			{
					PrintUsage();
					return 0;
			}

// .NET cmd processing treats \" as a single ", not part of a delimiter.
// This can mess up closing " delimiters when the string ends with backslash.
// To get around this, you need to add an extra \ to the end.  "D:\"  -> D:"     "D:\\" -> D:\
// Cmd line with 4 args: "Software\SIL\"7.0" "Projects\\Dir\" "I:\" "e:\\"
// Interpreted as 3 args: 1)"Software\\SIL\\FieldWorks\"7.0"  2)"Projects\\\\Dir\" I:\""  3)"e:\\"

			if (args[0] == "CU")
				success = WriteKeyString(Registry.CurrentUser, args[1], args[2], args[3]);

			else if (args[0] == "LM")
				success = WriteKeyString(Registry.LocalMachine, args[1], args[2], args[3]);

			else if (args[0] == "CR")
				success = WriteKeyString(Registry.ClassesRoot, args[1], args[2], args[3]);

			else if (args[0] == "CU")
				success = WriteKeyString(Registry.Users, args[1], args[2], args[3]);

			else if (args[0] == "PD")
				success = WriteKeyString(Registry.PerformanceData, args[1], args[2], args[3]);

			else if (args[0] == "CC")
				success = WriteKeyString(Registry.CurrentConfig, args[1], args[2], args[3]);

			else if (args[0] == "DD")
				success = WriteKeyString(Registry.DynData, args[1], args[2], args[3]);
			else
			{
				PrintUsage();
				return 0;
			}



			if (!success)
				Console.WriteLine("Error: Could not write the value");
			else
				Console.WriteLine(args[3] + ": Written succesfully");

			return 0;
		}

		public static bool WriteKeyString(RegistryKey k, string location, string key, string val)
		{
			RegistryKey loc = k.CreateSubKey(location);

			if (loc == null)
			{
					return false;
			}

			loc.SetValue(key, val);

			k.Close();
			return true;
		}

		public static void PrintUsage()
		{
			Console.WriteLine("Write Usage:\n");
			Console.WriteLine("WriteKey Mode Location Key Value\n");
			Console.WriteLine("Mode One of:");
			Console.WriteLine("CU - CurrentUser");
			Console.WriteLine("LM - LocalMachine");
			Console.WriteLine("CR - ClassesRoot");
			Console.WriteLine("U - Users");
			Console.WriteLine("PD - PerformanceData");
			Console.WriteLine("CC - CurrentConfig");
			Console.WriteLine("DD - DynData\n");
			Console.WriteLine("For Example: WriteKey LM \"Software\\SIL\" \"Icu36Dir \" \"/usr/local/FieldWorks/DistFiles/Icu36\" ");

		}
	}
}
