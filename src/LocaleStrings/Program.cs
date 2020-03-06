using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Win32;

using SIL.FieldWorks.Common.Utils;

namespace LocaleStrings
{
	public class Program
	{
		/// <summary>
		/// These are the various ways that newlines may be represented in the input strings.
		/// </summary>
		static readonly string[] s_rgsNewline = { "\r\n", "\n" };
		/// <summary>
		/// These are the values that must be quoted by a backslash in a quoted string.
		/// </summary>
		static readonly char[] s_rgchQuoted = { '"', '\\' };
		/// <summary>
		/// Flag that a string in a resx file contains "\\t", "\\r", "\\n", or "\\\"".
		/// These shouldn't be quoted in an XML file!
		/// </summary>
		static bool s_fBadStringValue;
		/// <summary>
		/// DistFiles directory used on developer machine, but missing on end-user machines.
		/// We want to be able to run this on end-user machines to build the strings-???.xml file.
		/// </summary>
		static string s_DistFiles = "";		// cannot be null -- used in Path.Combine below.

		/// <summary>
		/// This program either extracts localizable strings from the files in the FieldWorks
		/// source tree into a POT file, or stores a localized strings-xx.xml file based on a
		/// PO file, or creates a test localization PO file.
		/// </summary>
		static void Main(string[] args)
		{
			try
			{
				var bpExtract = new CommandLineOptions.BoolParam("x",
					"extract",
					"Extract localizable strings into a POT file.",
					false);
				var bpStore = new CommandLineOptions.BoolParam("s",
					"store",
					"Store strings from the PO file into the Language Explorer strings-xx.xml file",
					false);
				var spRoot = new CommandLineOptions.StringParam("r",
					"root",
					"The root directory of the FieldWorks source tree (typically C:\\fwrepo\\fw or ~/fwrepo/fw)",
					null);
				var spPOTFile = new CommandLineOptions.StringParam("p",
					"pot",
					"Name of the output POT file",
					"FieldWorks.pot");
				var spTest = new CommandLineOptions.StringParam("t",
					"test",
					"Generate a test PO file for the given locale",
					"en-FOO");
				var bpForce = new CommandLineOptions.BoolParam("",
					"force",
					"Overwrite existing (test?) PO file",
					false);
				var rgsDirs = new List<string>();
				rgsDirs.Add("Src");
				var slpDirs = new CommandLineOptions.StringListParam("d",
					"dir",
					"Add a subdirectory to those searched under the root (default = Src)",
					rgsDirs);
				var spImport = new CommandLineOptions.StringParam("i",
					"import",
					"Import translations from the PO file into the POT file, storing them in a new PO file.",
					"messages.xx.po");
				var spMerge = new CommandLineOptions.StringParam("m",
					"merge",
					"Merge PO files together, overwriting the first one.",
					"messages.xx.po");
				var spUpdate = new CommandLineOptions.StringParam("u",
					"update",
					"Update a PO file.",
					"messages.xx.po");
				var bpCheck = new CommandLineOptions.BoolParam("c",
					"check",
					"Check translated strings for matching/valid argument markers.",
					false);

				var rgParam = new CommandLineOptions.Param[] {
					bpExtract, bpStore, spRoot, spPOTFile, spTest,
					bpForce, slpDirs, spImport, spMerge, bpCheck, spUpdate
				};
				bool fOk = CommandLineOptions.Parse(args, ref rgParam, out var iOpt);
				int cOps = 0;
				if (bpExtract.Value)
					++cOps;
				if (bpStore.Value)
					++cOps;
				if (spImport.HasValue)
					++cOps;
				if (spMerge.HasValue)
					++cOps;
				if (spTest.HasValue)
					++cOps;
				if (bpCheck.Value)
					++cOps;
				if (spUpdate.HasValue)
					++cOps;
				if (cOps != 1)
					fOk = false;
				if (!fOk)
				{
					Usage(null, rgParam);
					return;
				}

				string sRoot = null;
				if (spRoot.HasValue)
				{
					sRoot = spRoot.Value;
					string devPath = Path.Combine(sRoot, "DistFiles");
					if (Directory.Exists(devPath))
						s_DistFiles = "DistFiles";
				}
				else if (bpExtract.Value|| bpStore.Value || spTest.HasValue)
					sRoot = GetRootDir();

				if (bpExtract.Value)
				{
					string sOutputFile = spPOTFile.Value;
					if (!spPOTFile.HasValue)
					{
						sOutputFile = "Flex.pot";
					}
					ExtractPOTFile(sRoot, rgsDirs, sOutputFile);
				}
				else if (bpStore.Value)
				{
					if (iOpt >= args.Length)
						Usage(null, rgParam);
					else if (!File.Exists(args[iOpt]))
						Usage($"{args[iOpt]} does not exist!", rgParam);
					else
						StoreLocalizedStrings(args[iOpt], sRoot);
				}
				else if (spTest.HasValue)
				{
					string sLocale = spTest.Value;
					if (VerifyValidLocale(sLocale))
					{
						string sBase = Path.Combine(sRoot, "Localizations");
						string sOutputFile = Path.Combine(sBase, $"messages.{sLocale}.po");
						if (File.Exists(sOutputFile) && !bpForce.Value)
						{
							Usage($"{sOutputFile} already exists.  Use --force to overwrite", rgParam);
						}
						else
						{
							GenerateTestPOFile(sOutputFile, sRoot, rgsDirs, sLocale);
						}
					}
					else
					{
						Usage($"{sLocale} is not a valid system locale", rgParam);
					}
				}
				else if (spImport.HasValue)
				{
					if (iOpt > args.Length - 2)
						Usage("Missing POT or target PO file", rgParam);
					var potFile = args[iOpt];
					var outFile = args[iOpt + 1];
					if (!File.Exists(spImport.Value))
						Usage($"Invalid source file: {spImport.Value}", rgParam);
					if (!File.Exists(potFile))
						Usage($"Invalid POT file {potFile}", rgParam);
					ImportPOTranslations(spImport.Value, potFile, outFile);
				}
				else if (spMerge.HasValue)
				{
					if (iOpt >= args.Length)
						Usage("Missing second merge file", rgParam);
					else if (!File.Exists(spMerge.Value))
						Usage($"Invalid first merge file {spMerge.Value}", rgParam);
					else if (!File.Exists(args[iOpt]))
						Usage($"Invalid second merge file {args[iOpt]}", rgParam);
					else
						MergePOFiles(spMerge.Value, args[iOpt]);
				}
				else if (bpCheck.Value)
				{
					if (iOpt >= args.Length)
						Usage(null, rgParam);
					else if (!File.Exists(args[iOpt]))
						Usage($"{args[iOpt]} does not exist!", rgParam);
					else
						CheckLocalizedStrings(args[iOpt]);
				}
				else if (spUpdate.HasValue)
				{
					if (iOpt >= args.Length)
						Usage("Missing POT file for update", rgParam);
					else if (!File.Exists(spUpdate.Value))
						Usage($"PO file {spUpdate.Value} for update does not exist!", rgParam);
					else if (!File.Exists(args[iOpt]))
						Usage($"POT file {args[iOpt]} for update does not exist!", rgParam);
					else
						UpdatePoFile(spUpdate.Value, args[iOpt]);
				}
				else
				{
					Usage("Invalid command", rgParam);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}

		/// <summary>
		/// Tersely explain how to use the program.
		/// </summary>
		private static void Usage(string sMsg, CommandLineOptions.Param[] rgParam)
		{
			if (!string.IsNullOrEmpty(sMsg))
			{
				Console.WriteLine(sMsg);
				Console.WriteLine("");
			}
			Console.WriteLine("Usage: LocaleStrings.exe --extract [options]");
			Console.WriteLine("    or");
			Console.WriteLine("       LocaleStrings.exe --store [options] messages.<xx>.po");
			Console.WriteLine("    or");
			Console.WriteLine("       LocaleStrings.exe --check [options] messages.<xx>.po");
			Console.WriteLine("    or");
			Console.WriteLine("       LocaleStrings.exe --test lg-co");
			Console.WriteLine("    or");
			Console.WriteLine("       LocaleStrings.exe [options] --import <old>.<xx>.po messages.pot <new>.<xx>.po");
			Console.WriteLine("    or");
			Console.WriteLine("       LocaleStrings.exe [options] --merge <old>.<xx>.po <new>.<xx>.po");
			Console.WriteLine("    or");
			Console.WriteLine("       LocaleStrings.exe [options] --update messages.<xx>.po messages.pot");
			Console.WriteLine("");
			CommandLineOptions.Usage(rgParam);
			Console.WriteLine("");
			Console.WriteLine("The --extract command creates a file (default FieldWorks.pot in the current");
			Console.WriteLine("directory) by extracting localizable strings from the C# resource files in the");
			Console.WriteLine("FieldWorks source tree.  It also extracts localizable strings from the XML");
			Console.WriteLine("configuration files in DistFiles/Language Explorer.  Plans are to also extract strings");
			Console.WriteLine("from the C/C++ resource (.rc) files, although that may not be done very soon.");
			Console.WriteLine("");
			Console.WriteLine("The --store command creates a file named strings-xx.xml from the messages.xx.po");
			Console.WriteLine("file, copying the existing strings-en.xml file, substituting translated strings");
			Console.WriteLine("for existing ones, and adding translated strings for the various label and abbr");
			Console.WriteLine("attributes in the other XML configuration files.");
			Console.WriteLine("");
			Console.WriteLine("The --check command reads through the messages.xx.po file to check that all the");
			Console.WriteLine("translated messages have valid argument markers matching the original strings.");
			Console.WriteLine("Problems are written to the standard output, and thus may be redirected to a file.");
			Console.WriteLine("");
			Console.WriteLine("The --test command create a messages.lg-co.po file which can be used to test");
			Console.WriteLine("the internationalization of the FieldWorks programs.");
			Console.WriteLine("");
			Console.WriteLine("The --import command imports translations from <old>.<xx>.po into messages.pot,");
			Console.WriteLine("preserving comments and also untranslated msgid strings from messages.pot,");
			Console.WriteLine("and storing the result in <new>.<xx>.po");
			Console.WriteLine("");
			Console.WriteLine("The --merge command merges the strings from <new>.<xx>.po into <old>.<xx>.po,");
			Console.WriteLine("overwriting any conflicts in the original (<old>.<xx>.po) file.");
			Console.WriteLine("");
			Console.WriteLine("The --update command merges the strings from messages.pot into messages.<xx>.po,");
			Console.WriteLine("replacing all the commments in messages.<xx>.po and adding any missing msgid strings.");

			throw new Exception("Invalid command arguments.");
		}

		/// <summary>
		/// Extract all the localizable strings from the C# resource (.resx) files in the
		/// FieldWorks source tree, the XML configuration files in the FieldWorks distribution
		/// tree, and (eventually) the C/C++ resource (.rc) files in the FieldWorks source
		/// tree, creating a POT file for the FieldWorks suite.
		/// </summary>
		/// <param name="sRoot"></param>
		/// <param name="rgsDirs"></param>
		/// <param name="sOutputFile"></param>
		private static void ExtractPOTFile(string sRoot, List<string> rgsDirs,
			string sOutputFile)
		{
			List<POString> rgsPOStrings = ExtractLocalizableStrings(sRoot, rgsDirs);
			StreamWriter swOut = null;
			try
			{
				swOut = new StreamWriter(sOutputFile, false, Encoding.UTF8);
				WritePotFile(swOut, sRoot, rgsPOStrings);
			}
			finally
			{
				if (swOut != null)
					swOut.Close();
			}
		}

		/// <summary>
		/// Write a POT file to the given stream from the list of POString objects.
		/// </summary>
		internal static void WritePotFile(TextWriter swOut, string sRoot, List<POString> rgsPOStrings)
		{
			WritePoHeader(swOut, sRoot, string.Empty);
			for (int i = 0; i < rgsPOStrings.Count; ++i)
			{
				var poString = rgsPOStrings[i];
				if (poString.Flags == null || !poString.Flags.Contains("fuzzy"))
				{
					poString.Write(swOut);
				}
			}
		}

		/// <summary>
		/// Extract all the localizable strings from the C# resource (.resx) files in the
		/// FieldWorks source tree, the XML configuration files in the FieldWorks distribution
		/// tree, and (eventually) the C/C++ resource (.rc) files in the FieldWorks source
		/// tree.
		/// </summary>
		/// <param name="sRoot"></param>
		/// <param name="rgsDirs"></param>
		/// <returns>Sorted and merged list of localizable strings</returns>
		private static List<POString> ExtractLocalizableStrings(string sRoot, List<string> rgsDirs)
		{
			List<POString> rgsPOStrings = new List<POString>(1000);
			ExtractFromResxFiles(sRoot, rgsDirs, rgsPOStrings);
			ExtractFromXmlConfigFiles(sRoot, rgsPOStrings);

			rgsPOStrings.Sort(POString.CompareMsgIds);
			POString.MergeDuplicateStrings(rgsPOStrings);
			return rgsPOStrings;
		}

		/// <summary>
		/// Returns a string like "FieldWorks 4.0.1"
		/// </summary>
		/// <param name="sRoot"></param>
		/// <returns></returns>
		private static string GetFieldWorksVersion(string sRoot)
		{
			string sMajor = "?";
			string sMinor = "?";
			string sRevision = "?";
			string sFile = Path.Combine(sRoot, "Build/GlobalInclude.properties");
			if (sRoot == "/home/testing/fw" && !File.Exists(sFile))
				return "FieldWorks 1.2.3";
			XmlDocument xdoc = new XmlDocument();
			xdoc.Load(sFile);
			// I would prefer to use SelectSingleNode, it it doesn't seem to work!
			foreach (XmlNode xn in xdoc.DocumentElement.FirstChild.ChildNodes)
			{
				if (!(xn is XmlElement))
					continue;

				switch (xn.Name)
				{
					case "FWMAJOR": sMajor = xn.InnerText; break;
					case "FWMINOR": sMinor = xn.InnerText; break;
					case "FWREVISION": sRevision = xn.InnerText; break;
				}
			}
			return $"FieldWorks {sMajor}.{sMinor}.{sRevision}";
		}

		/// <summary>
		/// Extract localizable strings from the C# (.resx) files in the source tree.
		/// </summary>
		/// <param name="sRoot"></param>
		/// <param name="rgsDirs"></param>
		/// <param name="rgsPOStrings"></param>
		private static void ExtractFromResxFiles(string sRoot, List<string> rgsDirs,
			List<POString> rgsPOStrings)
		{
			for (int iDir = 0; iDir < rgsDirs.Count; ++iDir)
			{
				string sRootDir = Path.Combine(sRoot, rgsDirs[iDir]);
				string[] rgsLocResxFiles = FindAllFiles(sRootDir, "*.*.resx");
				List<string> lssLocResx = new List<string>(rgsLocResxFiles.Length);
				for (int i = 0; i < rgsLocResxFiles.Length; ++i)
					lssLocResx.Add(rgsLocResxFiles[i].ToLower());
				string[] rgsResxFiles = FindAllFiles(sRootDir, "*.resx");
				for (int i = 0; i < rgsResxFiles.Length; ++i)
				{
					string sFile = rgsResxFiles[i].ToLower();
					{
						if (sFile.IndexOf("\\te\\") >= 0)
							continue;
						if (sFile.IndexOf("\\tedll\\") >= 0)
							continue;
						if (sFile.IndexOf("\\teexe\\") >= 0)
							continue;
						if (sFile.IndexOf("\\scrfdo\\") >= 0)
							continue;
						if (sFile.IndexOf("\\scrimportcomponents\\") >= 0)
							continue;
						if (sFile.IndexOf("\\scripture\\") >= 0)
							continue;
						if (sFile.IndexOf("\\fdo\\scr") >= 0)
							continue;
					}					if (!lssLocResx.Contains(sFile))
						ProcessResxFile(rgsResxFiles[i], sRoot, rgsPOStrings);
				}
				if (s_fBadStringValue)
				{
					throw new Exception("FIX THE INVALID RESX STRINGS!");
				}
			}
		}

		/// <summary>
		/// Verify that the given locale string represents a valid system locale.
		/// </summary>
		/// <param name="sLoc">locale string</param>
		/// <returns>true of sLoc is valid, otherwise false</returns>
		private static bool VerifyValidLocale(string sLoc)
		{
			CultureInfo[] rgci = CultureInfo.GetCultures(CultureTypes.AllCultures);
			for (int i = 0; i < rgci.Length; ++i)
			{
				if (rgci[i].Name == sLoc)
					return true;
			}
			return false;
		}

		/// <summary>
		/// Load a single resource (.resx) file, possibly merging in string values from a
		/// localized version of the same, and write the strings found therein to the output
		/// (.po) file.
		/// </summary>
		/// <param name="sResxFile">path to the file</param>
		/// <param name="sRoot">leading part of file paths to ignore</param>
		/// <param name="rgsPOStrings"></param>
		private static void ProcessResxFile(string sResxFile, string sRoot, List<POString> rgsPOStrings)
		{
			var xdoc = new XmlDocument();
			xdoc.Load(sResxFile);
			var sAutoCommentFilePath = ComputeAutoCommentFilePath(sResxFile, sRoot);
			if (xdoc.DocumentElement != null)
				ProcessResxData(xdoc.DocumentElement, sAutoCommentFilePath, rgsPOStrings);
		}

		internal static string ComputeAutoCommentFilePath(string sFilePath, string sRoot)
		{
			string sIgnore = sRoot.Replace('\\', '/');
			if (sIgnore.EndsWith("/"))
				sIgnore = sIgnore.Substring(0, sIgnore.Length - 1);
			string sPath = sFilePath.Replace('\\', '/');
			if (sPath.StartsWith(sIgnore))
				sPath = sPath.Substring(sIgnore.Length);
			if (sPath.ToLowerInvariant().StartsWith("/distfiles"))
				sPath = sPath.Substring(10);
			return sPath;
		}

		/// <summary>
		/// Process the content from a single resource (.resx) file.
		/// </summary>
		internal static void ProcessResxData(XmlElement xdoc, string sAutoCommentFilePath, List<POString> rgsPOStrings)
		{
			foreach (XmlNode x in xdoc.ChildNodes)
			{
				XmlElement xel = x as XmlElement;
				if (xel != null && xel.Name == "data")
				{
					string sName = xel.GetAttribute("name");
					string sType = xel.GetAttribute("type");
					string sMimeType = xel.GetAttribute("mimetype");
					string sValue = null;
					string sComment = null;
					if (!string.IsNullOrEmpty(sName) &&
						string.IsNullOrEmpty(sType) && string.IsNullOrEmpty(sMimeType))
					{
						if (!sName.StartsWith(">>") && IsTextName(sName))
						{
							for (int i = 0; i < xel.ChildNodes.Count; ++i)
							{
								if (xel.ChildNodes[i].Name == "value")
									sValue = xel.ChildNodes[i].InnerText;
								if (xel.ChildNodes[i].Name == "comment")
									sComment = xel.ChildNodes[i].InnerText;
							}
						}
					}
					if (!string.IsNullOrEmpty(sValue))
					{
						if (sValue.IndexOfAny("/\\".ToCharArray()) >= 0 && sValue.Trim().EndsWith(".htm"))
							continue;
						if (sComment != null && sComment.Trim().ToLower() == "do not translate")
							continue;
						if (string.IsNullOrEmpty(sValue.Trim()))
							continue;
						if (sValue.IndexOf("\\n") >= 0 ||
							sValue.IndexOf("\\r") >= 0 ||
							sValue.IndexOf("\\t") >= 0 ||
							sValue.IndexOf("\\\\") >= 0 ||
							sValue.IndexOf("\\\"") >= 0)
						{
							s_fBadStringValue = true;
							Console.WriteLine(
								$"Backslash quoted character found for {sName} in {sAutoCommentFilePath}");
						}
						string[] rgsComment = null;
						if (!string.IsNullOrEmpty(sComment))
							rgsComment = sComment.Split(s_rgsNewline, StringSplitOptions.None);
						sValue = FixStringForEmbeddedQuotes(sValue);
						sValue = sValue.Replace(Environment.NewLine, "\\n" + Environment.NewLine);
						if (sValue.EndsWith(Environment.NewLine))
							sValue = sValue.Substring(0, sValue.Length - Environment.NewLine.Length);
						string[] rgsId = sValue.Split(s_rgsNewline, StringSplitOptions.None);
						string[] rgsStr = new string[1] { "" };
						POString pos = new POString(rgsComment, rgsId, rgsStr);
						string sPath = $"{sAutoCommentFilePath}::{sName}";
						pos.AddAutoComment(sPath);
						rgsPOStrings.Add(pos);
					}
				}
			}
		}

		private static bool IsTextName(string sName)
		{
			if (sName.EndsWith(".Name") ||
				sName.EndsWith(".AccessibleName") ||
				sName.EndsWith(".AccessibleDescription"))
			{
				return false;
			}
			else
			{
				return true;
			}
		}

		/// <summary>
		/// Write the header to the PO file.
		/// </summary>
		/// <param name="swOut">output FileStream</param>
		/// <param name="sRoot"></param>
		/// <param name="sLocale"></param>
		private static void WritePoHeader(TextWriter swOut, string sRoot, string sLocale)
		{
			swOut.WriteLine("");	// StreamWriter writes a BOM for UTF-8: put it on a line by itself.
			string sTime = DateTime.Now.ToLocalTime().ToString("o");
			string s = string.Format("Created from FieldWorks sources", sTime);
			POString.WriteComment(s, ' ', swOut);
			s = $"Copyright (c) {DateTime.Now.Year} SIL International";
			POString.WriteComment(s, ' ', swOut);
			POString.WriteComment("This software is licensed under the LGPL, version 2.1 or later",
				' ', swOut);
			POString.WriteComment("(http://www.gnu.org/licenses/lgpl-2.1.html)", ' ', swOut);
			POString.WriteComment("", ' ', swOut);
			//POString.WriteComment("fuzzy", ',', swOut);	the on-line translation site doesn't like this.
			swOut.Write("msgid ");
			POString.WriteQuotedLine("", swOut);
			swOut.Write("msgstr ");
			POString.WriteQuotedLine("", swOut);
			string sVersion = GetFieldWorksVersion(sRoot);
			POString.WriteQuotedLine($"Project-Id-Version: {sVersion}\\n", swOut);
			POString.WriteQuotedLine("Report-Msgid-Bugs-To: FlexErrors@sil.org\\n", swOut);
			POString.WriteQuotedLine($"POT-Creation-Date: {sTime}\\n", swOut);
			POString.WriteQuotedLine("PO-Revision-Date: \\n", swOut);
			POString.WriteQuotedLine("Last-Translator: Full Name <email@address>\\n", swOut);
			POString.WriteQuotedLine("Language-Team: Language <email@address>\\n", swOut);
			POString.WriteQuotedLine("MIME-Version: 1.0\\n", swOut);
			POString.WriteQuotedLine("Content-Type: text/plain; charset=UTF-8\\n", swOut);
			POString.WriteQuotedLine("Content-Transfer-Encoding: 8bit\\n", swOut);
			if (string.IsNullOrEmpty(sLocale))
			{
				POString.WriteQuotedLine("X-Poedit-Language: \\n", swOut);
				POString.WriteQuotedLine("X-Poedit-Country: \\n", swOut);
			}
			else
			{
				string[] rgsLocale = sLocale.Split(new char[] { '-', '_' });
				CultureInfo ci = new CultureInfo(rgsLocale[0]);
				string sLanguage = ci.EnglishName;
				string sCountry = string.Empty;
				string sVariant = string.Empty;
				if (rgsLocale.Length > 1)
				{
					RegionInfo ri = new RegionInfo(rgsLocale[1]);
					sCountry = ri.EnglishName;
				}
				if (rgsLocale.Length > 2)
				{
					ci = new CultureInfo(sLocale);
					sVariant = ci.EnglishName;
					int idx = sVariant.IndexOf(sLanguage);
					if (idx >= 0)
						sVariant = sVariant.Remove(idx, sLanguage.Length);
					idx = sVariant.IndexOf(sCountry);
					if (idx >= 0)
						sVariant = sVariant.Remove(idx, sCountry.Length);
					sVariant = sVariant.Replace('(', ' ');
					sVariant = sVariant.Replace(')', ' ');
					sVariant = sVariant.Trim(new char[] { ',', ' ', '\t', '\n', '\r' });
				}
				POString.WriteQuotedLine($"X-Poedit-Language: {sLanguage}\\n", swOut);
				POString.WriteQuotedLine($"X-Poedit-Country: {sCountry}\\n", swOut);
				if (!string.IsNullOrEmpty(sVariant))
					POString.WriteQuotedLine($"X-Poedit-Variant: {sVariant}\\n", swOut);
			}
			swOut.WriteLine("");
		}

		/// <summary>
		/// Add backslashes as needed to quote quotation marks and backslashes.
		/// </summary>
		/// <param name="sValue">string value which needs backslashes added</param>
		/// <returns>revised string (or possibly the original one)</returns>
		private static string FixStringForEmbeddedQuotes(string sValue)
		{
			// remove any current quoting of the quotation marks, which obviously isn't
			// needed in the XML based RESX files.
			int idx = sValue.IndexOf("\\\"");
			while (idx >= 0)
			{
				if (idx == 0 || sValue[idx - 1] != '\\')
					sValue = sValue.Remove(idx, 1);
				else
					++idx;
				if (idx < sValue.Length)
					idx = sValue.IndexOf("\\\"", idx);
				else
					break;
			}
			idx = sValue.IndexOfAny(s_rgchQuoted);
			while (idx >= 0)
			{
				sValue = sValue.Insert(idx, "\\");
				if (idx + 2 >= sValue.Length)
					break;
				idx = sValue.IndexOfAny(s_rgchQuoted, idx + 2);
			}
			return sValue;
		}

		/// <summary>
		/// Find the root directory of the FieldWorks source tree.
		/// </summary>
		/// <returns>path to the sources root (Src) in the FieldWorks source tree</returns>
		private static string GetRootDir()
		{
			string defaultDir = Environment.GetEnvironmentVariable("FWROOT") != null
				? Path.Combine(Environment.ExpandEnvironmentVariables("%FWROOT%"), "DistFiles")
				: null;
			object rootDir = null;
			var sRegKey = Registry.CurrentUser.OpenSubKey("Software\\SIL\\FieldWorks") ??
						Registry.LocalMachine.OpenSubKey("Software\\SIL\\FieldWorks");
			if (sRegKey != null)
			{
				rootDir = sRegKey.GetValue("RootCodeDir");
				if (rootDir == null)
				{
					sRegKey = sRegKey.OpenSubKey("7.0");
					rootDir = sRegKey.GetValue("RootCodeDir", defaultDir);
				}
			}
			if (rootDir == null || !(rootDir is string))
			{
				throw new ApplicationException("Cannot obtain FieldWorks root directory");
			}
			string sRootDir = (string)rootDir;
			int idx = sRootDir.ToLowerInvariant().LastIndexOf("distfiles", StringComparison.InvariantCulture);
			if (idx == sRootDir.Length - 9)
			{
				sRootDir = sRootDir.Substring(0, idx);
				s_DistFiles = "DistFiles";
			}
			return sRootDir;
		}

		/// <see cref="Usage"/>
		private static void ImportPOTranslations(string sourceFile, string templateFile, string outFile)
		{
			//var locale = GetLocaleFromMsgFile(sourceFile);

			var timeStamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
			Dictionary<string, POString> templateWithTranslations;
			POString posHeader;
			using (var sourceReader = new StreamReader(sourceFile, Encoding.UTF8))
			using (var templateReader = new StreamReader(templateFile, Encoding.UTF8))
			using (var swLog = new StreamWriter($"Import-{timeStamp}.log"))
			{
				templateWithTranslations = ImportTranslations(swLog, sourceReader, templateReader, sourceFile, templateFile, out posHeader);
			}

			// Don't overwrite an existing file; move it out of the way
			if (File.Exists(outFile))
			{
				File.Move(outFile, $"{outFile}-{timeStamp}.{Path.GetExtension(outFile)}");
			}
			using (var swOut = new StreamWriter(outFile))
			{
				WritePoFile(swOut, templateWithTranslations, posHeader, null);
			}
		}

		internal static Dictionary<string, POString> ImportTranslations(TextWriter log, TextReader source, TextReader template,
			string sourceFile, string templateFile, out POString poHeader)
		{
			var sources = ReadPoFile(source, out poHeader, out _);
			var translated = ReadPoFile(template, out var templateHeader, out _);
			MergePOHeaders(poHeader, templateHeader);
			foreach (var msgId in translated.Keys.ToList())
			{
				if (sources.TryGetValue(msgId, out var poString))
				{
					translated[msgId].MsgStr = poString.MsgStr;
				}
			}
			return translated;
		}

		/// <summary>
		/// Merges the PO files.
		/// </summary>
		private static void MergePOFiles(string sMainFile, string sNewFile)
		{
			string sLoc = GetLocaleFromMsgFile(sMainFile);
			string sLoc2 = GetLocaleFromMsgFile(sNewFile);
			if (sLoc != sLoc2)
			{
				Console.WriteLine("Mismatched locales in the two merge files: {0} vs {1}",
					sLoc, sLoc2);
				throw new Exception("MISMATCHED LOCALES IN MERGE FILES");
			}
			var timeStamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
			Dictionary<string, POString> dictMerged;
			POString posHeader;
			POString posObsolete;
			using (StreamReader srMain = new StreamReader(sMainFile, Encoding.UTF8))
			{
				using (StreamReader srNew = new StreamReader(sNewFile, Encoding.UTF8))
				{
					string sLogFile = $"Merge-{timeStamp}.log";
					using (StreamWriter swLog = new StreamWriter(sLogFile))
					{
						dictMerged = MergePoFileData(swLog, srMain, srNew, sMainFile, sNewFile, out posHeader, out posObsolete);
						swLog.Close();
					}
					srNew.Close();
				}
				srMain.Close();
			}
			// Save the original main file, and write out a new main file.
			File.Copy(sMainFile, $"{sMainFile}-{timeStamp}");
			using (StreamWriter swOut = new StreamWriter(sMainFile))
			{
				WritePoFile(swOut, dictMerged, posHeader, posObsolete);
				swOut.Close();
			}
		}

		private static void WritePoFile(TextWriter swOut, Dictionary<string, POString> dictMerged, POString posHeader, POString posObsolete)
		{
			posHeader.Write(swOut);
			using (Dictionary<string, POString>.Enumerator it = dictMerged.GetEnumerator())
			{
				while (it.MoveNext())
					it.Current.Value.Write(swOut);
			}
			if (posObsolete != null && posObsolete.UserComments != null)
				posObsolete.Write(swOut);
		}

		internal static Dictionary<string, POString> MergePoFileData(TextWriter swLog, TextReader srMain, TextReader srNew, string sMainFile, string sNewFile,
			out POString posHeader, out POString posObsolete)
		{
			POString posObsOld;
			POString posMainHeader;
			Dictionary<string, POString> dictMain = ReadPoFile(srMain, out posMainHeader, out posObsOld);
			POString posObsNew;
			POString posNewHeader;
			Dictionary<string, POString> dictNew = ReadPoFile(srNew, out posNewHeader, out posObsNew);
			if (dictNew.Count == 0)
			{
				Console.WriteLine("No translations found in new PO file!");
				throw new Exception("VOID NEW PO FILE");
			}
			DateTime now = DateTime.Now;
			CheckCompatiblePOFiles(posMainHeader, posNewHeader);
			MergePOHeaders(posMainHeader, posNewHeader);
			int cReplaced = 0;
			int cAdded = 0;
			int cSame = 0;
			int cMissing = 0;
			swLog.WriteLine($"Merging {sNewFile} into {sMainFile} on {now.ToString()}");
			using (Dictionary<string, POString>.Enumerator it = dictNew.GetEnumerator())
			{
				while (it.MoveNext())
				{
					var newMsgStr = it.Current.Value.MsgStrAsString();
					if (dictMain.ContainsKey(it.Current.Key))
					{
						if (!string.IsNullOrWhiteSpace(newMsgStr) && dictMain[it.Current.Key].MsgStrAsString() != newMsgStr)
						{
							swLog.WriteLine("");
							swLog.WriteLine(string.Format("OLD VALUE"));
							dictMain[it.Current.Key].Write(swLog);
							swLog.WriteLine(string.Format("NEW VALUE"));
							it.Current.Value.Write(swLog);
							dictMain[it.Current.Key] = it.Current.Value;
							++cReplaced;
						}
						else
						{
							++cSame;
						}
					}
					else if (!string.IsNullOrEmpty(newMsgStr))
					{
						dictMain.Add(it.Current.Key, it.Current.Value);
						++cAdded;
					}
					else
					{
						swLog.WriteLine("");
						swLog.WriteLine("Ignoring untranslated message from new file.");
						swLog.WriteLine(string.Format("NEW VALUE"));
						it.Current.Value.Write(swLog);
					}
				}
			}
			cMissing = dictMain.Count - (cReplaced + cSame + cAdded);
			swLog.WriteLine("");
			swLog.WriteLine($"Merge: {cReplaced} replaced, {cAdded} added, {cSame} same, {cMissing} not in new");
			Console.WriteLine("Merge: {0} replaced, {1} added, {2} same, {3} not in new",
				cReplaced, cAdded, cSame, cMissing);
			posHeader = posMainHeader;
			posObsolete = posObsOld;
			return dictMain;
		}

		/// <summary>
		/// Check the following lines in the PO file header for compatibility:
		///		MIME-Version:
		///		Content-Type:
		///		Content-Transfer-Encoding:
		///		X-Poedit-Language:
		///		X-Poedit-Country:
		/// </summary>
		/// <param name="posMainHeader"></param>
		/// <param name="posNewHeader"></param>
		private static void CheckCompatiblePOFiles(POString posMainHeader, POString posNewHeader)
		{
			CheckCompatiblePOHeader(posMainHeader, posNewHeader, "MIME Version", IsMIMEVersion);
			CheckCompatiblePOHeader(posMainHeader, posNewHeader, "Content Type", IsContentType);
			CheckCompatiblePOHeader(posMainHeader, posNewHeader, "Encoding", IsContentTransferEncoding);
			CheckCompatiblePOHeader(posMainHeader, posNewHeader, "Language", IsXPoeditLanguage);
			CheckCompatiblePOHeader(posMainHeader, posNewHeader, "Country", IsXPoeditCountry);
		}

		private static void CheckCompatiblePOHeader(POString posMainHeader, POString posNewHeader,
			string sHeader, Predicate<string> matchFn)
		{
			string sPo1 = posMainHeader.MsgStr.Find(matchFn);
			string sPo2 = posNewHeader.MsgStr.Find(matchFn);
			if (sPo1 == null || sPo2 == null || sPo1.ToLower() == sPo2.ToLower())
				return;
			int idx = sPo1.IndexOf(":");
			string s1 = sPo1.Substring(idx + 1).Trim();
			string s2 = sPo2.Substring(idx + 1).Trim();
			if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2))
				return;
			if (s1.ToLower() == s2.ToLower())
				return;
			Console.WriteLine("WARNING: first file's {0} = \"{1}\", but second file's {0} = \"{2}\"",
				sHeader, s1, s2);
		}

