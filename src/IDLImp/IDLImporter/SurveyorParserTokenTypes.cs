// $ANTLR 2.7.7 (20060930): "SurveyorTags.g" -> "SurveyorParser.cs"$

// Copyright (c) 2007-2015 SIL International
// This software is licensed under the LGPL, version 2.1 or later
// (http://www.gnu.org/licenses/lgpl-2.1.html)

// Defines the grammar for processing some Surveyor tags.

#pragma warning disable 0618,0219, 0162

using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace SIL.FieldWorks.Tools
{
	public class SurveyorParserTokenTypes
	{
		public const int EOF = 1;
		public const int NULL_TREE_LOOKAHEAD = 3;
		public const int DOLLAR = 4;
		public const int LBRACE = 5;
		public const int IDENTIFIER = 6;
		public const int TABLE = 7;
		public const int RBRACE = 8;
		public const int ROW = 9;
		public const int CELL = 10;
		public const int HTTP = 11;
		public const int REFERENCE = 12;
		public const int POUND = 13;
		public const int DIGIT = 14;
		public const int LETTER = 15;
		public const int WS = 16;
		public const int IGNORE = 17;
		
	}
}
