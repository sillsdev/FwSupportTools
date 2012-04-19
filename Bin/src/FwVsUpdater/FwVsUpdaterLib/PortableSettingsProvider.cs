/*************************************************************
 * PortableSettingsProvider.cs
 * Portable Settings Provider for C# applications
 *
 * 2010- Michael Nathan
 * http://www.Geek-Republic.com
 *
 * Licensed under Creative Commons CC BY-SA
 * http://creativecommons.org/licenses/by-sa/3.0/legalcode
 *
 * Slightly modified by Eberhard Beilharz, 03-2012
 *
 *************************************************************/
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Win32;
using System.Xml;
using System.Xml.Serialization;

namespace SIL.FieldWorks.DevTools.FwVsUpdater
{
	/// <summary>
	/// Settings provider that reads/writes from an XML file
	/// in the directory where the executable is.
	/// </summary>
	public class PortableSettingsProvider : SettingsProvider
	{
		// Define some static strings later used in our XML creation
		// XML Root node
		private const string Xmlroot = "configuration";

		// Configuration declaration node
		private const string ConfigNode = "configSections";

		// Configuration section group declaration node
		private const string GroupNode = "sectionGroup";

		// User section node
		private const string UserNode = "userSettings";

		// Application Specific Node
		private string AppNode = Assembly.GetExecutingAssembly().GetName().Name +
			".Properties.Settings";

		private XmlDocument m_XmlDoc;

		/// <summary>
		/// Override the Initialize method
		/// </summary>
		public override void Initialize(string name, NameValueCollection config)
		{
			base.Initialize(ApplicationName, config);
		}

		/// <summary>
		/// Override the ApplicationName property, returning the solution name.  No need to set
		/// anything, we just need to retrieve information, though the set method still needs
		/// to be defined.
		/// </summary>
		public override string ApplicationName
		{
			get { return (Assembly.GetExecutingAssembly().GetName().Name); }
			set { }
		}

		/// <summary>
		/// Simply returns the name of the settings file, which is the solution name plus
		/// ".config"
		/// </summary>
		public virtual string GetSettingsFilename()
		{
			return ApplicationName + ".config";
		}

		/// <summary>
		/// Gets current executable path in order to determine where to read and write the
		/// config file
		/// </summary>
		public virtual string GetAppPath()
		{
			return new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName;
		}

		/// <summary>
		/// Retrieve settings from the configuration file
		/// </summary>
		public override SettingsPropertyValueCollection GetPropertyValues(SettingsContext sContext,
			SettingsPropertyCollection settingsColl)
		{
			// Create a collection of values to return
			var retValues = new SettingsPropertyValueCollection();

			// Loop through the list of settings that the application has requested and add them
			// to our collection of return values.
			foreach (SettingsProperty sProp in settingsColl)
			{
				var setVal = new SettingsPropertyValue(sProp);
				setVal.IsDirty = false;
				setVal.SerializedValue = GetSetting(sProp);
				retValues.Add(setVal);
			}
			return retValues;
		}

		/// <summary>
		/// Save any of the applications settings that have changed (flagged as "dirty")
		/// </summary>
		public override void SetPropertyValues(SettingsContext sContext,
			SettingsPropertyValueCollection settingsColl)
		{
			// Set the values in XML
			foreach (SettingsPropertyValue value in settingsColl)
			{
				if (!value.IsDirty || value.SerializedValue == null)
					continue;

				SetSetting(value);
			}

			// Write the XML file to disk
			try
			{
				XmlConfig.Save(Path.Combine(GetAppPath(), GetSettingsFilename()));
			}
			catch (Exception ex)
			{
				// Create an informational message for the user if we cannot save the settings.
				// Enable whichever applies to your application type.

				// Uncomment the following line to enable a console message for console-based apps
				Console.WriteLine("Error writing configuration file to disk: " + ex.Message);
			}
		}