		private static bool IsMIMEVersion(string s)
		{
			return s != null && s.ToLower().StartsWith("mime-version:");
		}
		private static bool IsContentType(string s)
		{
			return s != null && s.ToLower().StartsWith("content-type:");
		}
		private static bool IsContentTransferEncoding(string s)
		{
			return s != null && s.ToLower().StartsWith("content-transfer-encoding:");
		}
		private static bool IsXPoeditLanguage(string s)
		{
			return s != null && s.ToLower().StartsWith("x-poedit-language:");
		}
		private static bool IsXPoeditCountry(string s)
		{
			return s != null && s.ToLower().StartsWith("x-poedit-country:");
		}

		/// <summary>
		/// Merge information in the following header lines:
		///		Project-Id-Version:
		///		POT-Creation-Date:
		///		PO-Revision-Date:
		///		Last-Translator:
		///		Language-Team:
		/// </summary>
		/// <param name="posMainHeader"></param>
		/// <param name="posNewHeader"></param>
		private static void MergePOHeaders(POString posMainHeader, POString posNewHeader)
		{
			MergePOHeaderLines(posMainHeader, posNewHeader, "Project-Id-Version:", IsProjectIdVersion);
			MergePOHeaderLines(posMainHeader, posNewHeader, "POT-Creation-Date:", IsPOTCreationDate);
			MergePOHeaderLines(posMainHeader, posNewHeader, "PO-Revision-Date:", IsPORevisionDate);
			MergePOHeaderLines(posMainHeader, posNewHeader, "Last-Translator:", IsLastTranslator);
			MergePOHeaderLines(posMainHeader, posNewHeader, "Language-Team:", IsLanguageTeam);
		}

