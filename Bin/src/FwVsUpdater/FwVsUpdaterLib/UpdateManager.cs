// ---------------------------------------------------------------------------------------------
#region // Copyright (c) 2006-2011, SIL International. All Rights Reserved.
// <copyright from='2006' to='2011' company='SIL International'>
//		Copyright (c) 2006, SIL International. All Rights Reserved.
//
//		Distributable under the terms of either the Common Public License or the
//		GNU Lesser General Public License, as specified in the LICENSING.txt file.
// </copyright>
#endregion
//
// File: Updater.cs
// Responsibility: TE Team
//
// <remarks>
// </remarks>
// ---------------------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.EnterpriseServices.Internal;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Win32;

namespace SIL.FieldWorks.DevTools.FwVsUpdater
{
	/// ----------------------------------------------------------------------------------------
	/// <summary>
	///
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	public class UpdateManager
	{
		private readonly string m_UpdatePath;
		private readonly bool m_fFirstTime;
		private readonly Hashtable m_FileHash = new Hashtable();
		private UpdateFile[] m_FilesToUpdate;
		private string m_VisualStudio;

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Initializes a new instance of the <see cref="T:Updater"/> class.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public UpdateManager(bool fFirstTime)
		{
			m_fFirstTime = fFirstTime;
			m_UpdatePath = Path.Combine(Properties.Settings.Default.FwRoot,
				Properties.Settings.Default.RemoteLocation);

			Debug.Assert((Properties.Settings.Default.FileHashKeys == null &&
				Properties.Settings.Default.FileHashValues == null) ||
				(Properties.Settings.Default.FileHashKeys.Count ==
				Properties.Settings.Default.FileHashValues.Count));
			if (Properties.Settings.Default.FileHashKeys != null)
			{
				for (int i = 0; i < Properties.Settings.Default.FileHashKeys.Count; i++)
				{
					m_FileHash.Add(Properties.Settings.Default.FileHashKeys[i],
						Properties.Settings.Default.FileHashValues[i]);
				}
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Returns <c>true</c> if we should perform an update check, otherwise <c>false</c>.
		/// We run update checks only once a day.
		/// </summary>
		/// <returns></returns>
		/// ------------------------------------------------------------------------------------
		private bool NeedCheck()
		{
			if (Properties.Settings.Default.LastCheck.Date < DateTime.Now.Date)
				return true;

			return false;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Calculates the hash value for the given file
		/// </summary>
		/// <param name="fileName">Name of the file.</param>
		/// <returns>MD5 hash value</returns>
		/// ------------------------------------------------------------------------------------
		private string ComputeHash(string fileName)
		{
			var md5 = new MD5CryptoServiceProvider();
			using (var reader = new StreamReader(fileName))
			{
				try
				{
					byte[] hash = md5.ComputeHash(reader.BaseStream);
					var bldr = new StringBuilder();
					foreach (byte b in hash)
						bldr.Append(b);
					return bldr.ToString();
				}
				finally
				{
					reader.Close();
				}
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Checks if updates are available
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private bool UpdatesAvailable()
		{
			XmlReader reader = XmlReader.Create(Path.Combine(m_UpdatePath, "Updates.xml"));
			var serializer = new XmlSerializer(typeof(UpdateDesc));
			var updateDesc = (UpdateDesc)serializer.Deserialize(reader);
			reader.Close();

			var filesToUpdate = new ArrayList();
			for (int i = 0; i < updateDesc.files.Length; i++)
			{
				try
				{
					var file = updateDesc.files[i];
					var hash = ComputeHash(Path.Combine(m_UpdatePath, file.Name));
					var storedHash = (string)m_FileHash[file.Name];

					if (hash != storedHash && (!file.Once || storedHash == null))
					{
						Trace.WriteLine(DateTime.Now + ": Need to update " + file.Name);

						if (!IncludeUpdate(file))
						{
							Trace.WriteLine(DateTime.Now + ": (Ignored because requested VS version " + file.VSVersion +
								" is not installed.)");
						}
						else if (!m_fFirstTime || !file.IgnoreFirstTime)
							filesToUpdate.Add(file);
						else
							Trace.WriteLine(DateTime.Now + ": (Ignored due to " + (m_fFirstTime ? "/first)" : "file props)"));

						// Store new hash value
						m_FileHash[file.Name] = hash;
					}
				}
				catch (Exception e)
				{
					Trace.WriteLine(DateTime.Now + ": Got exception " + e.Message);
				}
			}

			m_FilesToUpdate = new UpdateFile[filesToUpdate.Count];
			filesToUpdate.CopyTo(m_FilesToUpdate);
			return m_FilesToUpdate.Length > 0;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Checks if the version of Visual Studio specified in file.VSVersion is installed on
		/// this machine.
		/// </summary>
		/// <returns>Returns <c>true</c> if no specific VS version is specified or that version
		/// is installed, otherwise <c>false</c>.</returns>
		/// ------------------------------------------------------------------------------------
		private bool IncludeUpdate(UpdateFile file)
		{
			if (string.IsNullOrEmpty(file.VSVersion))
				return true;

			bool fInclude = false;
			foreach (var vsversion in file.VSVersion.Split(','))
			{
				int version;
				switch (vsversion.Trim())
				{
					case "2005":
						version = 8;
						break;
					case "2008":
						version = 9;
						break;
					case "2010":
						version = 10;
						break;
					default:
						Trace.WriteLine(DateTime.Now + ": Corrupt updates.xml file. Unknown VS version: " +
										file.VSVersion);
						return true;
				}
				// See http://www.mztools.com/articles/2008/MZ2008003.aspx on how to detect which VS
				// version is installed.
				var key = Registry.ClassesRoot.OpenSubKey(string.Format("VisualStudio.DTE.{0}.0", version));
				fInclude |= (key != null);
				if (key != null)
				{
					key.Close();
					break;
				}
			}
			return fInclude;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Updates all files that require an update.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void UpdateFiles()
		{
			bool fAskedUserToExit = false; // will be set to true as soon as we have asked user
			using (var dlg = new ProgressDialog())
			{
				dlg.ProgressBar.Maximum = m_FilesToUpdate.Length;
				dlg.Show();
				for (int i = 0; i < m_FilesToUpdate.Length && !dlg.ShouldCancel; i++)
				{
					UpdateFile file = m_FilesToUpdate[i];
					Trace.WriteLine(DateTime.Now + ": Installing " + file.Name);
					dlg.InstallText = "Installing " + file.Name;

					if (file.ExitVs && !fAskedUserToExit)
					{
						fAskedUserToExit = true;
						CloseVs();
					}

					try
					{
						string extension = Path.GetExtension(file.Name).ToLower();
						if (extension == ".msi")
						{
							// Install this file
							using (var process = new Process())
							{
								process.StartInfo.FileName = Path.Combine(m_UpdatePath, file.Name);
								process.StartInfo.CreateNoWindow = true;
								process.StartInfo.Arguments = "/quiet";
								process.Start();
								process.WaitForExit();
								Trace.WriteLine(DateTime.Now + ": Exit code: " + process.ExitCode);
							}
						}
						else if (extension == ".vsi" || extension == ".vsix")
						{
							// Install this file
							using (var process = new Process())
							{
								process.StartInfo.FileName = "vsixinstaller";
								process.StartInfo.CreateNoWindow = true;
								process.StartInfo.Arguments = string.Format("/q /i \"{0}\"", Path.Combine(m_UpdatePath, file.Name));
								process.Start();
								process.WaitForExit();
								Trace.WriteLine(DateTime.Now + ": Exit code: " + process.ExitCode);
							}
						}
						else if (extension == ".dll")
						{
							// Install in the GAC
							string fileName = Path.Combine(m_UpdatePath, file.Name);
							var publish = new Publish();
							publish.GacInstall(fileName);
							Trace.WriteLine(DateTime.Now + ": Installed in GAC");
						}
					}
					catch (Exception e)
					{
						Trace.WriteLine(DateTime.Now + ": Got exception installing " +
							file.Name + ": " + e.Message);
						// remove from list of updated files so that we try again
						m_FileHash.Remove(file.Name);
					}
					dlg.ProgressBar.Increment(1);
				}

				RestartVisualStudio(dlg);
				dlg.Close();
			}
		}


		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Closes Visual Studio if it is running.
		/// </summary>
		/// <returns>Returns <c>true</c> if VS was closed or not running, <c>false</c> if
		/// user didn't want to close.</returns>
		/// ------------------------------------------------------------------------------------
		private bool CloseVs()
		{
			Process[] processes = Process.GetProcessesByName("devenv");
			if (processes.Length > 0)
			{
				if (MessageBox.Show("Visual Studio needs to close before update can proceed",
					"Updates available", MessageBoxButtons.OKCancel) == DialogResult.OK)
				{
					foreach (var process in processes)
					{
						Trace.WriteLine(DateTime.Now + ": Closing " + process.MainWindowTitle);
						m_VisualStudio = process.MainModule.FileName;
						process.CloseMainWindow();
						process.WaitForExit();
					}
					return true;
				}
				Trace.WriteLine(DateTime.Now + ": Skipped closing Visual Studio by user's request");
				return false;
			}
			return true;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Restarts Visual Studio.
		/// </summary>
		/// <param name="dlg">The progress dialog or <c>null</c>.</param>
		/// ------------------------------------------------------------------------------------
		private void RestartVisualStudio(ProgressDialog dlg)
		{
			// if we had to close VS, we restart it now.
			if (m_VisualStudio != null)
			{
				try
				{
					if (dlg != null)
						dlg.InstallText = "Restarting Visual Studio";
					using (var process = new Process())
					{
						process.StartInfo.FileName = m_VisualStudio;
						bool fSuccess = process.Start();
						Trace.WriteLine(DateTime.Now + ": Restarting Visual Studio from " +
										m_VisualStudio + ". Result: " + fSuccess);
						m_VisualStudio = null;
					}
				}
				catch (Exception e)
				{
					Trace.WriteLine(DateTime.Now + ": Got exception restarting Visual Studio: " +
						e.Message);
				}
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Close Visual Studio if any of the updates requires it to be closed.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void CloseVsIfNeeded()
		{
			foreach (var file in m_FilesToUpdate)
			{
				if (file.ExitVs)
				{
					if (!CloseVs())
						break;
				}
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Copies the array.
		/// </summary>
		/// <param name="src">The source.</param>
		/// <returns></returns>
		/// ------------------------------------------------------------------------------------
		private StringCollection CopyArray(ICollection src)
		{
			var dest = new StringCollection();
			foreach (string s in src)
				dest.Add(s);

			return dest;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Executes the external updater application.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void StartUpdater()
		{
			using (var process = new Process())
			{
				process.StartInfo.FileName = Path.Combine(Path.GetDirectoryName(
					Assembly.GetExecutingAssembly().Location), "FwVsUpdater.exe");
				process.StartInfo.CreateNoWindow = true;
				var bldr = new StringBuilder();
				if (m_fFirstTime)
					bldr.Append("/first ");
				bldr.AppendFormat("\"{0}\"", Properties.Settings.Default.FwRoot);
				process.StartInfo.Arguments = bldr.ToString();
				process.Start();
				process.WaitForExit();
				Trace.WriteLine(DateTime.Now + ": Exit code: " + process.ExitCode);
			}
		}

		/// ------------------------------------------------------------------------------------------
		/// <summary>
		/// Updates this instance.
		/// </summary>
		/// ------------------------------------------------------------------------------------------
		public void CheckForUpdateFiles(bool fForce)
		{
			// when we're restarting VS we don't want to display the "Updates available" dialog again.
			if (Properties.Settings.Default.UpdateInProgress)
			{
				Trace.WriteLine(DateTime.Now + ": Update in progress");
				return;
			}

			if (fForce || NeedCheck())
			{
				try
				{
					Properties.Settings.Default.UpdateInProgress = true;
					Properties.Settings.Default.Save();

					Trace.WriteLine(DateTime.Now + ": Checking for updates");
					if (fForce)
						Trace.WriteLine(DateTime.Now + ": Forcing check");

					if (UpdatesAvailable())
					{
						Trace.WriteLine(DateTime.Now + ": New updates available");
						if (MessageBox.Show("Updates for FieldWorks developer tools available. Install now?",
							"Update available", MessageBoxButtons.YesNo) == DialogResult.Yes)
						{
							CloseVsIfNeeded();

							try
							{
								StartUpdater();
								Properties.Settings.Default.FileHashKeys = CopyArray(m_FileHash.Keys);
								Properties.Settings.Default.FileHashValues = CopyArray(m_FileHash.Values);
								MessageBox.Show("Done installing updates!", "FW Developer Tools Updater");
							}
							catch (Exception e)
							{
								MessageBox.Show("Installing updates failed. Got exception: " + e.Message,
									"FW Developer Tools Updater");
								Trace.WriteLine(string.Format("{3}: Installing updates failed. Got exception: {0}{1}{2}{1}{3}",
									e.Message, Environment.NewLine, e, DateTime.Now));
								throw;
							}

							RestartVisualStudio(null);
						}
						else
							Trace.WriteLine(DateTime.Now + ": Skipping update by user request");
					}

					Properties.Settings.Default.LastCheck = DateTime.Now;
					Properties.Settings.Default.Save();
				}
				finally
				{
					Properties.Settings.Default.UpdateInProgress = false;
					Properties.Settings.Default.Save();
				}
			}
			else
				Trace.WriteLine(DateTime.Now + ": Already checked for updates today");
		}

		/// ------------------------------------------------------------------------------------------
		/// <summary>
		/// Updates this instance.
		/// </summary>
		/// ------------------------------------------------------------------------------------------
		public void InstallUpdates()
		{
			if (UpdatesAvailable())
			{
				Trace.WriteLine(DateTime.Now + ": Installing updates");
				try
				{
					UpdateFiles();
					Properties.Settings.Default.FileHashKeys = CopyArray(m_FileHash.Keys);
					Properties.Settings.Default.FileHashValues = CopyArray(m_FileHash.Values);
				}
				catch (Exception e)
				{
					MessageBox.Show("Installing updates failed. Got exception: " + e.Message,
						"FW Developer Tools Updater");
					Trace.WriteLine(string.Format("{3}: Installing updates failed. Got exception: {0}{1}{2}{1}{3}",
						e.Message, Environment.NewLine, e, DateTime.Now));
					throw;
				}
			}
			else
				Trace.WriteLine(DateTime.Now + ": No updates to install");
		}
	}
}
