// Copyright (c) 2013, SIL International. All Rights Reserved.
//
// Distributable under the terms of either the Common Public License or the
// GNU Lesser General Public License, as specified in the LICENSING.txt file.
//
// Original author: TomH 2008-08-19 WriteKey.cs

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
					return 2;
			}

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
				return 2;
			}

			if (!success)
			{
				Console.Error.WriteLine("Error: Could not write the value");
				return 1;
			}

			Console.WriteLine(args[3] + ": Written successfully");
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
			Console.Error.WriteLine("Usage: WriteKey.exe MODE LOCATION KEY VALUE");
			Console.Error.WriteLine("Writes registry key.");
			Console.Error.WriteLine("Example: WriteKey.exe LM \"Software/SIL/FieldWorks/9\" \"ProjectsDir\" \"$HOME/.local/share/fieldworks/Projects\"");
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
			Console.Error.WriteLine("  1  if there was an error writing the key");
			Console.Error.WriteLine("  2  for invalid arguments");

			string doublequote = "\"";
			string backslash = "\\";

			Console.Error.WriteLine();
			Console.Error.WriteLine($"Note: Be aware that .NET command processing converts {backslash}{doublequote} to a literal {doublequote}");
			Console.Error.WriteLine($"  So if your string ends with a backslash, you will want to escape it and");
			Console.Error.WriteLine($"  other backslashes. For example, change {doublequote}C:{backslash}Projects{backslash}{doublequote} to {doublequote}C:{backslash}{backslash}Projects{backslash}{backslash}{doublequote}");
		}
	}
}