		private static void MergePOHeaderLines(POString posMainHeader, POString posNewHeader,
			string sHeaderTag, Predicate<string> matchFn)
		{
			string sPo2 = posNewHeader.MsgStr.Find(matchFn);
			if (sPo2 == null)
				return;		// no information in new file
			string s2 = sPo2.Substring(sHeaderTag.Length).Trim();
			if (string.IsNullOrEmpty(s2))
				return;		// no information in new file
			int idx = posMainHeader.MsgStr.FindIndex(matchFn);
			if (idx < 0)
			{
				posMainHeader.MsgStr.Add(sPo2);	// no information in old file: add new info at end.
				return;
			}
			string s1 = posMainHeader.MsgStr[idx].Substring(sHeaderTag.Length).Trim();
			if (s1.ToLower() == s2.ToLower())
				return;		// same information as before
			if (string.IsNullOrEmpty(s1))
			{
				// empty information in the old file: store information from new file
				posMainHeader.MsgStr[idx] = $"{sHeaderTag} {s2}\n";
				return;
			}
			int ich = s1.ToLower().IndexOf(s2.ToLower());
			if (ich < 0)
			{
				// add the new info to the end of the old info
				posMainHeader.MsgStr[idx] = $"{sHeaderTag} {s1}; {s2}\n";
			}
		}

