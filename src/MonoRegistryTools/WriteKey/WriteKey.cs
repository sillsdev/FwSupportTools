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
			Console.Error.WriteLine("Write Usage:\n");
			Console.Error.WriteLine("WriteKey Mode Location Key Value\n");
			Console.Error.WriteLine("Mode One of:");
			Console.Error.WriteLine("CU - CurrentUser");
			Console.Error.WriteLine("LM - LocalMachine");
			Console.Error.WriteLine("CR - ClassesRoot");
			Console.Error.WriteLine("U - Users");
			Console.Error.WriteLine("PD - PerformanceData");
			Console.Error.WriteLine("CC - CurrentConfig");
			Console.Error.WriteLine("DD - DynData\n");
			Console.Error.WriteLine("For Example: WriteKey LM \"Software\\SIL\" \"Icu36Dir \" \"/usr/local/FieldWorks/DistFiles/Icu36\" ");

			string doublequote = "\"";
			string backslash = "\\";

			Console.Error.WriteLine();
			Console.Error.WriteLine(String.Format(".NET cmd processing treats {1}{0} as a single {0}, not part of a delimiter.", doublequote, backslash));
			Console.Error.WriteLine(String.Format("This can mess up closing {0} delimiters when the string ends with {1}.", doublequote, backslash));
			Console.Error.WriteLine(String.Format("To get around this, you need to add an extra {1} to the end.  {0}D:{1}{0}  -> D:{0}     {0}D:{1}{1}{0} -> D:{1}", doublequote, backslash));
			Console.Error.WriteLine(String.Format("Cmd line with 4 args: {0}Software{1}SIL{1}{0}7.0{0} {0}Projects{1}{1}Dir{1}{0} {0}I:{1}{0} {0}e:{1}{1}{0}", doublequote, backslash));
			Console.Error.WriteLine(String.Format("Interpreted as 3 args: 1){0}Software{1}{1}SIL{1}{1}FieldWorks{1}{0}7.0{0}  2){0}Projects{1}{1}{1}{1}Dir{1}{0} I:{1}{0}{0}  3){0}e:{1}{1}{0}", doublequote, backslash));
		}
	}
}
