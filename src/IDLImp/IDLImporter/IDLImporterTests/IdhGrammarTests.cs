// ---------------------------------------------------------------------------------------------
#region // Copyright (c) 2009, SIL International. All Rights Reserved.
// <copyright from='2009' to='2009' company='SIL International'>
//		Copyright (c) 2009, SIL International. All Rights Reserved.
//
//		Distributable under the terms of either the Common Public License or the
//		GNU Lesser General Public License, as specified in the LICENSING.txt file.
// </copyright>
#endregion
//
// File: IdhGrammarTests.cs
// Responsibility: EberhardB
//
// <remarks>
// </remarks>
// ---------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;
using antlr;

namespace SIL.FieldWorks.Tools
{
	/// ----------------------------------------------------------------------------------------
	/// <summary>
	///
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	[TestFixture]
	public class IdhGrammarTests
	{
		private StringReader m_Reader;

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Creates the parser.
		/// </summary>
		/// <param name="input">The input.</param>
		/// <returns></returns>
		/// ------------------------------------------------------------------------------------
		private IdhParser CreateParser(string input)
		{
			m_Reader = new StringReader(input);
			var lexer = new IdhLexer(m_Reader);
			return new IdhParser(lexer);
		}

		///--------------------------------------------------------------------------------------
		/// <summary>
		/// Tests the 'comment' rule
		/// </summary>
		///--------------------------------------------------------------------------------------
		[Test]
		public void Comment()
		{
			var parser = CreateParser("// Comment\nbla\n");
			//var ret = parser.idhfile();
			Assert.AreEqual("// Comment\n", parser.comment());
		}

		///--------------------------------------------------------------------------------------
		/// <summary>
		/// Tests the 'comment' rule for a multiline comment
		/// </summary>
		///--------------------------------------------------------------------------------------
		[Test]
		public void Comment_Multiline()
		{
			var parser = CreateParser("/* Comment\nbla\n*/");
			//var ret = parser.idhfile();
			Assert.AreEqual("/* Comment\nbla\n*/", parser.comment());
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Tests rules for declaring an interface preceded by a preprocessor directive
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Test]
		public void Interface_Preprocessor()
		{
			var parser = CreateParser("	#define HVO long // comment\n" +
				"/* Comment for interface */\n" +
				"DeclareInterface(Interface, Unknown, 6C456541-C2B6-11d3-8078-0000C0FB81B5) { /* empty */ }");

			var info = parser.idhfile();
			Assert.AreEqual(1, info.Children.Count);
			Assert.AreEqual("/* Comment for interface */", info.Children["Interface"].Comment);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Tests rules for declaring an interface preceded by a typedef directive
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Test]
		public void Interface_Typedef()
		{
			var parser = CreateParser("	typedef int HVO; // comment\n" +
				"/* Comment for interface */\n" +
				"DeclareInterface(Interface, Unknown, 6C456541-C2B6-11d3-8078-0000C0FB81B5) { /* empty */ }");

			var info = parser.idhfile();
			Assert.AreEqual(2, info.Children.Count);
			Assert.AreEqual("", info.Children["HVO"].Comment);
			Assert.AreEqual("/* Comment for interface */", info.Children["Interface"].Comment);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Tests rules for declaring an enum typedef
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Test]
		public void Typedef_Enum()
		{
			var parser = CreateParser("	typedef [v1_enum] enum Enum\n" +
				"{" +
				"kscBackspace = 8," +
				"} EnumName;");

			var info = new Dictionary<string, IdhCommentProcessor.CommentInfo>();
			parser.typedef(info);
			Assert.AreEqual(1, info.Count);
			Assert.AreEqual("", info["EnumName"].Comment);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Tests rules for declaring a struct typedef
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Test]
		public void Typedef_Struct()
		{
			var parser = CreateParser("	typedef struct Bla\n" +
				"{" +
				"int m_bla;" +
				"} StructName;");

			var info = new Dictionary<string, IdhCommentProcessor.CommentInfo>();
			parser.typedef(info);
			Assert.AreEqual(1, info.Count);
			Assert.AreEqual("", info["StructName"].Comment);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Tests rules for declaring a type
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Test]
		public void Typedef_Int()
		{
			var parser = CreateParser("	typedef int HVO; // comment\n");

			var info = new Dictionary<string, IdhCommentProcessor.CommentInfo>();
			parser.typedef(info);
			Assert.AreEqual(1, info.Count);
			Assert.AreEqual("", info["HVO"].Comment);
		}
	}
}