		private static bool IsProjectIdVersion(string s)
		{
			return s != null && s.ToLower().StartsWith("project-id-version:");
		}
		private static bool IsPOTCreationDate(string s)
		{
			return s != null && s.ToLower().StartsWith("pot-creation-date:");
		}
		private static bool IsPORevisionDate(string s)
		{
			return s != null && s.ToLower().StartsWith("po-revision-date:");
		}
		private static bool IsLastTranslator(string s)
		{
			return s != null && s.ToLower().StartsWith("last-translator:");
		}
		private static bool IsLanguageTeam(string s)
		{
			return s != null && s.ToLower().StartsWith("language-team:");
		}

		/// <summary>
		/// Update the PO file with the comments and new msgid strings in the POT file.
		/// </summary>
		private static void UpdatePoFile(string sPoFile, string sNewPot)
		{
			POString posMainHeader;
			POString posObsolete;
			var dictMain = LoadPOFile(sPoFile, out posMainHeader, out posObsolete);
			POString posNewHeader;
			var dictNew = LoadPotFile(sNewPot, out posNewHeader);
			DateTime now = DateTime.Now;
			string sTimeStamp = $"{now.Year:D4}{now.Month:D2}{now.Day:D2}{now.Hour:D2}{now.Minute:D2}{now.Second:D2}";
			File.Copy(sPoFile, $"{sPoFile}-{sTimeStamp}");
			using (StreamWriter swOut = new StreamWriter(sPoFile))
			{
				WriteUpdatedPoFile(swOut, dictMain, dictNew, posMainHeader, posObsolete, posNewHeader);
				swOut.Close();
			}
		}

