// ---------------------------------------------------------------------------------------------
#region // Copyright (c) 2010, SIL International. All Rights Reserved.
// <copyright from='2010' to='2010' company='SIL International'>
//		Copyright (c) 2010, SIL International. All Rights Reserved.
//
//		Distributable under the terms of either the Common Public License or the
//		GNU Lesser General Public License, as specified in the LICENSING.txt file.
// </copyright>
#endregion
//
// File: Program.cs
// Responsibility: mcconnel
//
// <remarks>
// </remarks>
// ---------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;

namespace SIL.FieldWorks.src.GenerateFwSchema
{
	/// ----------------------------------------------------------------------------------------
	/// <summary>
	/// This program is used to generate a Relax NG schema from the master model file.
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	class Program
	{
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// The main entry point
		/// </summary>
		/// ------------------------------------------------------------------------------------
		static void Main(string[] args)
		{
			bool fOld = false;
			string sModelFile = null;
			string sSchemaFile = null;
			if (args.Length == 3)
			{
				switch (args[0])
				{
					case "--old":
						fOld = true;
						sModelFile = args[1];
						sSchemaFile = args[2];
						break;
					default:
						break;
				}
			}
			else if (args.Length == 2)
			{
				sModelFile = args[0];
				sSchemaFile = args[1];
			}
			if (String.IsNullOrEmpty(sModelFile) || String.IsNullOrEmpty(sSchemaFile))
			{
				ShowUsage();
			}
			else
			{
				FwModel model = new FwModel(sModelFile);
				if (sSchemaFile.ToLowerInvariant().EndsWith(".rng"))
					model.WriteRngSchema(sSchemaFile, fOld);
				else if (sSchemaFile.ToLowerInvariant().EndsWith(".dtd"))
					model.WriteDtd(sSchemaFile, fOld);
				else
					Console.WriteLine("Sorry, I only know how to generate DTD and RNG schema files.");
			}
		}

		private static void ShowUsage()
		{
			Console.WriteLine("Usage: GenerateFwSchema [--old] <modelFile> <schemaFile>");
			Console.WriteLine("If schemaFile ends with .rng, then a Relax NG schema is generated.");
			Console.WriteLine("If schemaFile ends with .dtd, then a DTD file is generated.");
		}
	}
}