		private XmlDocument XmlConfig
		{
			get
			{
				// Check if we already have accessed the XML config file. If the xmlDoc object is empty, we have not.
				if (m_XmlDoc == null)
				{
					m_XmlDoc = new XmlDocument();

					// If we have not loaded the config, try reading the file from disk.
					try
					{
						m_XmlDoc.Load(Path.Combine(GetAppPath(), GetSettingsFilename()));
					}
					catch (Exception)
					{ // If the file does not exist on disk, catch the exception then create the XML template for the file.
						// XML Declaration
						// <?xml version="1.0" encoding="utf-8"?>
						var dec = m_XmlDoc.CreateXmlDeclaration("1.0", "utf-8", null);
						m_XmlDoc.AppendChild(dec);

						// Create root node and append to the document
						// <configuration>
						var rootNode = m_XmlDoc.CreateElement(Xmlroot);
						m_XmlDoc.AppendChild(rootNode);

						// Create Configuration Sections node and add as the first node under the root
						// <configSections>
						var configNode = m_XmlDoc.CreateElement(ConfigNode);
						m_XmlDoc.DocumentElement.PrependChild(configNode);

						// Create the user settings section group declaration and append to the config node above
						// <sectionGroup name="userSettings"...>
						var groupNode = m_XmlDoc.CreateElement(GroupNode);
						groupNode.SetAttribute("name", UserNode);
						groupNode.SetAttribute("type", "System.Configuration.UserSettingsGroup");
						configNode.AppendChild(groupNode);

						// Create the Application section declaration and append to the groupNode above
						// <section name="AppName.Properties.Settings"...>
						var newSection = m_XmlDoc.CreateElement("section");
						newSection.SetAttribute("name", AppNode);
						newSection.SetAttribute("type", "System.Configuration.ClientSettingsSection");
						groupNode.AppendChild(newSection);

						// Create the userSettings node and append to the root node
						// <userSettings>
						var userNode = m_XmlDoc.CreateElement(UserNode);
						m_XmlDoc.DocumentElement.AppendChild(userNode);

						// Create the Application settings node and append to the userNode above
						// <AppName.Properties.Settings>
						var appNode = m_XmlDoc.CreateElement(AppNode);
						userNode.AppendChild(appNode);
					}
				}
				return m_XmlDoc;
			}
		}

		// Retrieve values from the configuration file, or if the setting does not exist in the file,
		// retrieve the value from the application's default configuration
		private object GetSetting(SettingsProperty setProp)
		{
			try
			{
				// Search for the specific settings node we are looking for in the configuration file.
				var childNode = XmlConfig.SelectSingleNode("//setting[@name='" + setProp.Name + "']").FirstChild;
				return setProp.SerializeAs == SettingsSerializeAs.String ? childNode.InnerText : childNode.InnerXml;
			}
			catch (Exception)
			{
				// Check to see if a default value is defined by the application.
				// If so, return that value, using the same rules for settings stored as Strings and XML as above
				if (setProp.DefaultValue != null)
				{
					if (setProp.SerializeAs == SettingsSerializeAs.String)
						return setProp.DefaultValue.ToString();

					var settingType = setProp.PropertyType.ToString();
					var xmlData = setProp.DefaultValue.ToString();
					var xs = new XmlSerializer(typeof(string[]));
					var data = (string[])xs.Deserialize(new XmlTextReader(xmlData, XmlNodeType.Element, null));

					switch (settingType)
					{
						case "System.Collections.Specialized.StringCollection":
							var sc = new StringCollection();
							sc.AddRange(data);
							return sc;
					}
				}
			}
			return string.Empty;
		}

		private void SetSetting(SettingsPropertyValue setProp)
		{
			// Define the XML path under which we want to write our settings if they do not already exist
			XmlNode settingNode;

			try
			{
				// Search for the specific settings node we want to update.
				// If it exists, return its first child node, (the <value>data here</value> node)
				settingNode = XmlConfig.SelectSingleNode("//setting[@name='" + setProp.Name + "']").FirstChild;
			}
			catch (Exception)
			{
				settingNode = null;
			}

			// If we have a pointer to an actual XML node, update the value stored there
			if ((settingNode != null))
			{
				SerializeProperty(setProp, settingNode);
			}
			else
			{
				// If the value did not already exist in this settings file, create a new entry for this setting

				// Search for the application settings node (<Appname.Properties.Settings>) and store it.
				var appNode = XmlConfig.SelectSingleNode("//" + AppNode);

				// Create a new settings node and assign its name as well as how it will be serialized
				var newSetting = m_XmlDoc.CreateElement("setting");
				newSetting.SetAttribute("name", setProp.Name);

				newSetting.SetAttribute("serializeAs",
					setProp.Property.SerializeAs == SettingsSerializeAs.String ? "String" : "Xml");

				// Append this node to the application settings node (<Appname.Properties.Settings>)
				appNode.AppendChild(newSetting);

				// Create an element under our named settings node, and assign it the value we are trying to save
				var valueElement = m_XmlDoc.CreateElement("value");
				SerializeProperty(setProp, valueElement);

				//Append this new element under the setting node we created above
				newSetting.AppendChild(valueElement);
			}
		}

		private static void SerializeProperty(SettingsPropertyValue property, XmlNode settingNode)
		{
			if (property.Property.SerializeAs == SettingsSerializeAs.String)
				settingNode.InnerText = property.SerializedValue.ToString();
			else
			{
				// Write the object to the config serialized as Xml - we must remove the Xml declaration when writing
				// the value, otherwise .Net's configuration system complains about the additional declaration.
				settingNode.InnerXml = property.SerializedValue.ToString().Replace(
					@"<?xml version=""1.0"" encoding=""utf-16""?>", string.Empty);
			}
		}
	}
}