		internal static void WriteUpdatedPoFile(TextWriter swOut, Dictionary<string, POString> dictOldPo,
			Dictionary<string, POString> dictNewPot, POString posHeader, POString posObsolete, POString posNewHeader)
		{
			var listNew = UpdatePoFromPot(dictNewPot, dictOldPo);
			// Should we update the header at all?
			posHeader.Write(swOut);
			var list = new List<POString>(dictOldPo.Values);
			int prevIdx = 0;
			foreach (var item in listNew)
			{
				int idx = list.Count;
				for (int i = prevIdx; i < list.Count; ++i)
				{
					if (POString.CompareMsgIds(item, list[i]) < 0)
					{
						idx = i;
						break;
					}
				}
				list.Insert(idx, item);
				prevIdx = idx;
			}
			UpdateAutoCommentsForUnusedMessages(dictNewPot, posNewHeader, list);
			foreach (POString item in list)
				item.Write(swOut);
			if (posObsolete != null)
				posObsolete.Write(swOut);
			Console.WriteLine("Update added {0} strings", listNew.Count);
		}

		private static void UpdateAutoCommentsForUnusedMessages(Dictionary<string, POString> dictNewPot, POString posNewHeader, List<POString> list)
		{
			const string projectIdTag = "Project-Id-Version: ";
			var unusedMsg = "(Not used by FieldWorks 1.0.0)";
			if (posNewHeader.MsgStr != null)
			{
				foreach (var msg in posNewHeader.MsgStr)
				{
					if (msg.StartsWith(projectIdTag))
					{
						unusedMsg = $"(Not used by {msg.Substring(projectIdTag.Length).Trim()})";
						break;
					}
				}
			}
			foreach (POString item in list)
			{
				if (!dictNewPot.ContainsKey(item.MsgIdAsString()))
				{
					// Insert a message at the beginning of the AutoComments
					if (item.AutoComments == null)
						item.AddAutoComment(unusedMsg);
					else
						item.AutoComments.Insert(0, unusedMsg);
				}
			}
		}

		/// <summary>
		/// Collect the new strings that have been added to the POT file.
		/// </summary>
		private static List<POString> UpdatePoFromPot(Dictionary<string, POString> dictNewPot, Dictionary<string, POString> dictOldPo)
		{
			List<POString> newItems = new List<POString>();
			foreach (var msgid in dictNewPot.Keys)
			{
				var newPo = dictNewPot[msgid];
				if (dictOldPo.ContainsKey(msgid))
				{
					// Add any new "user" comments to the existing ones.
					// Replace the old "auto" comments with the new ones.
					var oldPo = dictOldPo[msgid];
					if (newPo.UserComments != null)
					{
						foreach (var comment in newPo.UserComments)
							oldPo.AddUserComment(comment);
					}
					if (oldPo.AutoComments != null)
						oldPo.AutoComments.Clear();
					if (newPo.AutoComments != null)
					{
						foreach (var comment in newPo.AutoComments)
							oldPo.AddAutoComment(comment);
					}
				}
				else
				{
					newItems.Add(newPo);
				}
			}
			return newItems;
		}

		private static Dictionary<string, POString> LoadPotFile(string sPotFile, out POString posHeader)
		{
			using (StreamReader srIn = new StreamReader(sPotFile, Encoding.UTF8))
			{
				var dictTrans =  ReadPotFile(srIn, out posHeader);
				srIn.Close();
				return dictTrans;
			}
		}

		internal static Dictionary<string, POString> ReadPotFile(TextReader srIn, out POString posHeader)
		{
			POString.ResetInputLineNumber();
			var dictTrans = new Dictionary<string, POString>();
			posHeader = POString.ReadFromFile(srIn);
			POString pos = POString.ReadFromFile(srIn);
			while (pos != null)
			{
				StoreString(dictTrans, pos);
				pos = POString.ReadFromFile(srIn);
			}
			return dictTrans;
		}

		/// <summary>
		/// Execute the --store operation.
		/// </summary>
		/// <param name="sMsgFile"></param>
		/// <param name="sRoot"></param>
		private static void StoreLocalizedStrings(string sMsgFile, string sRoot)
		{
			string sLoc = GetLocaleFromMsgFile(sMsgFile);
			string sBase = Path.Combine(sRoot, s_DistFiles);
			string sEngFile = Path.Combine(sBase, "Language Explorer/Configuration/strings-en.xml");
			string sNewFile = Path.Combine(sBase, "Language Explorer/Configuration/strings-" + sLoc + ".xml");
			File.Delete(sNewFile);
			POString posObsolete;
			POString posHeader;
			Dictionary<string, POString> dictTrans = LoadPOFile(sMsgFile, out posHeader, out posObsolete);
			if (dictTrans.Count == 0)
			{
				Console.WriteLine("No translations found in PO file!");
				throw new Exception("VOID PO FILE");
			}
			XmlDocument xdoc = new XmlDocument();
			xdoc.Load(sEngFile);
			TranslateStringsElements(xdoc.DocumentElement, dictTrans);
			StoreTranslatedAttributes(xdoc.DocumentElement, dictTrans);
			StoreTranslatedLiterals(xdoc.DocumentElement, dictTrans);
			StoreTranslatedContextHelp(xdoc.DocumentElement, dictTrans);
			xdoc.Save(sNewFile);
		}

		private static void CheckLocalizedStrings(string sMsgFile)
		{
			POString posObsolete;
			POString posHeader;
			Dictionary<string, POString> dictTrans = LoadPOFile(sMsgFile, out posHeader, out posObsolete);
			if (dictTrans.Count == 0)
			{
				Console.WriteLine("No translations found in PO file!");
				throw new Exception("VOID PO FILE");
			}
			foreach (string sKey in dictTrans.Keys)
			{
				string sId = dictTrans[sKey].MsgIdAsString();
				string sMsg = dictTrans[sKey].MsgStrAsString();
				if (sId == sMsg || string.IsNullOrEmpty(sMsg))
					continue;
				List<string> rgsArgsOrig = FindAllArgumentMarkers(sId);
				List<string> rgsArgsTrans = FindAllArgumentMarkers(sMsg);
				bool fOk = true;
				if (rgsArgsOrig.Count != rgsArgsTrans.Count)
					fOk = false;
				for (int i = 0; fOk && i < rgsArgsOrig.Count; ++i)
				{
					fOk = rgsArgsOrig[i] == rgsArgsTrans[i];
				}
				if (!fOk)
				{
					Console.WriteLine("The translation for");
					Console.WriteLine("    \"{0}\"", sId);
					Console.WriteLine("        does not have the proper set of matching argument markers.");
					Console.WriteLine("    \"{0}\"", sMsg);
				}
			}
		}

