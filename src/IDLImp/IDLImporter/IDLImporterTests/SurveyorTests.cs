// --------------------------------------------------------------------------------------------
// 	Copyright (c) 2009,  SIL International. All Rights Reserved.
//
// 	Distributable under the terms of either the Common Public License or the
// 	GNU Lesser General Public License, as specified in the LICENSING.txt file.
//
// File: SurveyorTests.cs
// Author: Eberhard Beilharz
//
// <remarks>
// </remarks>
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Text;
using NUnit.Framework;
using antlr;

namespace SIL.FieldWorks.Tools
{

	/// ----------------------------------------------------------------------------------------
	/// <summary>
	/// Tests for SurveyorTags.g
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	[TestFixture]
	public class SurveyorTests
	{
		private StringBuilder m_Bldr;
		private StringReader m_Reader;

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Creates the parser.
		/// </summary>
		/// <param name="input">The input.</param>
		/// <returns>A SurveyorParser</returns>
		/// ------------------------------------------------------------------------------------
		private SurveyorParser CreateParser(string input)
		{
			m_Reader = new StringReader(input);
			m_Bldr = new StringBuilder();
			SurveyorLexer lexer = new SurveyorLexer(m_Bldr, m_Reader);
			return new SurveyorParser(m_Bldr, lexer);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Tests the http rule.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Test]
		public void Http()
		{
			var parser = CreateParser("Before @HTTP{intranet.sil.org/file.html} after\n");
			parser.http();

			Assert.AreEqual("Before <see href=\"intranet.sil.org/file.html\">intranet.sil.org/file.html</see>",
							m_Bldr.ToString());
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Tests the reference rule.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Test]
		public void Reference()
		{
			var parser = CreateParser("${Reference}");
			parser.reference();

			Assert.AreEqual("<c>Reference</c>", m_Bldr.ToString());
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Tests the reference rule for a reference that contains a class name.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Test]
		public void ReferenceWithClassName()
		{
			var parser = CreateParser("${Class#Method}");
			parser.reference();

			Assert.AreEqual("<c>Class.Method</c>", m_Bldr.ToString());
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Tests the table rule.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Test]
		public void Table()
		{
			var parser = CreateParser("@table{" +
									  "@row{" +
									  " @cell{Cell 1 of row 1}" +
									  " @cell{Cell 2 of row 1}" +
									  "}" +
									  "@row{" +
									  " @cell{Cell 1 of row 2}" +
									  " @cell{Cell 2 of row 2}" +
									  "}}");
			parser.table();

			Assert.AreEqual(string.Format("<list type=\"table\">  " +
							"<item><term>Cell 1 of row 1</term>{0} <description>{0} Cell 2 of row 1{0} </description>{0} </item>  " +
							"<item><term>Cell 1 of row 2</term>{0} <description>{0} Cell 2 of row 2{0} </description>{0} </item>" +
							"</list>", Environment.NewLine), m_Bldr.ToString());
		}
	}
}
