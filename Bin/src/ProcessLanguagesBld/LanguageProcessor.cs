using System;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ProcessLanguagesBld
{
	/// <summary>
	/// A replacement for the NAnt target "processLanguages" formerly in Bld\Localize.build.xml.
	/// The advantage of this C# program is that it processes localizations in parallel.
	/// </summary>
	class LanguageProcessor
	{
		private string m_config; // e.g. Release
		private string m_fwRoot; // e.g. C:\FW-WW
		private string m_fwOutput; // e.g. C:\FW-WW\Output\Release
		private readonly bool IsUnix = (Environment.OSVersion.Platform == PlatformID.Unix);

		public string Config { set { m_config = value; } }
		public string FwRoot { set { m_fwRoot = value; } }
		public string FwOutput { set { m_fwOutput = value; } }

		internal int Run()
		{
			// Get the path to the .po files:
			var poFileFolder = Path.Combine(m_fwRoot, "Localizations");

			// Get all the .po files paths:
			string[] poFiles = Directory.GetFiles(poFileFolder, "messages.*.po");

			// Prepare to get responses from processing each .po file
			int numFailures = 0;
			var localizationLogs = new string [poFiles.Length]; // Console output for each localization

			// Main loop for each language:
			Parallel.ForEach(poFiles, currentFile =>
				{
					// Process current .po file, storinng console output in log:
					string log;
					if (!ProcessFile(currentFile, out log))
						numFailures++;

					// Slot current log file into array at index matching current language:
					int index = Array.FindIndex(poFiles, poFile => (poFile == currentFile));
					if (index != -1)
						localizationLogs[index] = log;
				}
			);

			// Output all processing logs to console:
			for (int i = 0; i < poFiles.Length; i++)
			{
				if (localizationLogs[i] == null)
					Console.WriteLine("ERROR: localization of " + poFiles[i] + " yielded no log!");
				else
					Console.WriteLine(localizationLogs[i] + Environment.NewLine);
			}

			// Decide if we succeeded or not:
			if (numFailures > 0)
				Console.WriteLine("STOPPING BUILD - number of failed localizations: " + numFailures + ".");

			return numFailures;
		}

		/// <summary>
		/// Method to process an individual .po file.
		/// </summary>
		/// <param name="currentFile">Path to a .po file</param>
		/// <param name="log">Output log of processing</param>
		/// <returns>True if successful, false if there is an error</returns>
		private bool ProcessFile(string currentFile, out string log)
		{
			// Get the language code embedded in the .po file name:
			var language = Path.GetFileNameWithoutExtension(currentFile).Replace("messages.", "", StringComparison.InvariantCultureIgnoreCase);
			// Get the path to the Bld directory:
			var bldDirectory = Path.Combine(m_fwRoot, "Bld");

			// Initiate log:
			log = "Localization for " + currentFile + " (language + " + language + "):" + Environment.NewLine;

			// Run findstr.exe utility to look for known issues and common errors:
			using (var findstrProc = new Process())
			{
				findstrProc.StartInfo.UseShellExecute = false;
				findstrProc.StartInfo.RedirectStandardOutput = true;
				if (IsUnix)
				{
					findstrProc.StartInfo.FileName = "/bin/grep";
					findstrProc.StartInfo.Arguments =
						"-e \"[{][oOlLi][}]\" -e \"[{][0-9][{]\" -e \"[}][0-9][}]\"" + Quote(currentFile);
				}
				else
				{
					findstrProc.StartInfo.FileName = "cmd.exe";
					findstrProc.StartInfo.Arguments =
						"/c findstr.exe /N \"{o} {O} {l} {L} {i} {0{ {1{ }0} }1}\" " + Quote(currentFile);
				}
				findstrProc.StartInfo.WorkingDirectory = bldDirectory;
				findstrProc.Start();

				// Grab the standard output:
				string output = findstrProc.StandardOutput.ReadToEnd();
				if (output.Length > 0)
				{
					log += "Found invalid strings in the " + currentFile + " PO data: " + Environment.NewLine +
						output + Environment.NewLine;
					return false;
				}

				findstrProc.WaitForExit();
					// This must come after findstrProc.StandardOutput.ReadToEnd() to avoid deadlocks.
			}

			// Run the po2xml.py utility:
			var po2xmlPath = Path.Combine(bldDirectory, "po2xml.py");
			var languageXml = Path.Combine(m_fwOutput, language + ".xml");

			using (var po2XmlProc = new Process())
			{
				po2XmlProc.StartInfo.UseShellExecute = false;
				po2XmlProc.StartInfo.RedirectStandardOutput = true;
				po2XmlProc.StartInfo.FileName = "python";
				po2XmlProc.StartInfo.Arguments = string.Format("{0} -o {1} {2}", Quote(po2xmlPath), Quote(languageXml), Quote(currentFile));
				po2XmlProc.StartInfo.WorkingDirectory = bldDirectory;
				po2XmlProc.Start();

				log += po2XmlProc.StandardOutput.ReadToEnd() + Environment.NewLine;

				po2XmlProc.WaitForExit();
				if (po2XmlProc.ExitCode != 0)
				{
					Console.WriteLine("Error: " + po2XmlProc + " returned error " + po2XmlProc.ExitCode +
						" for language " + language + ".");
					return false;
				}
			}

			// Run NAnt on target "buildLocalizedDlls":
			string nantLog;
			var nantRetVal = Nant("buildLocalizedDlls -D:language=" + language, language, out nantLog);

			log += nantLog + Environment.NewLine;

			if (nantRetVal != 0)
			{
				log += "Error: NAnt buildLocalizedDlls (" + language + ") returned " + nantRetVal + "." + Environment.NewLine;
				return false;
			}

			// Run the LocaleStrings.exe utility:
			var localeStringsPath = Path.Combine(m_fwRoot, Path.Combine("Bin", "LocaleStrings.exe"));
			var localeStringsArgs = "--root " + Quote(m_fwRoot) + " --store " + Quote(currentFile);

			using (var localeStringsProc = new Process())
			{
				localeStringsProc.StartInfo.UseShellExecute = false;
				localeStringsProc.StartInfo.RedirectStandardOutput = true;
				localeStringsProc.StartInfo.FileName = localeStringsPath;
				localeStringsProc.StartInfo.Arguments = localeStringsArgs;
				localeStringsProc.StartInfo.WorkingDirectory = bldDirectory;
				localeStringsProc.Start();

				log += localeStringsProc.StandardOutput.ReadToEnd() + Environment.NewLine;

				localeStringsProc.WaitForExit();
				if (localeStringsProc.ExitCode != 0)
				{
					Console.WriteLine("Error: " + localeStringsPath + " returned error " +
						localeStringsProc.ExitCode + " for language " + language + ".");
					return false;
				}

				// Return "success" code:
				return true;
			}
		}

		/// <summary>
		/// Runs NAnt on the given target(s).
		/// </summary>
		/// <param name="targets">The NAnt target(s) to run</param>
		/// <param name="language">The language code e.g. "es" or "zh-CN"</param>
		/// <param name="log">An output of status, error messages etc.</param>
		/// <returns>Exit code of NAnt process</returns>
		private int Nant(string targets, string language, out string log)
		{
			// TODO-Linux: needs to be done differently
			// Write a temporary batch file containing a few DOS commands needed to run NAnt in the right context:
			var batchFilePath = Path.Combine(m_fwRoot, language + "_nant.bat");
			var batchFile = new StreamWriter(batchFilePath);
			batchFile.WriteLine("set fwroot=" + m_fwRoot);
			batchFile.WriteLine("set path=%fwroot%\\DistFiles;%path%");
			batchFile.WriteLine("cd " + Path.Combine(m_fwRoot, "bld"));
			batchFile.WriteLine("..\\bin\\nant\\bin\\nant " + m_config + " " + targets);
			batchFile.Close();

			using (var nantProc = new Process())
			{
				nantProc.StartInfo.UseShellExecute = false;
				nantProc.StartInfo.RedirectStandardError = true;
				nantProc.StartInfo.RedirectStandardOutput = true;
				nantProc.StartInfo.FileName = batchFilePath;
				nantProc.Start();

				string output = nantProc.StandardOutput.ReadToEnd();
				string error = nantProc.StandardError.ReadToEnd();

				nantProc.WaitForExit();

				log = output + Environment.NewLine;

				if (error.Length > 0)
					log += language + ": Error calling NAnt target(s) " + targets + ": " +
						error + Environment.NewLine;

				File.Delete(batchFilePath);

				return nantProc.ExitCode;
			}
		}

		/// <summary>
		/// Puts quotation marks around given string.
		/// </summary>
		/// <param name="str">A string</param>
		/// <returns>Quoted string</returns>
		private static string Quote(string str)
		{
			return "\"" + str + "\"";
		}
	}

	public static class StringExtensions
	{
		/// <summary>
		/// An alternative to the regular String.Replace method. This one allows for case-insensitive searches.
		/// </summary>
		/// <param name="originalString">String to be searched</param>
		/// <param name="oldValue">Substring to look for</param>
		/// <param name="newValue">Substring to insert instead</param>
		/// <param name="comparisonType">Whether to use case-sensitivity, etc.</param>
		/// <returns>The modified string</returns>
		public static string Replace(this string originalString, string oldValue, string newValue, StringComparison comparisonType)
		{
			int startIndex = 0;
			while (true)
			{
				startIndex = originalString.IndexOf(oldValue, startIndex, comparisonType);
				if (startIndex == -1)
					break;

				originalString = originalString.Substring(0, startIndex) + newValue + originalString.Substring(startIndex + oldValue.Length);

				startIndex += newValue.Length;
			}

			return originalString;
		}
	}
}