		// {index[,alignment][:formatString]}
		static Regex s_regexArgMarker = new Regex("{[0-9]+(,[-+ 0-9]+)?(:[ 0-9a-zA-Z]+)?}",
			RegexOptions.Compiled|RegexOptions.CultureInvariant|RegexOptions.ExplicitCapture);

		private static List<string> FindAllArgumentMarkers(string sMsg)
		{
			MatchCollection matches = s_regexArgMarker.Matches(sMsg);
			List<string> rgsArgs = new List<string>(matches.Count);
			foreach (Match match in matches)
			{
				string sArg = sMsg.Substring(match.Index, match.Length);
				if (!rgsArgs.Contains(sArg))
					rgsArgs.Add(sArg);
			}
			rgsArgs.Sort();
			return rgsArgs;
		}

		private static Dictionary<string, POString> LoadPOFile(string sMsgFile, out POString posHeader, out POString posFinalObsolete)
		{
			using (StreamReader srIn = new StreamReader(sMsgFile, Encoding.UTF8))
			{
				var dictTrans = ReadPoFile(srIn, out posHeader, out posFinalObsolete);
				srIn.Close();
				return dictTrans;
			}
		}

		internal static Dictionary<string, POString> ReadPoFile(TextReader srIn, out POString posHeader, out POString posFinalObsolete)
		{
			POString.ResetInputLineNumber();
			posFinalObsolete = null;
			Dictionary<string, POString> dictTrans = new Dictionary<string, POString>();
			posHeader = POString.ReadFromFile(srIn);
			POString posFinal = null;
			POString pos = POString.ReadFromFile(srIn);
			while (pos != null)
			{
				if (StoreString(dictTrans, pos))
					posFinal = pos;
				pos = POString.ReadFromFile(srIn);
			}
			if (posFinal != null && posFinal.IsObsolete)
				posFinalObsolete = posFinal;
			return dictTrans;
		}

		/// <summary>
		/// Try to store a POString in the dictionary.
		/// </summary>
		/// <returns><c>true</c>, if string was stored (or obsolete), <c>false</c> if an error occurs.</returns>
		private static bool StoreString(Dictionary<string, POString> dictTrans, POString pos)
		{
			if (!pos.IsObsolete)
			{
				var msgid = pos.MsgIdAsString();
				if (dictTrans.ContainsKey(msgid))
				{
					Console.WriteLine("The message id \"{0}\" already exists.  Ignoring this occurrence around line {1}.",
						msgid, POString.InputLineNumber);
					return false;
				}
				else
				{
					dictTrans.Add(msgid, pos);
				}
			}
			return true;
		}

		/// <summary>
		/// This nicely recursive method replaces the English txt attribute values with the
		/// corresponding translated values if they exist.
		/// </summary>
		/// <param name="xel"></param>
		/// <param name="dictTrans"></param>
		private static void TranslateStringsElements(XmlElement xel,
			Dictionary<string, POString> dictTrans)
		{
			if (xel.Name == "string")
			{
				POString pos = null;
				string sEnglish = xel.GetAttribute("txt");
				if (dictTrans.TryGetValue(sEnglish, out pos))
				{
					string sTranslation = pos.MsgStrAsString();
					xel.SetAttribute("txt", sTranslation);
					xel.SetAttribute("English", sEnglish);
				}
			}
			foreach (XmlNode xn in xel.ChildNodes)
			{
				if (xn is XmlElement)
					TranslateStringsElements(xn as XmlElement, dictTrans);
			}
		}

		private static void StoreTranslatedAttributes(XmlElement xelRoot,
			Dictionary<string, POString> dictTrans)
		{
			XmlElement xelGroup = xelRoot.OwnerDocument.CreateElement("group");
			xelGroup.SetAttribute("id", "LocalizedAttributes");
			Dictionary<string, POString>.Enumerator en = dictTrans.GetEnumerator();
			while (en.MoveNext())
			{
				POString pos = en.Current.Value;
				string sValue = pos.MsgStrAsString();
				if (string.IsNullOrEmpty(sValue))
					continue;
				List<string> rgs = pos.AutoComments;
				if (rgs == null)
					continue;
				for (int i = 0; i < rgs.Count; ++i)
				{
					if (rgs[i] != null &&
						// handle bug in creating original POT file due to case sensitive search.
						(rgs[i].StartsWith("/") || rgs[i].StartsWith("file:///")) &&
						IsFromXmlAttribute(rgs[i]))
					{
						XmlElement xelString = xelRoot.OwnerDocument.CreateElement("string");
						xelString.SetAttribute("id", pos.MsgIdAsString());
						xelString.SetAttribute("txt", sValue);
						xelGroup.AppendChild(xelString);
						break;
					}
				}
			}
			xelRoot.AppendChild(xelGroup);
		}

		private static bool IsFromXmlAttribute(string sComment)
		{
			int idx = sComment.LastIndexOf("/");
			if (idx < 0 || sComment.Length == idx + 1)
				return false;
			if (sComment[idx + 1] != '@')
				return false;
			else
				return sComment.Length > idx + 2;
		}

		private static void StoreTranslatedLiterals(XmlElement xelRoot,
			Dictionary<string, POString> dictTrans)
		{
			XmlElement xelGroup = xelRoot.OwnerDocument.CreateElement("group");
			xelGroup.SetAttribute("id", "LocalizedLiterals");
			Dictionary<string, POString>.Enumerator en = dictTrans.GetEnumerator();
			while (en.MoveNext())
			{
				POString pos = en.Current.Value;
				string sValue = pos.MsgStrAsString();
				if (string.IsNullOrEmpty(sValue))
					continue;
				List<string> rgs = pos.AutoComments;
				if (rgs == null)
					continue;
				for (int i = 0; i < rgs.Count; ++i)
				{
					if (rgs[i] != null && rgs[i].StartsWith("/") && rgs[i].EndsWith("/lit"))
					{
						XmlElement xelString = xelRoot.OwnerDocument.CreateElement("string");
						xelString.SetAttribute("id", pos.MsgIdAsString());
						xelString.SetAttribute("txt", sValue);
						xelGroup.AppendChild(xelString);
						break;
					}
				}
			}
			xelRoot.AppendChild(xelGroup);
		}

		private static void StoreTranslatedContextHelp(XmlElement xelRoot,
			Dictionary<string, POString> dictTrans)
		{
			XmlElement xelGroup = xelRoot.OwnerDocument.CreateElement("group");
			xelGroup.SetAttribute("id", "LocalizedContextHelp");
			Dictionary<string, POString>.Enumerator en = dictTrans.GetEnumerator();
			while (en.MoveNext())
			{
				POString pos = en.Current.Value;
				string sValue = pos.MsgStrAsString();
				if (string.IsNullOrEmpty(sValue))
					continue;
				List<string> rgs = pos.AutoComments;
				if (rgs == null)
					continue;
				for (int i = 0; i < rgs.Count; ++i)
				{
					string sId = FindContextHelpId(rgs[i]);
					if (!string.IsNullOrEmpty(sId))
					{
						XmlElement xelString = xelRoot.OwnerDocument.CreateElement("string");
						xelString.SetAttribute("id", sId);
						xelString.SetAttribute("txt", sValue);
						xelGroup.AppendChild(xelString);
						break;
					}
				}
			}
			xelRoot.AppendChild(xelGroup);
		}

		private static string FindContextHelpId(string sComment)
		{
			const string ksContextMarker = "/ContextHelp.xml::/strings/item[@id=\"";
			if (sComment != null &&
				sComment.StartsWith("/"))
			{
				int idx = sComment.IndexOf(ksContextMarker);
				if (idx > 0)
				{
					string sId = sComment.Substring(idx + ksContextMarker.Length);
					int idxEnd = sId.IndexOf('"');
					if (idxEnd > 0)
						return sId.Remove(idxEnd);
				}
			}
			return null;
		}

		/// <summary>
		/// Derive the desired locale from the given message filename.
		/// </summary>
		private static string GetLocaleFromMsgFile(string sMsgFile)
		{
			// Try to obtain the locale from the filename.
			string sLoc1 = null;
			int idx = sMsgFile.LastIndexOfAny("\\/".ToCharArray());
			if (idx < 0)
				idx = 0;
			int idx1 = sMsgFile.IndexOf('.', idx);
			int idx2 = sMsgFile.LastIndexOf('.');
			if (idx1 != -1 && idx2 > idx1)
			{
				++idx1;
				int cch = idx2 - idx1;
				sLoc1 = sMsgFile.Substring(idx1, cch);
			}
			if (string.IsNullOrEmpty(sLoc1))
			{
				Console.WriteLine("ERROR: cannot determine locale from filename!");
				throw new Exception("CANNOT DETERMINE LOCALE");
			}
			return sLoc1;
		}

