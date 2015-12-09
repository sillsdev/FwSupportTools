// Copyright (c) 2007-2015 SIL International
// This software is licensed under the LGPL, version 2.1 or later
// (http://www.gnu.org/licenses/lgpl-2.1.html)

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CSharp;

namespace SIL.FieldWorks.Tools
{
	/// ----------------------------------------------------------------------------------------
	/// <summary>
	///
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	public class CSharpCodeProviderEx: CSharpCodeProvider
	{
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets an instance of the C# code generator.
		/// </summary>
		/// <returns>
		/// An instance of the C# <see cref="T:System.CodeDom.Compiler.ICodeGenerator"></see> implementation.
		/// </returns>
		/// ------------------------------------------------------------------------------------
		[ObsoleteAttribute("Callers should not use the ICodeGenerator interface and should instead use the methods directly on the CodeDomProvider class.")]
		public override System.CodeDom.Compiler.ICodeGenerator CreateGenerator()
		{
			return new CSharpCodeGenerator();
		}
	}
}
