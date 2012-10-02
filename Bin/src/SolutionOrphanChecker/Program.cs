using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace SolutionOrphanChecker
{
	public class Program
	{
		public const string SourceFilePattern = @"(?<!\.g)\.cs$";

		static void Main(string[] args)
		{
			string path = (args.Length > 0) ? args[0] : GetWorkingFolder();
			Regex regex = new Regex(SourceFilePattern);
			var allSourceFiles = from file in Directory.GetFiles(path, "*.cs", SearchOption.AllDirectories)
								 where regex.IsMatch(file)
								 select file;
			var projects = Directory.GetFiles(path, "*.csproj", SearchOption.AllDirectories);

			//Non-compiling
			Console.WriteLine("-------------------\nFiles without compile actions:");
			foreach (var file in FindCSharpFiles(projects, true))
			{
				//We want to filter out generated code scripts & assembly info
				if (!file.Contains("AssemblyInfo.cs") && !file.Contains("vm.cs"))
					Console.WriteLine(file);
			}

			Console.WriteLine("-------------------\nReference where SpecificVersion is true (= bad!):");
			foreach (var reference in ParseProjectPathsReferences(projects))
				Console.WriteLine(reference);

			//Orphans
			Console.WriteLine("-------------------\nOrphans:");
			HashSet<string> fastList = new HashSet<string>();
			foreach (var filename in FindCSharpFiles(projects, false))
				fastList.Add(filename.ToLower());
			var orphans = from sourceFile in allSourceFiles
						  where !fastList.Contains(sourceFile.ToLower())
						  select sourceFile;
			foreach (var orphan in orphans)
				Console.WriteLine(orphan);

		}

		static string GetWorkingFolder()
		{
			return Path.GetDirectoryName(typeof(Program).Assembly.CodeBase.Replace("file:///", String.Empty));
		}

		static IEnumerable<string> FindCSharpFiles(IEnumerable<string> projectPaths, bool noneActionOnly)
		{
			var filesCompile = ParseProjectPaths(projectPaths, "Compile");
			var filesNone = ParseProjectPaths(projectPaths, "None");
			return (noneActionOnly) ? filesNone : filesCompile.Concat(filesNone);
		}

		static IEnumerable<string> ParseProjectPaths(IEnumerable<string> projectPaths, string tag)
		{
			const string xmlNamespace = "{http://schemas.microsoft.com/developer/msbuild/2003}";
			return from projectPath in projectPaths
				   let xml = XDocument.Load(projectPath)
				   let dir = Path.GetDirectoryName(projectPath)
				   from c in xml.Descendants(xmlNamespace + tag)
				   let inc = c.Attribute("Include").Value
				   where inc.EndsWith(".cs")
				   select Path.Combine(dir, c.Attribute("Include").Value);
		}

		static IEnumerable<string> ParseProjectPathsReferences(IEnumerable<string> projectPaths)
		{
			const string xmlNamespace = "{http://schemas.microsoft.com/developer/msbuild/2003}";
			return (from projectPath in projectPaths
					let xml = XDocument.Load(projectPath)
					let dir = Path.GetDirectoryName(projectPath)
					from c in xml.Descendants(xmlNamespace + "Reference")
					let bla = c.Element(xmlNamespace + "SpecificVersion")
					where (bla != null && ((string)bla).ToLower() == "true")
					|| (bla == null && c.Attribute("Include").Value.Contains("Version="))
					select projectPath + ": " + c.Attribute("Include").Value);
		}


	}
}