		/// <summary>
		/// Extract localizable strings from the XML configuration files in the distribution
		/// tree.
		/// </summary>
		private static void ExtractFromXmlConfigFiles(string sRoot, List<POString> rgsPOStrings)
		{
			// Get the list of configuration files to process.
			string sBase = Path.Combine(sRoot, s_DistFiles);
			string sRootDir = Path.Combine(sBase, "Language Explorer/Configuration");
			string[] rgsLocConfigFiles = FindAllFiles(sRootDir, "strings-*.xml");
			List<string> lssLocConfigFiles = new List<string>(rgsLocConfigFiles.Length);
			for (int i = 0; i < rgsLocConfigFiles.Length; ++i)
				lssLocConfigFiles.Add(rgsLocConfigFiles[i].ToLower());
			string[] rgsConfigFiles = FindAllFiles(sRootDir, "*.xml");
			List<string> lssConfigFiles = new List<string>(
				rgsConfigFiles.Length + 4 - lssLocConfigFiles.Count);
			for (int i = 0; i < rgsConfigFiles.Length; ++i)
			{
				if (!lssLocConfigFiles.Contains(rgsConfigFiles[i].ToLower()))
					lssConfigFiles.Add(rgsConfigFiles[i]);
			}
			rgsConfigFiles = FindAllFiles(sRootDir, "*.fwlayout");
			for (int i = 0; i < rgsConfigFiles.Length; ++i)
				lssConfigFiles.Add(rgsConfigFiles[i]);
			sRootDir = Path.Combine(sBase, "Parts");
			// Let's not include the auto-generated parts and layouts for now.
			//rgsConfigFiles = FindAllFiles(sRootDir, "*.xml");
			rgsConfigFiles = FindAllFiles(sRootDir, "Standard*.xml");
			for (int i = 0; i < rgsConfigFiles.Length; ++i)
				lssConfigFiles.Add(rgsConfigFiles[i]);
			// Process the configuration files.
			for (int i = 0; i < lssConfigFiles.Count; ++i)
				ProcessXmlConfigFile(lssConfigFiles[i], sRoot, rgsPOStrings);
			ProcessXmlStringsFile(sRoot, rgsPOStrings);

			sRootDir = Path.Combine(sBase, "Language Explorer/DefaultConfigurations");
			if (Directory.Exists(sRootDir))
			{
				lssConfigFiles.Clear();
				rgsConfigFiles = FindAllFiles(sRootDir, "*.fwdictconfig");
				if (rgsConfigFiles != null && rgsConfigFiles.Length > 0)
					lssConfigFiles.AddRange(rgsConfigFiles);
				for (int i = 0; i < lssConfigFiles.Count; ++i)
					ProcessFwDictConfigFile(lssConfigFiles[i], sRoot, rgsPOStrings);
			}
		}

		/// <summary>
		/// Process one XML configuration file to extract localizable strings.
		/// </summary>
		/// <param name="sConfigFile"></param>
		/// <param name="rgsPOStrings"></param>
		private static void ProcessXmlConfigFile(string sConfigFile, string sRoot, List<POString> rgsPOStrings)
		{
			XmlDocument xdoc = new XmlDocument();
			xdoc.Load(sConfigFile);
			var sAutoFilePath = ComputeAutoCommentFilePath(sConfigFile, sRoot);
			ProcessConfigElement(xdoc.DocumentElement, sAutoFilePath, rgsPOStrings);
		}

		/// <summary>
		/// Process one XML element to extract localizable strings.
		/// </summary>
		internal static void ProcessConfigElement(XmlElement xel, string sAutoCommentFilePath, List<POString> rgsPOStrings)
		{
			if (xel.Name == "lit" && !string.IsNullOrEmpty(xel.InnerText.Trim()))
				StoreLiteralString(xel, rgsPOStrings, sAutoCommentFilePath);

			string sLabel = xel.GetAttribute("label");
			if (!string.IsNullOrEmpty(sLabel.Trim()) && sLabel.Trim() != "$label")
				StoreAttributeString(xel, "label", sLabel, rgsPOStrings, sAutoCommentFilePath);
			string sAbbr = xel.GetAttribute("abbr");
			if (!string.IsNullOrEmpty(sAbbr.Trim()))
				StoreAttributeString(xel, "abbr", sAbbr, rgsPOStrings, sAutoCommentFilePath);
			string sTitle = xel.GetAttribute("title");
			if (!string.IsNullOrEmpty(sTitle.Trim()))
				StoreAttributeString(xel, "title", sTitle, rgsPOStrings, sAutoCommentFilePath);
			sLabel = xel.GetAttribute("formlabel");
			if (!string.IsNullOrEmpty(sLabel.Trim()))
				StoreAttributeString(xel, "formlabel", sLabel, rgsPOStrings, sAutoCommentFilePath);
			sLabel = xel.GetAttribute("okbuttonlabel");
			if (!string.IsNullOrEmpty(sLabel.Trim()))
				StoreAttributeString(xel, "okbuttonlabel", sLabel, rgsPOStrings, sAutoCommentFilePath);
			sLabel = xel.GetAttribute("headerlabel");
			if (!string.IsNullOrEmpty(sLabel.Trim()))
				StoreAttributeString(xel, "headerlabel", sLabel, rgsPOStrings, sAutoCommentFilePath);
			sLabel = xel.GetAttribute("ghostLabel");
			if (!string.IsNullOrEmpty(sLabel.Trim()))
				StoreAttributeString(xel, "ghostLabel", sLabel, rgsPOStrings, sAutoCommentFilePath);
			string sAfter = xel.GetAttribute("after");
			if (!string.IsNullOrEmpty(sAfter.Trim()))
				StoreAttributeString(xel, "after", sAfter, rgsPOStrings, sAutoCommentFilePath);
			string sBefore = xel.GetAttribute("before");
			if (!string.IsNullOrEmpty(sBefore.Trim()))
				StoreAttributeString(xel, "before", sBefore, rgsPOStrings, sAutoCommentFilePath);
			string sTooltip = xel.GetAttribute("tooltip");
			if (!string.IsNullOrEmpty(sTooltip.Trim()))
				StoreAttributeString(xel, "tooltip", sTooltip, rgsPOStrings, sAutoCommentFilePath);

			string sEditor = xel.GetAttribute("editor");
			string sMessage = xel.GetAttribute("message");
			if (sEditor.Trim().ToLower() == "lit" && !string.IsNullOrEmpty(sMessage.Trim()))
				StoreAttributeString(xel, "message", sMessage, rgsPOStrings, sAutoCommentFilePath);

			if (xel.Name == "item" &&
				!string.IsNullOrEmpty(xel.InnerText.Trim()) &&
				xel.ParentNode.Name == "strings")
			{
				string sId = xel.GetAttribute("id");
				if (!string.IsNullOrEmpty(sId))
				{
					StoreLiteralString(xel, rgsPOStrings, sAutoCommentFilePath);
					string sCaption = xel.GetAttribute("caption");
					if (!string.IsNullOrEmpty(sCaption))
						StoreAttributeString(xel, "caption", sCaption, rgsPOStrings, sAutoCommentFilePath);
					sCaption = xel.GetAttribute("captionformat");
					if (!string.IsNullOrEmpty(sCaption))
						StoreAttributeString(xel, "captionformat", sCaption, rgsPOStrings, sAutoCommentFilePath);
				}
			}

			foreach (XmlNode xn in xel.ChildNodes)
			{
				if (xn is XmlElement)
					ProcessConfigElement(xn as XmlElement, sAutoCommentFilePath, rgsPOStrings);
			}
		}

		/// <summary>
		/// Store an attribute value with a comment giving its xpath location.
		/// </summary>
		/// <param name="xel"></param>
		/// <param name="sName"></param>
		/// <param name="sValue"></param>
		/// <param name="rgsPOStrings"></param>
		private static void StoreAttributeString(XmlElement xel, string sName, string sValue,
			List<POString> rgsPOStrings, string sAutoCommentFilePath)
		{
			string sTranslate = xel.GetAttribute("translate");
			if (sTranslate.Trim().ToLower() == "do not translate")
				return;
			string sPath = ComputePathComment(xel, sName, sAutoCommentFilePath);
			string[] rgsComment = null;
			if (string.IsNullOrEmpty(sTranslate))
				rgsComment = new string[1] { sPath };
			else
				rgsComment = new string[2] { sPath, FixStringForEmbeddedQuotes(sTranslate) };
			POString pos = new POString(rgsComment,
				new string[1] { FixStringForEmbeddedQuotes(sValue) },
				new string[1] { "" });
			rgsPOStrings.Add(pos);
		}

		/// <summary>
		/// Compute an XPath like comment to store in the POT file.
		/// </summary>
		/// <param name="xel"></param>
		/// <param name="sName"></param>
		/// <returns></returns>
		private static string ComputePathComment(XmlElement xel, string sName, string sAutoCommentFilePath)
		{
			StringBuilder bldr = new StringBuilder(sName);
			if (sName != null)
				bldr.Insert(0, "/@");
			while (xel != null)
			{
				if (xel.Name == "part" || xel.Name == "item")
				{
					string s = xel.GetAttribute("id");
					if (!string.IsNullOrEmpty(s))
					{
						bldr.Insert(0, $"[@id=\"{s}\"]");
					}
					else
					{
						s = xel.GetAttribute("ref");
						if (!string.IsNullOrEmpty(s))
							bldr.Insert(0, $"[@ref=\"{s}\"]");
					}
				}
				else if (xel.Name == "layout")
				{
					string s1 = xel.GetAttribute("class");
					string s2 = xel.GetAttribute("type");
					string s3 = xel.GetAttribute("name");
					bldr.Insert(0, $"[\"{s1}-{s2}-{s3}\"]");
				}
				bldr.Insert(0, xel.Name);
				bldr.Insert(0, "/");
				xel = xel.ParentNode as XmlElement;
			}
			bldr.Insert(0, "::");
			bldr.Insert(0, sAutoCommentFilePath);
			return bldr.ToString();
		}

