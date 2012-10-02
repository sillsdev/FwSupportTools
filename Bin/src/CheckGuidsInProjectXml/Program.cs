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

namespace SIL.FieldWorks.src.CheckGuidsInProjectXml
{
	/// ----------------------------------------------------------------------------------------
	/// <summary>
	/// This program scans a FieldWorks XML project file to crosscheck all the guid references.
	/// This is similar in purpose to the ID/IDREF checking done by DTD validation.
	/// Guids aren't always valid ID strings, and thus can't be checked by DTD/RNG validation.
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
			if (args.Length == 1)
			{
				GuidCheck checker = new GuidCheck(args[0]);
				checker.WriteBadReferences();
			}
			else
			{
				Console.WriteLine("Usage: CheckGuidsInProjectXml <projectXmlFile>");
			}
		}
	}
}
