// ---------------------------------------------------------------------------------------------
#region // Copyright (c) 2011-2011, SIL International. All Rights Reserved.
// <copyright from='2011' to='2011' company='SIL International'>
//		Copyright (c) 2011, SIL International. All Rights Reserved.
//
//		Distributable under the terms of either the Common Public License or the
//		GNU Lesser General Public License, as specified in the LICENSING.txt file.
// </copyright>
#endregion
// ---------------------------------------------------------------------------------------------
using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;

namespace SIL.FieldWorks.DevTools.FwVsUpdater
{
	/// ----------------------------------------------------------------------------------------
	/// <summary>
	/// Provides a settings provider for a library. Allows clients of the library to share
	/// settings.
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	public class LibSettingsProvider: SettingsProvider, IApplicationSettingsProvider
	{
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the name of the currently running application.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public override string ApplicationName
		{
			get { return Path.GetFileName(Assembly.GetCallingAssembly().Location); }
			set
			{
				// do nothing
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Initializes the provider.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public override void Initialize(string name, NameValueCollection config)
		{
			base.Initialize(ApplicationName, config);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Writes <paramref name="values"/> properties to config files.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public override void SetPropertyValues(SettingsContext context,
			SettingsPropertyValueCollection values)
		{
			var configMap = SetConfigFiles();
			var localConfig = ConfigurationManager.OpenMappedExeConfiguration(configMap,
				ConfigurationUserLevel.PerUserRoamingAndLocal);
			var roamingConfig = ConfigurationManager.OpenMappedExeConfiguration(configMap,
				ConfigurationUserLevel.PerUserRoaming);
			var groupName = (string)context["GroupName"];

			var localCollection = GetSection(localConfig, groupName).Settings;
			var roamingCollection = GetSection(roamingConfig, groupName).Settings;

			// Create new collection of values
			foreach (SettingsPropertyValue value in values)
			{
				if (!value.IsDirty || value.SerializedValue == null)
					continue;

				if (IsApplicationScoped(value.Property))
					continue;

				if (value.PropertyValue.Equals(value.Property.DefaultValue))
					continue;

				SettingElement elem;
				if (value.Property.Attributes[typeof(SettingsManageabilityAttribute)] == null)
				{
					// this is a property for a local user
					elem = localCollection.Get(value.Name);
					if (elem == null)
					{
						elem = new SettingElement { Name = value.Name };
						localCollection.Add(elem);
					}
				}
				else
				{
					// this is a property for a roaming user
					elem = roamingCollection.Get(value.Name);
					if (elem == null)
					{
						elem = new SettingElement { Name = value.Name };
						roamingCollection.Add(elem);
					}
				}
				elem.SerializeAs = value.Property.SerializeAs;
				elem.Value.ValueXml = SerializeToXmlElement(value);
			}

			if (localCollection.Count > 0)
				localConfig.Save();
			if (roamingCollection.Count > 0)
				roamingConfig.Save();
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the property values from a config file.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public override SettingsPropertyValueCollection GetPropertyValues(SettingsContext context,
			SettingsPropertyCollection properties)
		{
			// Set the config files
			var configMap = SetConfigFiles();

			// Create new collection of values
			var values = new SettingsPropertyValueCollection();

			ReadProperties(context, properties, configMap, ConfigurationUserLevel.None,
				values);
			ReadProperties(context, properties, configMap,
				ConfigurationUserLevel.PerUserRoamingAndLocal, values);
			ReadProperties(context, properties, configMap, ConfigurationUserLevel.PerUserRoaming,
				values);

			// save new user config file
			try
			{
				SetPropertyValues(context, values);
			}
			catch
			{
			}

			return values;
		}

		private static bool IsRoaming(SettingsProperty prop)
		{
			return prop.Attributes[typeof(SettingsManageabilityAttribute)] != null;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Looks in the "attribute bag" for a given property to determine if it is app-scoped
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private static bool IsApplicationScoped(SettingsProperty prop)
		{
			return HasSettingScope(prop, typeof(ApplicationScopedSettingAttribute));
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Looks in the "attribute bag" for a given property to determine if it is user-scoped
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private static bool IsUserScoped(SettingsProperty prop)
		{
			return HasSettingScope(prop, typeof(UserScopedSettingAttribute));
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Checks for app or user-scoped based on the attributeType argument
		/// Also checks for sanity, i.e. a setting not marked as both or neither scope
		/// (just like the LocalFileSettingsProvider)
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private static bool HasSettingScope(SettingsProperty prop, Type attributeType)
		{
			// TODO: add support for roaming
			Debug.Assert((attributeType == typeof(ApplicationScopedSettingAttribute)) ||
				(attributeType == typeof(UserScopedSettingAttribute)));
			bool isAppScoped = (prop.Attributes[typeof(ApplicationScopedSettingAttribute)] != null);
			bool isUserScoped = (prop.Attributes[typeof(UserScopedSettingAttribute)] != null);

			// Check constraints
			if (isUserScoped && isAppScoped)
				throw new ApplicationException("Can't mark a setting as User and Application-scoped: " + prop.Name);

			if (!isUserScoped && !isAppScoped)
				throw new ApplicationException("Must mark a setting as User or Application-scoped: " + prop.Name);

			// Return scope check result
			return attributeType == typeof(ApplicationScopedSettingAttribute) ? isAppScoped : isUserScoped;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the section <paramref name="sectionName"/> in the section group "userSettings".
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private static ClientSettingsSection GetSection(Configuration config, string sectionName,
			string sectionGroup = "userSettings")
		{
			var userSettingsGroup = config.GetSectionGroup(sectionGroup);
			if (userSettingsGroup == null)
			{
				userSettingsGroup = new ConfigurationSectionGroup();
				config.SectionGroups.Add(sectionGroup, userSettingsGroup);
			}
			var section = userSettingsGroup.Sections[sectionName] as ClientSettingsSection;
			if (section == null)
			{
				section = new ClientSettingsSection();
				userSettingsGroup.Sections.Add(sectionName, section);
			}
			return section;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Reads <paramref name="values"/> properties from the config file in
		/// <paramref name="configMap"/>. Which config file to read is specified in
		/// <paramref name="userLevel"/>.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private static bool ReadProperties(SettingsContext context,
			SettingsPropertyCollection properties, ExeConfigurationFileMap configMap,
			ConfigurationUserLevel userLevel, SettingsPropertyValueCollection values)
		{
			var isReadingAppConfig = (userLevel == ConfigurationUserLevel.None);
			var isReadingRoamingConfig = (userLevel == ConfigurationUserLevel.PerUserRoaming);
			var config = ConfigurationManager.OpenMappedExeConfiguration(configMap, userLevel);
			var groupName = (string)context["GroupName"];
			var configSettings = GetSection(config, groupName,
				isReadingAppConfig ? "applicationSettings" : "userSettings");

			// Create new collection of values
			foreach (SettingsProperty setting in properties)
			{
				var value = values[setting.Name];
				if (value == null)
				{
					values.Add(new SettingsPropertyValue(setting));
					value = values[setting.Name];
					//value.IsDirty = true;
					//value.SerializedValue = setting.DefaultValue;
				}

				if (IsUserScoped(setting) && isReadingRoamingConfig && !IsRoaming(setting) ||
					!isReadingAppConfig && IsApplicationScoped(setting))
				{
					continue;
				}

				var elem = configSettings.Settings.Get(setting.Name);
				if (elem != null)
					SetProperty(value, elem);
			}

			return config.HasFile;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Extracts a property from the XML element.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private static void SetProperty(SettingsPropertyValue property, SettingElement elem)
		{
			property.SerializedValue = elem.Value.ValueXml.InnerText;
			property.IsDirty = true;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Sets the configuration files.
		/// </summary>
		/// <returns>File mapping for configuration files</returns>
		/// ------------------------------------------------------------------------------------
		private ExeConfigurationFileMap SetConfigFiles()
		{
			var configMap = new ExeConfigurationFileMap();
			string configFilename = Path.Combine(Application.CompanyName, Path.Combine(ApplicationName, "user.config"));
			configMap.LocalUserConfigFilename = Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
				configFilename);
			configMap.RoamingUserConfigFilename = Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), configFilename);
			configMap.ExeConfigFilename = Assembly.GetCallingAssembly().Location + ".config";
			return configMap;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Serializes to XML element.
		/// </summary>
		/// <param name="value">The property.</param>
		/// <returns></returns>
		/// ------------------------------------------------------------------------------------
		private static XmlNode SerializeToXmlElement(SettingsPropertyValue value)
		{
			var element = new XmlDocument().CreateElement("value");
			var serializedString = value.SerializedValue as string;
			if (serializedString == null && value.Property.SerializeAs == SettingsSerializeAs.Binary)
			{
				if (value.SerializedValue is byte[])
					serializedString = Convert.ToBase64String(value.SerializedValue as byte[]);
			}
			//if (serializedString == null)
			//    serializedString = string.Empty;
			element.InnerText = serializedString;
			return element;
		}

		#region IApplicationSettingsProvider Members

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the previous version of a property.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public SettingsPropertyValue GetPreviousVersion(SettingsContext context, SettingsProperty prop)
		{
			// always return an empty property
			return new SettingsPropertyValue(prop) { PropertyValue = null };
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Resets the application settings associated with the specified application to their default values.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public void Reset(SettingsContext context)
		{
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Indicates to the provider that the application has been upgraded. This offers the
		/// provider an opportunity to upgrade its stored settings as appropriate.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public void Upgrade(SettingsContext context, SettingsPropertyCollection properties)
		{
		}

		#endregion
	}
}