		/// <summary>
		/// Store the string for a &lt;lit&gt; element.
		/// </summary>
		/// <param name="xel"></param>
		/// <param name="rgsPOStrings"></param>
		private static void StoreLiteralString(XmlElement xel, List<POString> rgsPOStrings, string sAutoCommentFilePath)
		{
			string sTranslate = xel.GetAttribute("translate");
			if (sTranslate.Trim().ToLower() == "do not translate")
				return;
			string sPath = ComputePathComment(xel, null, sAutoCommentFilePath);
			string[] rgsComment = null;
			if (string.IsNullOrEmpty(sTranslate))
				rgsComment = new string[1] { sPath };
			else
				rgsComment = new string[2] { sPath, FixStringForEmbeddedQuotes(sTranslate) };
			string sVal = FixStringForEmbeddedQuotes(xel.InnerText);
			string[] rgsValue = sVal.Split(s_rgsNewline, StringSplitOptions.None);
			POString pos = new POString(rgsComment, rgsValue, new string[1] { "" });
			rgsPOStrings.Add(pos);
		}

		/// <summary>
		/// Process a FieldWorks .fwdictconfig file.
		/// </summary>
		private static void ProcessFwDictConfigFile(string sConfigFile, string sRoot, List<POString> rgsPOStrings)
		{
			XmlDocument xdoc = new XmlDocument();
			xdoc.Load(sConfigFile);
			var sAutoFilePath = ComputeAutoCommentFilePath(sConfigFile, sRoot);
			ProcessFwDictConfigElement(xdoc.DocumentElement, sAutoFilePath, rgsPOStrings);
		}

		/// <summary>
		/// Process one element from a FieldWorks .fwdictconfig file.  Recursively process any children.
		/// </summary>
		internal static void ProcessFwDictConfigElement(XmlElement xel, string sAutoFilePath, List<POString> rgsPOStrings)
		{
			string sName = xel.GetAttribute("name");
			if (!string.IsNullOrEmpty(sName.Trim()))
				StoreFwDictAttributeString(xel, "name", sName, sAutoFilePath, rgsPOStrings);
			string sNameSuffix = xel.GetAttribute("nameSuffix");
			if (!string.IsNullOrEmpty(sNameSuffix.Trim()))
				StoreFwDictAttributeString(xel, "nameSuffix", sNameSuffix, sAutoFilePath, rgsPOStrings);
			string sAfter = xel.GetAttribute("after");
			if (!string.IsNullOrEmpty(sAfter.Trim()))
				StoreFwDictAttributeString(xel, "after", sAfter, sAutoFilePath, rgsPOStrings);
			string sBefore = xel.GetAttribute("before");
			if (!string.IsNullOrEmpty(sBefore.Trim()))
				StoreFwDictAttributeString(xel, "before", sBefore, sAutoFilePath, rgsPOStrings);
			string sBetween = xel.GetAttribute("between");
			if (!string.IsNullOrEmpty(sBetween.Trim()))
				StoreFwDictAttributeString(xel, "between", sBetween, sAutoFilePath, rgsPOStrings);
			foreach (XmlNode xn in xel.ChildNodes)
			{
				if (xn is XmlElement)
					ProcessFwDictConfigElement(xn as XmlElement, sAutoFilePath, rgsPOStrings);
			}
		}

		/// <summary>
		/// Store one attribute value from a FieldWorks .fwdictconfig file as a POString, adding it to the list.
		/// </summary>
		private static void StoreFwDictAttributeString(XmlElement xel, string sName, string sValue, string sAutoFilePath, List<POString> rgsPOStrings)
		{
			POString pos = new POString(
				new string[1] { ComputeFwDictConfigPathComment(xel, sAutoFilePath, sName) },
				new string[1] { FixStringForEmbeddedQuotes(sValue) },
				new string[1] { string.Empty });
			rgsPOStrings.Add(pos);
		}

		/// <summary>
		/// Compute a reduced path comment for one attribute from a FieldWorks .fwdictconfig file.
		/// </summary>
		private static string ComputeFwDictConfigPathComment(XmlElement xel, string sAutoFilePath, string sName)
		{
			return $"{sAutoFilePath}:://{xel.Name}/@{sName}";
		}

		/// <summary>
		/// Process the strings-en.xml file.
		/// </summary>
		/// <param name="sRoot"></param>
		/// <param name="rgsPOStrings"></param>
		private static void ProcessXmlStringsFile(string sRoot, List<POString> rgsPOStrings)
		{
			string sBase = Path.Combine(sRoot, s_DistFiles);
			string sFile = Path.Combine(sBase, "Language Explorer/Configuration/strings-en.xml");
			XmlDocument xdoc = new XmlDocument();
			xdoc.Load(sFile);
			ProcessStringsElement(xdoc.DocumentElement, rgsPOStrings);
		}

		/// <summary>
		/// Process one element from the strings-en.xml file.
		/// </summary>
		/// <param name="xmlElement"></param>
		/// <param name="rgsPOStrings"></param>
		internal static void ProcessStringsElement(XmlElement xel, List<POString> rgsPOStrings)
		{
			if (xel.Name == "string")
			{
				string sTxt = FixStringForEmbeddedQuotes(xel.GetAttribute("txt"));
				if (!string.IsNullOrEmpty(sTxt.Trim()))
				{
					string sComment = xel.GetAttribute("translate");
					if (sComment.Trim().ToLower() != "do not translate")
					{
						string sPath = ComputeStringPathComment(xel);
						string[] rgsComments = null;
						if (!string.IsNullOrEmpty(sComment.Trim()))
							rgsComments = new string[2] { sComment, sPath };
						else
							rgsComments = new string[1] { sPath };
						POString pos = new POString(rgsComments,
							new string[1] { sTxt }, new string[1] { "" });
						rgsPOStrings.Add(pos);
					}
				}
			}
			foreach (XmlNode xn in xel.ChildNodes)
			{
				if (xn is XmlElement)
					ProcessStringsElement(xn as XmlElement, rgsPOStrings);
			}
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="xel"></param>
		/// <returns></returns>
		private static string ComputeStringPathComment(XmlElement xel)
		{
			StringBuilder bldr = new StringBuilder("|");
			while (xel != null)
			{
				if (xel.Name == "string" || xel.Name == "group")
					bldr.Insert(0, $"/{xel.GetAttribute("id")}");
				xel = xel.ParentNode as XmlElement;
			}
			bldr.Insert(0, "/|strings-en.xml::");
			return bldr.ToString();
		}

		/// <summary>
		/// Find all files in the given directory tree that match the given filename pattern.
		/// Note that this searches all subdirectories, ie, the whole tree under the root directory.
		/// </remarks>
		private static string[] FindAllFiles(string sRootDir, string sPattern)
		{
			return Directory.GetFiles(sRootDir, sPattern, SearchOption.AllDirectories);
		}

		/// <summary>
		/// Create a test "localized" PO file.
		/// </summary>
		private static void GenerateTestPOFile(string sOutputFile, string sRoot,
			List<string> rgsDirs, string sLocale)
		{
			List<POString> rgPOStrings = ExtractLocalizableStrings(sRoot, rgsDirs);
			StreamWriter swOut = null;
			try
			{
				swOut = new StreamWriter(sOutputFile, false, Encoding.UTF8);
				WritePoHeader(swOut, sRoot, sLocale);
				for (int i = 0; i < rgPOStrings.Count; ++i)
				{
					List<string> rgsId = rgPOStrings[i].MsgId;
					List<string> rgsStr = new List<string>(rgsId.Count);
					for (int j = 0; j < rgsId.Count; ++j)
						rgsStr.Add(MungeForTest(rgsId[j], sLocale));
					rgPOStrings[i].MsgStr = rgsStr;
					rgPOStrings[i].Write(swOut);
				}
			}
			finally
			{
				if (swOut != null)
					swOut.Close();
			}

		}

		/// <summary>
		/// Add and change characters in the input string.
		/// </summary>
		/// <param name="sInput"></param>
		/// <param name="sLocale"></param>
		/// <returns></returns>
		private static string MungeForTest(string sInput, string sLocale)
		{
			// Regex for RGB codes at the end of color strings
			Regex s_regexArgRGB = new Regex(",[0-9]+,[0-9]+,[0-9]+$",
				RegexOptions.Compiled|RegexOptions.CultureInvariant);
			if (sInput == "en")
				return sLocale;
			// First, convert string to all uppercase.
			string s1 = sInput.ToUpper();
			if (s1.EndsWith(".CHM"))
				return s1;	// don't munge helpfile pathnames beyond uppercasing.
			if (s_regexArgRGB.IsMatch(s1))
				return s1;	// don't munge color strings beyond uppercasing.  If they don't end with a valid RGB code, FW will crash.
			StringBuilder bldr = new StringBuilder(s1);
			//// Second, double all vowels.
			//for (int i = bldr.Length - 1; i >= 0; --i)
			//{
			//    if ("AEIOU".IndexOf(bldr[i]) >= 0)
			//        bldr.Insert(i, bldr[i]);
			//}
			//if (bldr.ToString().EndsWith("Y"))	// trailing y is a vowel.
			//    bldr.Append("Y");
			// Third, add an @ to the beginning and ending of the string.
			for (int idx = 0; idx < bldr.Length; ++idx)
			{
				if (!char.IsWhiteSpace(bldr[idx]))
				{
					bldr.Insert(idx, '@');
					break;
				}
			}
			for (int idx = bldr.Length - 1; idx >= 0; --idx)
			{
				if (!char.IsWhiteSpace(bldr[idx]))
				{
					bldr.Insert(idx+1, '@');
					break;
				}
			}
			return bldr.ToString();
		}
	}
}
