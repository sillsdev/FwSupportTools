// Copyright (c) 2013, SIL International. All Rights Reserved.
//
// Distributable under the terms of either the Common Public License or the
// GNU Lesser General Public License, as specified in the LICENSING.txt file.
//
// Original author: TomH 2008-08-19 ReadKey.cs

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
					return 2;
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
				return 2;
			}

			if (!success)
			{
				Console.Error.WriteLine("Error: Could not read key");
				return 1;
			}

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
			Console.Error.WriteLine("Usage: ReadKey.exe MODE LOCATION KEY");
			Console.Error.WriteLine("Prints registry key.");
			Console.Error.WriteLine("Example: ReadKey.exe LM \"Software/SIL/FieldWorks/9\" \"ProjectsDir\"");
			Console.Error.WriteLine();
			Console.Error.WriteLine("MODE is one of:");
			Console.Error.WriteLine("  CU - CurrentUser");
			Console.Error.WriteLine("  LM - LocalMachine");
			Console.Error.WriteLine("  CR - ClassesRoot");
			Console.Error.WriteLine("  U  - Users");
			Console.Error.WriteLine("  PD - PerformanceData");
			Console.Error.WriteLine("  CC - CurrentConfig");
			Console.Error.WriteLine("  DD - DynData");
			Console.Error.WriteLine();
			Console.Error.WriteLine("Exit status:");
			Console.Error.WriteLine("  0  if read successfully");
			Console.Error.WriteLine("  1  if there was an error reading the key");
			Console.Error.WriteLine("  2  for invalid arguments");
		}
	}
}
