using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace SetCsprojFilePropsToX86
{
	/// <summary>
	/// A utility to adjust all .csproj files in the current FW branch's src folder (assuming our .exe
	/// is in the bin folder), to set the PlatformTarget property of each build configuration to x86.
	/// IMPORTANT: every .csproj file that needs adjusting must have write access available. This probably
	/// means checking out every file from Perforce, then reverting unchanged files afterwards.
	/// </summary>
	class Program
	{
		private const string MsbuildUri = "http://schemas.microsoft.com/developer/msbuild/2003";
		private const string CpuTarget = "x86";

		static void Main()
		{
			// Get FW src folder:
			var exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
			if (exePath == null)
				throw new Exception("So sorry, don't know where we are!");
			var exeFolder = Path.GetDirectoryName(exePath);

			// Get development project root path:
			if (exeFolder.ToLowerInvariant().EndsWith("bin"))
			{
				var rootFolder = Path.Combine(exeFolder.Substring(0, exeFolder.LastIndexOf('\\')), "src");
				var projFileSet = GetAllProjFiles(rootFolder);

				foreach (var file in projFileSet)
					EnsurePlatformTargetInProjFile(file);
			}
			else
				throw new Exception("Cannot determine path of FW src folder from .exe location '" + exeFolder + "'.");

		}

		/// <summary>
		/// Adjusts a Visual Studio .csproj file so that the properties of each build configuration
		/// specify x86 as the targeted platform.
		/// </summary>
		/// <param name="filePath">The full path to the .csproj file</param>
		private static void EnsurePlatformTargetInProjFile(string filePath)
		{
			// Load the .csproj file, preserving white space:
			var projXml = new XmlDocument {PreserveWhitespace = true};
			projXml.Load(filePath);

			// Set up a mini report of any changes we make:
			var editsMade = "";

			// Contend with the namespace used in .csproj files:
			var xmlnsManager = new XmlNamespaceManager(projXml.NameTable);
			xmlnsManager.AddNamespace("msbuild", MsbuildUri);

			// Get all PropertyGroup nodes in the .csproj file:
			var propertyGroups = projXml.SelectNodes("//msbuild:Project/msbuild:PropertyGroup", xmlnsManager);

			foreach (XmlElement propertyGroup in propertyGroups)
			{
				// We only want to adjust PropertyGroup nodes that have a Condition attribute:
				var condition = propertyGroup.GetAttribute("Condition");

				// See if there is already a PlatformTarget node:
				var platformTargetNode = propertyGroup.SelectSingleNode("msbuild:PlatformTarget", xmlnsManager) as XmlElement;
				if (platformTargetNode == null && condition != "")
				{
					// Create missing PlatformTarget element with suitable whitespace:
					var tabNode = projXml.CreateWhitespace("  ");
					propertyGroup.AppendChild(tabNode); // Indent

					platformTargetNode = projXml.CreateElement("PlatformTarget", MsbuildUri);
					propertyGroup.AppendChild(platformTargetNode); // The PlatformTarget element itself

					var newlineNode = projXml.CreateWhitespace(Environment.NewLine);
					propertyGroup.AppendChild(newlineNode); // New line

					// Describe what we did in plain text:
					editsMade += "Added PlatformTarget to PropertyGroup[Condition=\"" + condition + "\"]; ";
				}

				// Make sure any PlatformTarget element is set to our required CpuTarget:
				if (platformTargetNode != null)
				{
					if (platformTargetNode.InnerText != CpuTarget)
					{
						// If there was a different PlatformTarget specification, report what it was originally:
						if (platformTargetNode.InnerText.Length > 0)
							editsMade += "Changed PlatformTarget from '" + platformTargetNode.Value + "' to '" + CpuTarget + "'; ";

						platformTargetNode.InnerText = CpuTarget;
					}
				}
			}

			// Report what's going on:
			Console.Write(filePath + ": ");

			if (editsMade.Length == 0)
				Console.WriteLine("No change.");
			else
			{
				// Try to save the edited file:
				try
				{
					projXml.Save(filePath);
				}
				catch (Exception e)
				{
					editsMade = "ERROR: COULD NOT SAVE: " + e.Message;
				}
				Console.WriteLine(editsMade);
			}
		}

		/// <summary>
		/// Scours directory structure recursively for .csproj files.
		/// </summary>
		/// <param name="rootFolderPath">The directory to start in</param>
		/// <returns>A set of all .csproj file paths</returns>
		private static IEnumerable<string> GetAllProjFiles(string rootFolderPath)
		{
			var response = new HashSet<string>();

			// Get the files in this directory:
			foreach (var file in Directory.GetFiles(rootFolderPath, "*.csproj"))
			{
				response.Add(file);
			}

			// Now recurse all subfolders:
			foreach (var subFolder in Directory.GetDirectories(rootFolderPath))
			{
				var subFolderFiles = GetAllProjFiles(subFolder);
				response.UnionWith(subFolderFiles);
			}
			return response;
		}
	}
}
