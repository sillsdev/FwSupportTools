// ---------------------------------------------------------------------------------------------
#region // Copyright (c) 2010, SIL International. All Rights Reserved.
// <copyright from='2010' to='2010' company='SIL International'>
//		Copyright (c) 2010, SIL International. All Rights Reserved.
//
//		Distributable under the terms of either the Common Public License or the
//		GNU Lesser General Public License, as specified in the LICENSING.txt file.
// </copyright>
#endregion
//
// File: ModelClass.cs
// Responsibility: mcconnel
//
// <remarks>
// </remarks>
// ---------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;

namespace SIL.FieldWorks.src.GenerateFwSchema
{
	/// ----------------------------------------------------------------------------------------
	/// <summary>
	/// This contains the relevant details for a single class in the model.
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	public class ModelClass
	{
		string m_sName;
		bool m_fAbstract;
		string m_sBase;
		List<ModelProperty> m_props = new List<ModelProperty>();

		/// <summary>
		/// Constructor.
		/// </summary>
		public ModelClass(string sName, bool fAbstract, string sBase)
		{
			m_sName = sName;
			m_fAbstract = fAbstract;
			m_sBase = sBase;
		}

		/// <summary>
		/// Class Name
		/// </summary>
		public string Name
		{
			get { return m_sName; }
		}

		/// <summary>
		/// Whether class is abstract
		/// </summary>
		public bool IsAbstract
		{
			get { return m_fAbstract; }
		}

		/// <summary>
		/// Name of the base class
		/// </summary>
		public string BaseClass
		{
			get { return m_sBase; }
		}

		/// <summary>
		/// List of class properties
		/// </summary>
		public List<ModelProperty> Properties
		{
			get { return m_props; }
		}

		/// <summary>
		/// Write a RelaxNG schema &lt;define&gt; element for this model class.
		/// </summary>
		public void WriteRngDefine(TextWriter tw, Dictionary<string, ModelClass> mapNameClass, bool fOld)
		{
			tw.WriteLine("<define name=\"{0}\">", this.Name);
			tw.WriteLine("    <attribute name=\"class\"><value>{0}</value></attribute>", this.Name);
			int cprop = TotalPropertyCount(mapNameClass);
			if (fOld)
			{
				tw.WriteLine("    <interleave>");
				if (!String.IsNullOrEmpty(this.BaseClass))
					WriteBaseClassAndProperties(tw, this.BaseClass, mapNameClass);
				tw.WriteLine("        <optional>");
				tw.WriteLine("        <element name=\"{0}\">", this.Name);
				if (this.Properties.Count == 0)
				{
					tw.WriteLine("        <empty/>");
				}
				else
				{
					if (this.Properties.Count > 1)
						tw.WriteLine("        <interleave>");
					this.WriteProperties(tw);
					if (this.Properties.Count > 1)
						tw.WriteLine("        </interleave>");
				}
				tw.WriteLine("        </element>");
				tw.WriteLine("        </optional>");
				tw.WriteLine("    </interleave>");
			}
			else
			{
				if (cprop == 0)
				{
					tw.WriteLine("    <empty/>");
				}
				else
				{
					tw.WriteLine("    <interleave>");
					if (!String.IsNullOrEmpty(this.BaseClass))
						WriteBaseClassProperties(tw, this.BaseClass, mapNameClass);
					WriteProperties(tw);
					tw.WriteLine("        <optional><ref name=\"CustomFields\"/></optional>");
					tw.WriteLine("    </interleave>");
				}
			}
			tw.WriteLine("</define>");
			tw.WriteLine();
		}

		private void WriteBaseClassAndProperties(TextWriter tw, string sBaseClass, Dictionary<string, ModelClass> mapNameClass)
		{
			ModelClass mcBase = mapNameClass[sBaseClass];
			if (!String.IsNullOrEmpty(mcBase.BaseClass))
				mcBase.WriteBaseClassAndProperties(tw, mcBase.BaseClass, mapNameClass);
			tw.WriteLine("        <optional>");
			tw.WriteLine("        <element name=\"{0}\">", sBaseClass);
			if (sBaseClass == "CmObject")
			{
				tw.WriteLine("        <optional><ref name=\"CustomFields\"/></optional>");
			}
			else
			{
				if (mcBase.Properties.Count == 0)
				{
					tw.WriteLine("        <empty/>");
				}
				else
				{
					if (mcBase.Properties.Count > 1)
						tw.WriteLine("        <interleave>");
					mcBase.WriteProperties(tw);
					if (mcBase.Properties.Count > 1)
						tw.WriteLine("        </interleave>");
				}
			}
			tw.WriteLine("        </element>");
			tw.WriteLine("        </optional>");
		}

		private int TotalPropertyCount(Dictionary<string, ModelClass> mapNameClass)
		{
			int cprop = this.Properties.Count;
			if (!String.IsNullOrEmpty(this.BaseClass))
			{
				ModelClass mcBase = mapNameClass[this.BaseClass];
				cprop += mcBase.TotalPropertyCount(mapNameClass);
			}
			return cprop;
		}

		private void WriteProperties(TextWriter tw)
		{
			foreach (ModelProperty mprop in this.Properties)
			{
				tw.Write("        <optional><element name=\"{0}\">", mprop.Name);
				switch (mprop.Type)
				{
					case ModelProperty.Types.Basic:
						tw.Write("<ref name=\"basic-{0}\"/>", mprop.Sig);
						break;
					case ModelProperty.Types.Owning:
						if (mprop.IsAtomic)
							tw.Write("<ref name=\"owning-atomic\"/>");
						else
							tw.Write("<ref name=\"owning-multiple\"/>");
						break;
					case ModelProperty.Types.Reference:
						if (mprop.IsAtomic)
							tw.Write("<ref name=\"reference-atomic\"/>");
						else
							tw.Write("<ref name=\"reference-multiple\"/>");
						break;
				}
				tw.WriteLine("</element></optional>");
			}
		}

		private void WriteBaseClassProperties(TextWriter tw, string sBaseClass,
			Dictionary<string, ModelClass> mapNameClass)
		{
			ModelClass mcBase = mapNameClass[sBaseClass];
			if (!String.IsNullOrEmpty(mcBase.BaseClass))
				mcBase.WriteBaseClassProperties(tw, mcBase.BaseClass, mapNameClass);
			mcBase.WriteProperties(tw);
		}

		/// <summary>
		/// Write a DTD ELEMENT definition for this class.  This is useful only for old style
		/// files.
		/// </summary>
		public void WriteOldDtdElement(TextWriter tw, Dictionary<string, List<ModelProperty>> mapNameProps)
		{
			tw.Write("<!ELEMENT {0} ", this.Name);
			int cProps = this.Properties.Count;
			List<ModelProperty> propsWithName = null;
			if (mapNameProps.TryGetValue(this.Name, out propsWithName))
			{
				mapNameProps.Remove(this.Name);
				cProps += propsWithName.Count;
			}
			if (cProps == 0)
			{
				if (this.Name == "CmObject")
					tw.Write("(Custom)*");
				else
					tw.Write("EMPTY");
			}
			else
			{
				tw.Write("(");
				bool fFirst = true;
				foreach (ModelProperty prop in this.Properties)
				{
					if (!fFirst)
						tw.Write(" | ");
					tw.Write(prop.Name);
					fFirst = false;
				}
				if (propsWithName != null)
				{
					foreach (ModelProperty mprop in propsWithName)
					{
						if (!fFirst)
							tw.Write(" | ");
						mprop.WritePartialDtdElement(tw);
						fFirst = false;
					}
				}
				tw.Write(")");
				if (cProps > 1)
					tw.Write("*");
				else
					tw.Write("?");
			}
			tw.WriteLine(">");
			if (propsWithName != null)
			{
				List<string> attrs = new List<string>();
				foreach (ModelProperty mprop in propsWithName)
					mprop.AddDtdAttrs(attrs);
				if (attrs.Count > 0)
				{
					tw.WriteLine("<!ATTLIST {0}", this.Name);
					foreach (string attr in attrs)
						tw.WriteLine(attr);
					tw.WriteLine(">");
				}
			}
			tw.WriteLine();
		}
	}

	/// ----------------------------------------------------------------------------------------
	/// <summary>
	/// This contains the relevant details of a single property in the model.
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	public class ModelProperty
	{
		/// <summary>
		/// General types of properties.
		/// </summary>
		public enum Types
		{
			/// <summary>basic: string, integer, boolean, etc.</summary>
			Basic,
			/// <summary>owns another object</summary>
			Owning,
			/// <summary>refers to another object</summary>
			Reference
		};
		Types m_type;
		string m_sName;
		string m_sSig;
		bool m_fAtomic;

		/// <summary>
		/// Constructor
		/// </summary>
		public ModelProperty(string sName, Types type, string sSig, bool fAtomic)
		{
			m_sName = sName;
			m_type = type;
			m_sSig = sSig;
			m_fAtomic = fAtomic;
		}

		/// <summary>
		/// General type of the property
		/// </summary>
		public Types Type
		{
			get { return m_type; }
		}

		/// <summary>
		/// Name of the property
		/// </summary>
		public string Name
		{
			get { return m_sName; }
		}

		/// <summary>
		/// Signature of the property
		/// </summary>
		public string Sig
		{
			get { return m_sSig; }
		}

		/// <summary>
		/// Whether the property is an atomic owning or reference property
		/// </summary>
		public bool IsAtomic
		{
			get { return m_fAtomic; }
		}

		/// <summary>
		/// Override Equals to merely check for value equality.
		/// </summary>
		public override bool Equals(object obj)
		{
			ModelProperty that = obj as ModelProperty;
			if (that == null)
				return false;
			return this.m_type == that.m_type &&
				this.m_sName == that.m_sName &&
				this.m_sSig == that.m_sSig &&
				this.m_fAtomic == that.m_fAtomic;
		}

		/// <summary>
		/// Override GetHashCode() because Equals() is overridden.
		/// </summary>
		public override int GetHashCode()
		{
			return m_sName.GetHashCode() + m_fAtomic.GetHashCode() + (int)m_type;
		}

		/// <summary>
		/// Write the !ELEMENT and !ATTLIST elements for a DTD schema.
		/// </summary>
		public void WriteDtdElement(TextWriter tw)
		{
			string sAttr = null;
			switch (m_type)
			{
				case Types.Basic:
					switch (m_sSig)
					{
						case "Binary":
							tw.WriteLine("<!ELEMENT {0} (Binary)?>", this.Name);
							break;
						case "Boolean":
						case "GenDate":
						case "Guid":
						case "Integer":
						case "Time":
							tw.WriteLine("<!ELEMENT {0} EMPTY>", this.Name);
							tw.WriteLine("<!ATTLIST {0}", this.Name);
							tw.WriteLine("  val CDATA #REQUIRED>");
							break;
						case "MultiString":
							tw.WriteLine("<!ELEMENT {0} (AStr)*>", this.Name);
							break;
						case "MultiUnicode":
							tw.WriteLine("<!ELEMENT {0} (AUni)*>", this.Name);
							break;
						case "String":
							tw.WriteLine("<!ELEMENT {0} (Str)?>", this.Name);
							break;
						case "TextPropBinary":
							tw.WriteLine("<!ELEMENT {0} (Prop)?>", this.Name);
							break;
						case "Unicode":
							tw.WriteLine("<!ELEMENT {0} (Uni)?>", this.Name);
							break;
					}
					break;
				case Types.Owning:
				case Types.Reference:
					if (m_fAtomic)
						tw.WriteLine("<!ELEMENT {0} (objsur)?>", this.Name);
					else
						tw.WriteLine("<!ELEMENT {0} (objsur)*>", this.Name);
					break;
			}
			if (!String.IsNullOrEmpty(sAttr))
			{
			}
			tw.WriteLine();
		}

		internal void WritePartialDtdElement(TextWriter tw)
		{
			switch (m_type)
			{
				case Types.Basic:
					switch (m_sSig)
					{
						case "Binary":
							tw.Write("Binary?");
							break;
						case "Boolean":
						case "GenDate":
						case "Guid":
						case "Integer":
						case "Time":
							tw.Write("EMPTY");
							break;
						case "MultiString":
							tw.Write("AStr*");
							break;
						case "MultiUnicode":
							tw.Write("AUni*");
							break;
						case "String":
							tw.Write("Str?");
							break;
						case "TextPropBinary":
							tw.Write("Prop?");
							break;
						case "Unicode":
							tw.Write("Uni?");
							break;
					}
					break;
				case Types.Owning:
				case Types.Reference:
					if (m_fAtomic)
						tw.Write("objsur?");
					else
						tw.Write("objsur*");
					break;
			}
		}

		internal void AddDtdAttrs(List<string> attrs)
		{
			if (m_type == Types.Basic)
			{
				switch (m_sSig)
				{
					case "Boolean":
					case "GenDate":
					case "Guid":
					case "Integer":
					case "Time":
						string sAttr = "  val CDATA #IMPLIED";
						if (!attrs.Contains(sAttr))
							attrs.Add(sAttr);
						break;
					default:
						break;
				}
			}
		}

		/// <summary>
		/// If this property's DTD definition would subsume the other property's DTD definition,
		/// then return true.
		/// </summary>
		public bool DtdSubsumes(ModelProperty that)
		{
			if (this.Equals(that))
				return true;
			switch (this.Type)
			{
				case Types.Basic:
					if (that.Type != Types.Basic)
						return false;
					if (this.Sig == that.Sig)
						return true;
					switch (this.Sig)
					{
						case "Boolean":
						case "GenDate":
						case "Guid":
						case "Integer":
						case "Time":
							return (that.Sig == "Boolean" ||
								that.Sig == "GenDate" ||
								that.Sig == "Guid" ||
								that.Sig == "Integer" ||
								that.Sig == "Time");
						case "Binary":
						case "MultiString":
						case "MultiUnicode":
						case "String":
						case "TextPropBinary":
						case "Unicode":
							break;
					}
					return false;
				case Types.Owning:
				case Types.Reference:
					if (that.Type == Types.Basic)
						return false;
					else if (this.IsAtomic == that.IsAtomic)
						return true;
					else
						return that.IsAtomic;
			}
			return true;
		}
	}

	/// ----------------------------------------------------------------------------------------
	/// <summary>
	/// This contains the relevant details of the entire model.
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	public class FwModel
	{
		string m_sVersion;
		Dictionary<string, ModelClass> m_mapNameClass = new Dictionary<string, ModelClass>();

		/// <summary>
		/// Constructor.
		/// </summary>
		public FwModel(string sModelFile)
		{
			ParseModelFile(sModelFile);
		}

		private void ParseModelFile(string sModelFile)
		{
			XmlReaderSettings settings = new XmlReaderSettings();
			settings.IgnoreComments = true;
			XmlReader xrdr = XmlReader.Create(sModelFile, settings);
			xrdr.MoveToContent();
			if (xrdr.Name == "EntireModel")
			{
				m_sVersion = xrdr.GetAttribute("version");
				xrdr.Read();
				xrdr.MoveToContent();
				while (xrdr.Name == "CellarModule")
				{
					ParseCellarModule(xrdr);
				}
			}
			else
			{
				throw new Exception(String.Format("{0} is not a valid model file.", sModelFile));
			}
		}

		private void ParseCellarModule(XmlReader xrdr)
		{
			if (xrdr.IsEmptyElement)
			{
				xrdr.Read();
				xrdr.MoveToContent();
			}
			if (!xrdr.Read())	// <CellarModule>
				return;
			xrdr.MoveToContent();
			while (xrdr.Name == "class")
			{
				string sName = xrdr.GetAttribute("id");
				string sAbstract = xrdr.GetAttribute("abstract");
				bool fAbstract = false;
				if (!String.IsNullOrEmpty(sAbstract) && sAbstract.ToLowerInvariant() == "true")
					fAbstract = true;
				string sBase = xrdr.GetAttribute("base");
				ModelClass mc = new ModelClass(sName, fAbstract, sBase);
				m_mapNameClass.Add(sName, mc);
				xrdr.Read();
				xrdr.MoveToContent();
				if (xrdr.Name == "comment")
				{
					xrdr.ReadOuterXml();	// swallow the comment.
					xrdr.MoveToContent();
				}
				if (xrdr.Name == "notes")
				{
					xrdr.ReadOuterXml();	// swallow the notes.
					xrdr.MoveToContent();
				}
				if (xrdr.Name == "props")
				{
					ParseClassProps(xrdr, mc.Properties);
				}
				else
				{
					throw new Exception("Expected <comment>, <notes>, or <props> (in that order)!");
				}
				xrdr.ReadEndElement();	// </class>
				xrdr.MoveToContent();
			}
			xrdr.ReadEndElement();	// </CellarModule>
			xrdr.MoveToContent();
		}

		private void ParseClassProps(XmlReader xrdr, List<ModelProperty> props)
		{
			if (xrdr.IsEmptyElement)
			{
				xrdr.Read();
				xrdr.MoveToContent();
				return;
			}
			if (!xrdr.Read())	// <props>
				return;
			xrdr.MoveToContent();
			while (xrdr.IsStartElement())
			{
				ModelProperty.Types type = ModelProperty.Types.Basic;
				switch (xrdr.Name)
				{
					case "basic": type = ModelProperty.Types.Basic; break;
					case "owning": type = ModelProperty.Types.Owning; break;
					case "rel": type = ModelProperty.Types.Reference; break;
					default: throw new Exception("Expected property element!");
				}
				string sName = xrdr.GetAttribute("id");
				string sSig = xrdr.GetAttribute("sig");
				bool fAtomic = xrdr.GetAttribute("card") == "atomic";
				ModelProperty mprop = new ModelProperty(sName, type, sSig, fAtomic);
				props.Add(mprop);
				xrdr.ReadOuterXml();	// swallows any comments or notes on the property.
				xrdr.MoveToContent();
			}
			xrdr.ReadEndElement();	// </props>
			xrdr.MoveToContent();
		}

		/// <summary>
		/// Write an RNG schema for the loaded model.
		/// </summary>
		public void WriteRngSchema(string sRngFile, bool fOld)
		{
			using (TextWriter tw = new StreamWriter(sRngFile, false, Encoding.UTF8))
			{
				tw.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
				tw.WriteLine("<!-- schema created {0} for FieldWorks model version {1} -->",
					DateTime.Now.ToString(), m_sVersion);
				tw.WriteLine("<grammar datatypeLibrary=\"http://www.w3.org/2001/XMLSchema-datatypes\"");
				tw.WriteLine("         xmlns=\"http://relaxng.org/ns/structure/1.0\"");
				tw.WriteLine("         xmlns:sch=\"http://purl.oclc.org/dsdl/schematron\">");
				tw.WriteLine();
				WriteRngBoilerPlate(tw, fOld);

				foreach (ModelClass mc in m_mapNameClass.Values)
				{
					if (!mc.IsAbstract)
						mc.WriteRngDefine(tw, m_mapNameClass, fOld);
				}
				tw.WriteLine("<start>");
				tw.WriteLine("    <element name=\"languageproject\">");
				tw.WriteLine("        <attribute name=\"version\"><value>{0}</value></attribute>", m_sVersion);
				tw.WriteLine("        <optional>");
				tw.WriteLine("            <element name=\"AdditionalFields\">");
				tw.WriteLine("                <zeroOrMore>");
				tw.WriteLine("                    <element name=\"CustomField\">");
				tw.WriteLine("                    <attribute name=\"name\"/>");
				tw.WriteLine("                    <attribute name=\"class\"/>");
				tw.WriteLine("                    <attribute name=\"type\"/>");
				tw.WriteLine("                    <optional><attribute name=\"destclass\"/></optional>");
				tw.WriteLine("                    <optional><attribute name=\"wsSelector\"/></optional>");
				tw.WriteLine("                    <optional><attribute name=\"listRoot\"/></optional>");
				tw.WriteLine("                    <optional><attribute name=\"helpString\"/></optional>");
				tw.WriteLine("                    </element>");
				tw.WriteLine("                </zeroOrMore>");
				tw.WriteLine("            </element>");
				tw.WriteLine("        </optional>");
				tw.WriteLine("        <zeroOrMore>");
				tw.WriteLine("            <element name=\"rt\">");
				tw.WriteLine("                <attribute name=\"guid\"/>");
				tw.WriteLine("                <optional><attribute name=\"ownerguid\"/></optional>");
				if (fOld)
				{
					tw.WriteLine("                <optional><attribute name=\"owningflid\"/></optional>");
					tw.WriteLine("                <optional><attribute name=\"owningord\"/></optional>");
				}
				tw.WriteLine("                <choice>");
				foreach (ModelClass mc in m_mapNameClass.Values)
				{
					if (!mc.IsAbstract)
						tw.WriteLine("                    <ref name=\"{0}\"/>", mc.Name);
				}
				tw.WriteLine("                </choice>");
				tw.WriteLine("            </element>");
				tw.WriteLine("        </zeroOrMore>");
				tw.WriteLine("    </element>");
				tw.WriteLine("</start>");
				tw.WriteLine();
				tw.WriteLine("</grammar>");
			}
		}

		private void WriteRngBoilerPlate(TextWriter tw, bool fOld)
		{
			tw.WriteLine("<define name=\"basic-Time\">");
			tw.WriteLine("    <attribute name=\"val\"/>");
			tw.WriteLine("    <empty/>");
			tw.WriteLine("</define>");
			tw.WriteLine();
			tw.WriteLine("<define name=\"basic-GenDate\">");
			tw.WriteLine("    <attribute name=\"val\"><data type=\"integer\"/></attribute>");
			tw.WriteLine("    <empty/>");
			tw.WriteLine("</define>");
			tw.WriteLine();
			tw.WriteLine("<define name=\"basic-Boolean\">");
			tw.WriteLine("    <attribute name=\"val\">");
			tw.WriteLine("        <choice>");
			tw.WriteLine("            <value>true</value>");
			tw.WriteLine("            <value>false</value>");
			tw.WriteLine("            <value>True</value>");
			tw.WriteLine("            <value>False</value>");
			tw.WriteLine("        </choice>");
			tw.WriteLine("    </attribute>");
			tw.WriteLine("    <empty/>");
			tw.WriteLine("</define>");
			tw.WriteLine();
			tw.WriteLine("<define name=\"basic-Integer\">");
			tw.WriteLine("    <attribute name=\"val\"><data type=\"integer\"/></attribute>");
			tw.WriteLine("    <empty/>");
			tw.WriteLine("</define>");
			tw.WriteLine();
			tw.WriteLine("<define name=\"basic-Guid\">");
			tw.WriteLine("    <attribute name=\"val\">");
			tw.WriteLine("        <data type=\"string\">");
			tw.WriteLine("            <param name=\"pattern\">[0-9A-Fa-f]{8}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{12}</param>");
			tw.WriteLine("        </data>");
			tw.WriteLine("    </attribute>");
			tw.WriteLine("    <empty/>");
			tw.WriteLine("</define>");
			tw.WriteLine();
			tw.WriteLine("<define name=\"String-Runs\">");
			tw.WriteLine("    <zeroOrMore>");
			tw.WriteLine("        <element name=\"Run\">");
			tw.WriteLine("            <attribute name=\"ws\"/>");
			tw.WriteLine("            <ref name=\"common-property-attributes\"/>");
			tw.WriteLine("            <ref name=\"Run-Prop-Attributes\"/>");
			tw.WriteLine("        	  <optional>");
			tw.WriteLine("                <attribute name=\"contextString\">");
			tw.WriteLine("                    <data type=\"string\">");
			tw.WriteLine("                        <param name=\"pattern\">[0-9A-Fa-f]{8}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{12}</param>");
			tw.WriteLine("                    </data>");
			tw.WriteLine("                </attribute>");
			tw.WriteLine("            </optional>");
			tw.WriteLine("        	  <optional><attribute name=\"externalLink\"/></optional>");
			tw.WriteLine("        	  <optional><attribute name=\"fontVariations\"/></optional>");
			tw.WriteLine("        	  <optional>");
			tw.WriteLine("                <attribute name=\"link\">");
			tw.WriteLine("                    <data type=\"string\">");
			tw.WriteLine("                        <param name=\"pattern\">[0-9A-Fa-f]{8}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{12}</param>");
			tw.WriteLine("                    </data>");
			tw.WriteLine("                </attribute>");
			tw.WriteLine("            </optional>");
			tw.WriteLine("        	  <optional>");
			tw.WriteLine("                <attribute name=\"moveableObj\">");
			tw.WriteLine("                    <data type=\"string\">");
			tw.WriteLine("                        <param name=\"pattern\">[0-9A-Fa-f]{8}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{12}</param>");
			tw.WriteLine("                    </data>");
			tw.WriteLine("                </attribute>");
			tw.WriteLine("            </optional>");
			tw.WriteLine("        	  <optional>");
			tw.WriteLine("                <attribute name=\"ownlink\">");
			tw.WriteLine("                    <data type=\"string\">");
			tw.WriteLine("                        <param name=\"pattern\">[0-9A-Fa-f]{8}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{12}</param>");
			tw.WriteLine("                    </data>");
			tw.WriteLine("                </attribute>");
			tw.WriteLine("            </optional>");
			tw.WriteLine("        	  <optional>");
			tw.WriteLine("                <attribute name=\"paraStyle\"/></optional>");
			tw.WriteLine("        	  <optional><attribute name=\"tabList\"/></optional>");
			tw.WriteLine("        	  <optional>");
			tw.WriteLine("                <attribute name=\"tags\">");
			tw.WriteLine("                    <data type=\"string\">");
			tw.WriteLine("                        <param name=\"pattern\">[0-9A-Fa-f]{8}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{12}(\\s+[0-9A-Fa-f]{8}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{12})*</param>");
			tw.WriteLine("                    </data>");
			tw.WriteLine("                </attribute>");
			tw.WriteLine("            </optional>");
			tw.WriteLine("            <optional>");
			tw.WriteLine("                <attribute name=\"type\">");
			tw.WriteLine("                    <choice>");
			tw.WriteLine("                        <value>chars</value>");
			tw.WriteLine("                        <value>picture</value>");
			tw.WriteLine("                    </choice>");
			tw.WriteLine("                </attribute>");
			tw.WriteLine("    </optional>");
			tw.WriteLine("	  <optional><attribute name=\"wsStyle\"/></optional>");
			tw.WriteLine("            <text/>");
			tw.WriteLine("        </element>");
			tw.WriteLine("    </zeroOrMore>");
			tw.WriteLine("</define>");
			tw.WriteLine();
			tw.WriteLine("<define name=\"common-property-attributes\">");
			tw.WriteLine("	  <optional><attribute name=\"backcolor\"/></optional>");
			tw.WriteLine("    <optional>");
			tw.WriteLine("        <attribute name=\"bold\">");
			tw.WriteLine("            <choice>");
			tw.WriteLine("                <value>invert</value>");
			tw.WriteLine("                <value>off</value>");
			tw.WriteLine("                <value>on</value>");
			tw.WriteLine("            </choice>");
			tw.WriteLine("        </attribute>");
			tw.WriteLine("    </optional>");
			tw.WriteLine("	  <optional><attribute name=\"forecolor\"/></optional>");
			tw.WriteLine("    <optional>");
			tw.WriteLine("        <attribute name=\"italic\">");
			tw.WriteLine("            <choice>");
			tw.WriteLine("                <value>invert</value>");
			tw.WriteLine("                <value>off</value>");
			tw.WriteLine("                <value>on</value>");
			tw.WriteLine("            </choice>");
			tw.WriteLine("        </attribute>");
			tw.WriteLine("    </optional>");
			tw.WriteLine("    <optional>");
			tw.WriteLine("        <attribute name=\"superscript\">");
			tw.WriteLine("            <choice>");
			tw.WriteLine("                <value>off</value>");
			tw.WriteLine("                <value>sub</value>");
			tw.WriteLine("                <value>super</value>");
			tw.WriteLine("            </choice>");
			tw.WriteLine("        </attribute>");
			tw.WriteLine("    </optional>");
			tw.WriteLine("	  <optional><attribute name=\"undercolor\"/></optional>");
			tw.WriteLine("    <optional>");
			tw.WriteLine("        <attribute name=\"underline\">");
			tw.WriteLine("            <choice>");
			tw.WriteLine("                <value>dashed</value>");
			tw.WriteLine("                <value>dotted</value>");
			tw.WriteLine("                <value>double</value>");
			tw.WriteLine("                <value>none</value>");
			tw.WriteLine("                <value>single</value>");
			tw.WriteLine("                <value>squiggle</value>");
			tw.WriteLine("                <value>strikethrough</value>");
			tw.WriteLine("            </choice>");
			tw.WriteLine("        </attribute>");
			tw.WriteLine("    </optional>");
			tw.WriteLine("</define>");
			tw.WriteLine();
			tw.WriteLine("<define name=\"Run-Prop-Attributes\">");
			tw.WriteLine("    <optional><attribute name=\"bulNumTxtAft\"/></optional>");
			tw.WriteLine("    <optional><attribute name=\"bulNumTxtBef\"/></optional>");
			tw.WriteLine("    <optional><attribute name=\"charStyle\"/></optional>");
			tw.WriteLine("	  <optional><attribute name=\"fontFamily\"/></optional>");
			tw.WriteLine("    <optional><attribute name=\"fontsize\"><data type=\"integer\"/></attribute></optional>");
			tw.WriteLine("    <optional>");
			tw.WriteLine("        <attribute name=\"fontsizeUnit\">");
			tw.WriteLine("            <choice>");
			tw.WriteLine("                <value>mpt</value>");
			tw.WriteLine("                <value>rel</value>");
			tw.WriteLine("            </choice>");
			tw.WriteLine("        </attribute>");
			tw.WriteLine("    </optional>");
			tw.WriteLine("	  <optional><attribute name=\"namedStyle\"/></optional>");
			tw.WriteLine("	  <optional><attribute name=\"offset\"><data type=\"integer\"/></attribute></optional>");
			tw.WriteLine("	  <optional>");
			tw.WriteLine("        <attribute name=\"offsetUnit\">");
			tw.WriteLine("            <choice>");
			tw.WriteLine("                <value>mpt</value>");
			tw.WriteLine("                <value>rel</value>");
			tw.WriteLine("            </choice>");
			tw.WriteLine("        </attribute>");
			tw.WriteLine("    </optional>");
			tw.WriteLine("    <optional>");
			tw.WriteLine("        <attribute name=\"spellcheck\">");
			tw.WriteLine("            <choice>");
			tw.WriteLine("                <value>normal</value>");
			tw.WriteLine("                <value>doNotCheck</value>");
			tw.WriteLine("                <value>forceCheck</value>");
			tw.WriteLine("            </choice>");
			tw.WriteLine("        </attribute>");
			tw.WriteLine("    </optional>");
			tw.WriteLine("	  <optional><attribute name=\"wsBase\"/></optional>");
			tw.WriteLine("</define>");
			tw.WriteLine();
			tw.WriteLine("<define name=\"basic-String\">");
			tw.WriteLine("    <optional>");
			tw.WriteLine("        <element name=\"Str\"><ref name=\"String-Runs\"/></element>");
			tw.WriteLine("    </optional>");
			tw.WriteLine("</define>");
			tw.WriteLine();
			tw.WriteLine("<define name=\"basic-MultiString\">");
			tw.WriteLine("    <zeroOrMore>");
			tw.WriteLine("        <element name=\"AStr\">");
			tw.WriteLine("            <attribute name=\"ws\"/>");
			tw.WriteLine("            <ref name=\"String-Runs\"/>");
			tw.WriteLine("        </element>");
			tw.WriteLine("    </zeroOrMore>");
			tw.WriteLine("</define>");
			tw.WriteLine();
			tw.WriteLine("<define name=\"basic-Unicode\">");
			tw.WriteLine("    <optional>");
			tw.WriteLine("        <element name=\"Uni\"><text/></element>");
			tw.WriteLine("    </optional>");
			tw.WriteLine("</define>");
			tw.WriteLine();
			tw.WriteLine("<define name=\"basic-MultiUnicode\">");
			tw.WriteLine("    <zeroOrMore>");
			tw.WriteLine("        <element name=\"AUni\">");
			tw.WriteLine("            <attribute name=\"ws\"/>");
			tw.WriteLine("            <text/>");
			tw.WriteLine("        </element>");
			tw.WriteLine("    </zeroOrMore>");
			tw.WriteLine("</define>");
			tw.WriteLine();
			tw.WriteLine("<define name=\"eight-bit-unsigned\">");
			tw.WriteLine("    <data type=\"integer\">");
			tw.WriteLine("        <param name=\"pattern\">([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])</param>");
			tw.WriteLine("    </data>");
			tw.WriteLine("</define>");
			tw.WriteLine("<define name=\"number-millipoint\">");
			tw.WriteLine("    <data type=\"string\">");
			tw.WriteLine("        <param name=\"pattern\">[0-9]+mpt</param>");
			tw.WriteLine("    </data>");
			tw.WriteLine("</define>");
			tw.WriteLine("<define name=\"basic-TextPropBinary\">");
			tw.WriteLine("    <optional>");
			tw.WriteLine("        <element name=\"Prop\">");
			tw.WriteLine("            <ref name=\"common-property-attributes\"/>");
			tw.WriteLine("            <ref name=\"Run-Prop-Attributes\"/>");
			tw.WriteLine("            <optional>");
			tw.WriteLine("                <attribute name=\"align\">");
			tw.WriteLine("                    <choice>");
			tw.WriteLine("                        <value>leading</value>");
			tw.WriteLine("                        <value>left</value>");
			tw.WriteLine("                        <value>center</value>");
			tw.WriteLine("                        <value>right</value>");
			tw.WriteLine("                        <value>trailing</value>");
			tw.WriteLine("                        <value>justify</value>");
			tw.WriteLine("                    </choice>");
			tw.WriteLine("                </attribute>");
			tw.WriteLine("            </optional>");
			tw.WriteLine("            <optional><attribute name=\"borderBottom\"><data type=\"integer\"/></attribute></optional>");
			tw.WriteLine("            <optional><attribute name=\"borderColor\"/></optional>");
			tw.WriteLine("            <optional><attribute name=\"borderLeading\"><data type=\"integer\"/></attribute></optional>");
			tw.WriteLine("            <optional><attribute name=\"borderTop\"><data type=\"integer\"/></attribute></optional>");
			tw.WriteLine("            <optional><attribute name=\"borderTrailing\"><data type=\"integer\"/></attribute></optional>");
			tw.WriteLine("            <optional><attribute name=\"bulNumScheme\"><ref name=\"eight-bit-unsigned\"/></attribute></optional>");
			tw.WriteLine("            <optional><attribute name=\"bulNumStartAt\"><data type=\"integer\"/></attribute></optional>");
			tw.WriteLine("            <optional><attribute name=\"firstIndent\"><data type=\"integer\"/></attribute></optional>");
			tw.WriteLine("            <optional><attribute name=\"keepTogether\"><ref name=\"eight-bit-unsigned\"/></attribute></optional>");
			tw.WriteLine("            <optional><attribute name=\"keepWithNext\"><ref name=\"eight-bit-unsigned\"/></attribute></optional>");
			tw.WriteLine("            <optional><attribute name=\"leadingIndent\"><data type=\"integer\"/></attribute></optional>");
			tw.WriteLine("            <optional><attribute name=\"lineHeight\"><data type=\"integer\"/></attribute></optional>");
			tw.WriteLine("            <optional>");
			tw.WriteLine("                <attribute name=\"lineHeightType\">");
			tw.WriteLine("                    <choice>");
			tw.WriteLine("                        <value>exact</value>");
			tw.WriteLine("                        <value>atLeast</value>");
			tw.WriteLine("                    </choice>");
			tw.WriteLine("                </attribute>");
			tw.WriteLine("            </optional>");
			tw.WriteLine("            <optional>");
			tw.WriteLine("                <attribute name=\"lineHeightUnit\">");
			tw.WriteLine("                    <choice>");
			tw.WriteLine("                        <value>mpt</value>");
			tw.WriteLine("                        <value>rel</value>");
			tw.WriteLine("                    </choice>");
			tw.WriteLine("                </attribute>");
			tw.WriteLine("            </optional>");
			tw.WriteLine("            <optional><attribute name=\"MarginTop\"><data type=\"integer\"/></attribute></optional>");
			tw.WriteLine("            <optional><attribute name=\"padBottom\"><data type=\"integer\"/></attribute></optional>");
			tw.WriteLine("            <optional><attribute name=\"padLeading\"><data type=\"integer\"/></attribute></optional>");
			tw.WriteLine("            <optional><attribute name=\"padTop\"><data type=\"integer\"/></attribute></optional>");
			tw.WriteLine("            <optional><attribute name=\"padTrailing\"><data type=\"integer\"/></attribute></optional>");
			tw.WriteLine("            <optional><attribute name=\"paracolor\"/></optional>");
			tw.WriteLine("            <optional><attribute name=\"rightToLeft\"><ref name=\"eight-bit-unsigned\"/></attribute></optional>");
			tw.WriteLine("            <optional><attribute name=\"spaceAfter\"><data type=\"integer\"/></attribute></optional>");
			tw.WriteLine("            <optional><attribute name=\"spaceBefore\"/></optional>");
			tw.WriteLine("            <optional><attribute name=\"tabDef\"><data type=\"integer\"/></attribute></optional>");
			tw.WriteLine("            <optional><attribute name=\"trailingIndent\"><data type=\"integer\"/></attribute></optional>");
			tw.WriteLine("            <optional><attribute name=\"widowOrphan\"><ref name=\"eight-bit-unsigned\"/></attribute></optional>");
			tw.WriteLine("            <optional><attribute name=\"ws\"/></optional>");
			tw.WriteLine("            <optional>");
			tw.WriteLine("                <element name=\"BulNumFontInfo\">");
			tw.WriteLine("                    <ref name=\"common-property-attributes\"/>");
			tw.WriteLine("	                  <optional><attribute name=\"fontFamily\"/></optional>");
			tw.WriteLine("                    <optional><attribute name=\"fontsize\"><ref name=\"number-millipoint\"/></attribute></optional>");
			tw.WriteLine("                    <optional><attribute name=\"offset\"><ref name=\"number-millipoint\"/></attribute></optional>");
			tw.WriteLine("                    <empty/>");
			tw.WriteLine("                </element>");
			tw.WriteLine("            </optional>");
			tw.WriteLine("            <optional>");
			tw.WriteLine("                <element name=\"WsStyles9999\">");
			tw.WriteLine("                    <zeroOrMore>");
			tw.WriteLine("                        <element name=\"WsProp\">");
			tw.WriteLine("                            <attribute name=\"ws\"/>");
			tw.WriteLine("                            <ref name=\"common-property-attributes\"/>");
			tw.WriteLine("                        	  <optional><attribute name=\"fontFamily\"/></optional>");
			tw.WriteLine("                            <optional><attribute name=\"fontVariations\"/></optional>");
			tw.WriteLine("                            <optional>");
			tw.WriteLine("                                <attribute name=\"fontsize\"><data type=\"integer\"/></attribute>");
			tw.WriteLine("                                <attribute name=\"fontsizeUnit\"><value>mpt</value></attribute>");
			tw.WriteLine("                            </optional>");
			tw.WriteLine("                            <optional>");
			tw.WriteLine("                                <attribute name=\"offset\"><data type=\"integer\"/></attribute>");
			tw.WriteLine("                                <attribute name=\"offsetUnit\"><value>mpt</value></attribute>");
			tw.WriteLine("                            </optional>");
			tw.WriteLine("                            <empty/>");
			tw.WriteLine("                        </element>");
			tw.WriteLine("                    </zeroOrMore>");
			tw.WriteLine("                </element>");
			tw.WriteLine("            </optional>");
			tw.WriteLine("        </element>");
			tw.WriteLine("    </optional>");
			tw.WriteLine("</define>");
			tw.WriteLine();
			tw.WriteLine("<define name=\"basic-Binary\">");
			tw.WriteLine("    <element name=\"Binary\">");
			tw.WriteLine("        <data type=\"string\">");
			tw.WriteLine("            <param name=\"pattern\">[\\s0-9A-Fa-f]*</param>");
			tw.WriteLine("        </data>");
			tw.WriteLine("    </element>");
			tw.WriteLine("</define>");
			tw.WriteLine();
			tw.WriteLine("<define name=\"owning-atomic\">");
			tw.WriteLine("    <optional>");
			tw.WriteLine("        <element name=\"objsur\">");
			tw.WriteLine("            <attribute name=\"guid\"/>");
			if (fOld)
				tw.WriteLine("            <optional><attribute name=\"t\"><value>o</value></attribute></optional>");
			else
				tw.WriteLine("            <attribute name=\"t\"><value>o</value></attribute>");
			tw.WriteLine("            <empty/>");
			tw.WriteLine("        </element>");
			tw.WriteLine("    </optional>");
			tw.WriteLine("</define>");
			tw.WriteLine();
			tw.WriteLine("<define name=\"owning-multiple\">");
			tw.WriteLine("    <zeroOrMore>");
			tw.WriteLine("        <element name=\"objsur\">");
			tw.WriteLine("            <attribute name=\"guid\"/>");
			if (fOld)
				tw.WriteLine("            <optional><attribute name=\"t\"><value>o</value></attribute></optional>");
			else
				tw.WriteLine("            <attribute name=\"t\"><value>o</value></attribute>");
			tw.WriteLine("            <empty/>");
			tw.WriteLine("        </element>");
			tw.WriteLine("    </zeroOrMore>");
			tw.WriteLine("</define>");
			tw.WriteLine();
			tw.WriteLine("<define name=\"reference-atomic\">");
			tw.WriteLine("    <optional>");
			tw.WriteLine("        <element name=\"objsur\">");
			tw.WriteLine("            <attribute name=\"guid\"/>");
			if (fOld)
				tw.WriteLine("            <optional><attribute name=\"t\"><value>r</value></attribute></optional>");
			else
				tw.WriteLine("            <attribute name=\"t\"><value>r</value></attribute>");
			tw.WriteLine("            <empty/>");
			tw.WriteLine("        </element>");
			tw.WriteLine("    </optional>");
			tw.WriteLine("</define>");
			tw.WriteLine();
			tw.WriteLine("<define name=\"reference-multiple\">");
			tw.WriteLine("    <zeroOrMore>");
			tw.WriteLine("        <element name=\"objsur\">");
			tw.WriteLine("            <attribute name=\"guid\"/>");
			if (fOld)
				tw.WriteLine("            <optional><attribute name=\"t\"><value>r</value></attribute></optional>");
			else
				tw.WriteLine("            <attribute name=\"t\"><value>r</value></attribute>");
			tw.WriteLine("            <empty/>");
			tw.WriteLine("        </element>");
			tw.WriteLine("    </zeroOrMore>");
			tw.WriteLine("</define>");
			tw.WriteLine();
			tw.WriteLine("<define name=\"CustomFields\">");
			tw.WriteLine("    <zeroOrMore>");
			tw.WriteLine("        <element name=\"Custom\">");
			tw.WriteLine("            <attribute name=\"name\"/>");
			tw.WriteLine("            <choice>");
			tw.WriteLine("                <ref name=\"basic-Boolean\"/>");
			tw.WriteLine("                <ref name=\"basic-Integer\"/>");
			tw.WriteLine("                <ref name=\"basic-Guid\"/>");
			tw.WriteLine("                <ref name=\"basic-String\"/>");
			tw.WriteLine("                <ref name=\"basic-MultiString\"/>");
			tw.WriteLine("                <ref name=\"basic-Unicode\"/>");
			tw.WriteLine("                <ref name=\"basic-MultiUnicode\"/>");
			tw.WriteLine("                <ref name=\"basic-Binary\"/>");
			tw.WriteLine("                <ref name=\"basic-Time\"/>");
			tw.WriteLine("                <ref name=\"basic-GenDate\"/>");
			tw.WriteLine("                <ref name=\"reference-atomic\"/>");
			tw.WriteLine("                <ref name=\"reference-multiple\"/>");
			tw.WriteLine("            </choice>");
			tw.WriteLine("        </element>");
			tw.WriteLine("    </zeroOrMore>");
			tw.WriteLine("</define>");
			tw.WriteLine();
		}

		public void WriteDtd(string sDtdFile)
		{
			Dictionary<string, List<ModelProperty>> mapNameProps = CreatePropertyList();
			using (TextWriter tw = new StreamWriter(sDtdFile, false, Encoding.UTF8))
			{
				tw.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
				tw.WriteLine("<!-- DTD created {0} for FieldWorks model version {1} -->",
					DateTime.Now.ToString(), m_sVersion);
				tw.WriteLine();
				tw.WriteLine("<!ELEMENT languageproject (AdditionalFields?, (rt)+)>");
				tw.WriteLine("<!ATTLIST languageproject");
				tw.WriteLine("  version CDATA #FIXED '{0}'>", m_sVersion);
				tw.WriteLine();
				tw.WriteLine("<!ELEMENT AdditionalFields (CustomField)*>");
				tw.WriteLine();
				tw.WriteLine("<!ELEMENT CustomField EMPTY>");
				tw.WriteLine("<!ATTLIST CustomField");
				tw.WriteLine("  name CDATA #REQUIRED");
				tw.WriteLine("  class CDATA #REQUIRED");
				tw.WriteLine("  type CDATA #REQUIRED");
				tw.WriteLine("  destclass CDATA #IMPLIED");
				tw.WriteLine("  wsSelector CDATA #IMPLIED");
				tw.WriteLine("  listRoot CDATA #IMPLIED");
				tw.WriteLine("  helpString CDATA #IMPLIED>");
				tw.WriteLine();
				tw.Write("<!ELEMENT rt (");
				bool fFirst = true;
				foreach (string prop in mapNameProps.Keys)
				{
					if (!fFirst)
						tw.Write("    |");
					tw.WriteLine(prop);
					fFirst = false;
				}
				if (!fFirst)
					tw.Write("    |");
				tw.WriteLine("Custom)*>");
				tw.WriteLine("<!ATTLIST rt");
				tw.WriteLine("  class NMTOKEN #REQUIRED");
				tw.WriteLine("  guid CDATA #REQUIRED");
				tw.WriteLine("  ownerguid CDATA #IMPLIED>");
				tw.WriteLine();

				WritePropertyDtdElements(mapNameProps, tw);
				WriteDtdBoilerplate(tw, false);
			}
		}

		private void WritePropertyDtdElements(Dictionary<string, List<ModelProperty>> mapNameProps, TextWriter tw)
		{
			foreach (List<ModelProperty> props in mapNameProps.Values)
			{
				if (props.Count == 1)
				{
					props[0].WriteDtdElement(tw);
				}
				else if (props.Count > 1)
				{
					tw.Write("<!ELEMENT {0} (", props[0].Name);
					bool fFirst = true;
					foreach (ModelProperty mprop in props)
					{
						if (!fFirst)
							tw.Write(" | ");
						mprop.WritePartialDtdElement(tw);
						fFirst = false;
					}
					tw.WriteLine(")>");
					List<string> attrs = new List<string>();
					foreach (ModelProperty mprop in props)
						mprop.AddDtdAttrs(attrs);
					if (attrs.Count > 0)
					{
						tw.WriteLine("<!ATTLIST {0}", props[0].Name);
						foreach (string attr in attrs)
							tw.WriteLine(attr);
						tw.WriteLine(">");
					}
					tw.WriteLine();
				}
			}
		}

		private Dictionary<string, List<ModelProperty>> CreatePropertyList()
		{
			List<ModelProperty> propsDel = new List<ModelProperty>();
			Dictionary<string, List<ModelProperty>> mapNameProps = new Dictionary<string, List<ModelProperty>>();
			foreach (ModelClass mc in m_mapNameClass.Values)
			{
				foreach (ModelProperty mprop in mc.Properties)
				{
					List<ModelProperty> props;
					if (mapNameProps.TryGetValue(mprop.Name, out props))
					{
						if (props.Contains(mprop))
							continue;
						bool fAdd = true;
						propsDel.Clear();
						foreach (ModelProperty mp in props)
						{
							if (mp.DtdSubsumes(mprop))
							{
								fAdd = false;
								break;
							}
							if (mprop.DtdSubsumes(mp))
								propsDel.Add(mp);
						}
						if (fAdd)
						{
							props.Add(mprop);
							foreach (ModelProperty mp in propsDel)
								props.Remove(mp);
						}
					}
					else
					{
						props = new List<ModelProperty>();
						props.Add(mprop);
						mapNameProps.Add(mprop.Name, props);
					}
				}
			}
			return mapNameProps;
		}

		private void WriteDtdBoilerplate(TextWriter tw, bool fOld)
		{
			tw.WriteLine("<!ELEMENT Uni (#PCDATA)>");
			tw.WriteLine();
			tw.WriteLine("<!ELEMENT AUni (#PCDATA)>");
			tw.WriteLine("<!ATTLIST AUni");
			tw.WriteLine("	ws CDATA #REQUIRED>");
			tw.WriteLine();
			tw.WriteLine("<!ELEMENT Str (Run)*>");
			tw.WriteLine();
			tw.WriteLine("<!ELEMENT AStr (Run)*>");
			tw.WriteLine("<!ATTLIST AStr");
			tw.WriteLine("	ws CDATA #REQUIRED>");
			tw.WriteLine();
			tw.WriteLine("<!ELEMENT objsur EMPTY>");
			tw.WriteLine("<!ATTLIST objsur");
			tw.WriteLine("  guid CDATA #REQUIRED");
			if (fOld)
				tw.WriteLine("  t (o | r) #IMPLIED>");
			else
				tw.WriteLine("  t (o | r) #REQUIRED>");
			tw.WriteLine();
			tw.WriteLine("<!ELEMENT Binary (#PCDATA)>");
			tw.WriteLine();
			tw.WriteLine("<!ELEMENT Run (#PCDATA)>");
			tw.WriteLine("<!ATTLIST Run");
			tw.WriteLine("    ws CDATA #REQUIRED");
			tw.WriteLine("    backcolor CDATA #IMPLIED");
			tw.WriteLine("    bold (off | on | invert) #IMPLIED");
			tw.WriteLine("    bulNumTxtAft CDATA #IMPLIED");
			tw.WriteLine("    bulNumTxtBef CDATA #IMPLIED");
			tw.WriteLine("    charStyle CDATA #IMPLIED");
			tw.WriteLine("    contextString CDATA #IMPLIED");	// single GUID
			tw.WriteLine("    externalLink CDATA #IMPLIED");
			tw.WriteLine("    fontFamily CDATA #IMPLIED");
			tw.WriteLine("    fontVariations CDATA #IMPLIED");
			tw.WriteLine("    fontsize CDATA #IMPLIED");
			tw.WriteLine("    fontsizeUnit (mpt | rel) #IMPLIED");
			tw.WriteLine("    forecolor CDATA #IMPLIED");
			tw.WriteLine("    italic (off | on | invert) #IMPLIED");
			tw.WriteLine("    link CDATA #IMPLIED");			// single GUID
			tw.WriteLine("    moveableObj CDATA #IMPLIED");		// single GUID
			tw.WriteLine("    namedStyle CDATA #IMPLIED");
			tw.WriteLine("    offset CDATA #IMPLIED");
			tw.WriteLine("    offsetUnit (mpt | rel) #IMPLIED");
			tw.WriteLine("    ownlink CDATA #IMPLIED");			// single GUID
			tw.WriteLine("    paraStyle CDATA #IMPLIED");
			tw.WriteLine("    spellcheck (normal | doNotCheck | forceCheck) #IMPLIED");
			tw.WriteLine("    superscript (off | super | sub) #IMPLIED");
			tw.WriteLine("    tabList CDATA #IMPLIED");
			tw.WriteLine("    tags CDATA #IMPLIED");			// space delimited list of GUIDs
			tw.WriteLine("    type (chars | picture) #IMPLIED");
			tw.WriteLine("    undercolor CDATA #IMPLIED");
			tw.WriteLine("    underline (none | dotted | dashed | strikethrough | single | double | squiggle) #IMPLIED");
			tw.WriteLine("    wsBase CDATA #IMPLIED");
			tw.WriteLine("    wsStyle CDATA #IMPLIED");
			tw.WriteLine(">");
			tw.WriteLine();
			tw.WriteLine("<!ELEMENT Prop (BulNumFontInfo | WsStyles9999)*>");
			tw.WriteLine("<!ATTLIST Prop");
			tw.WriteLine("    align (leading | left | center | right | trailing | justify) #IMPLIED");
			tw.WriteLine("    backcolor CDATA #IMPLIED");
			tw.WriteLine("    bold (off | on | invert) #IMPLIED");
			tw.WriteLine("    borderBottom CDATA #IMPLIED");	// number
			tw.WriteLine("    borderColor CDATA #IMPLIED");
			tw.WriteLine("    borderLeading CDATA #IMPLIED");	// number
			tw.WriteLine("    borderTop CDATA #IMPLIED");		// number
			tw.WriteLine("    borderTrailing CDATA #IMPLIED");	// number
			tw.WriteLine("    bulNumScheme CDATA #IMPLIED");	// number in range 0-255
			tw.WriteLine("    bulNumStartAt CDATA #IMPLIED");	// number
			tw.WriteLine("    bulNumTxtAft CDATA #IMPLIED");
			tw.WriteLine("    bulNumTxtBef CDATA #IMPLIED");
			tw.WriteLine("    charStyle CDATA #IMPLIED");
			tw.WriteLine("    firstIndent CDATA #IMPLIED");		// number
			tw.WriteLine("    fontFamily CDATA #IMPLIED");
			tw.WriteLine("    fontsize CDATA #IMPLIED");
			tw.WriteLine("    fontsizeUnit (mpt | rel) #IMPLIED");
			tw.WriteLine("    forecolor CDATA #IMPLIED");
			tw.WriteLine("    italic (off | on | invert) #IMPLIED");
			tw.WriteLine("    keepTogether CDATA #IMPLIED");	// number in range 0-255
			tw.WriteLine("    keepWithNext CDATA #IMPLIED");	// number in range 0-255
			tw.WriteLine("    leadingIndent CDATA #IMPLIED");	// number
			tw.WriteLine("    lineHeight CDATA #IMPLIED");
			tw.WriteLine("    lineHeightType (exact | atLeast) #IMPLIED");
			tw.WriteLine("    lineHeightUnit (mpt | rel) #IMPLIED");
			tw.WriteLine("    MarginTop CDATA #IMPLIED");		// number
			tw.WriteLine("    namedStyle CDATA #IMPLIED");
			tw.WriteLine("    offset CDATA #IMPLIED");
			tw.WriteLine("    offsetUnit (mpt | rel) #IMPLIED");
			tw.WriteLine("    padBottom CDATA #IMPLIED");		// number
			tw.WriteLine("    padLeading CDATA #IMPLIED");		// number
			tw.WriteLine("    padTop CDATA #IMPLIED");			// number
			tw.WriteLine("    padTrailing CDATA #IMPLIED");		// number
			tw.WriteLine("    paracolor CDATA #IMPLIED");
			tw.WriteLine("    rightToLeft CDATA #IMPLIED");		// number in range 0-255
			tw.WriteLine("    spaceAfter CDATA #IMPLIED");		// number
			tw.WriteLine("    spaceBefore CDATA #IMPLIED");		// number
			tw.WriteLine("    spellcheck (normal | doNotCheck | forceCheck) #IMPLIED");
			tw.WriteLine("    superscript (off | super | sub) #IMPLIED");
			tw.WriteLine("    tabDef CDATA #IMPLIED");			// number
			tw.WriteLine("    trailingIndent CDATA #IMPLIED");	// number
			tw.WriteLine("    undercolor CDATA #IMPLIED");
			tw.WriteLine("    underline (none | dotted | dashed | strikethrough | single | double | squiggle) #IMPLIED");
			tw.WriteLine("    widowOrphan CDATA #IMPLIED");		// number in range 0-255
			tw.WriteLine("    ws CDATA #IMPLIED");
			tw.WriteLine("    wsBase CDATA #IMPLIED");
			tw.WriteLine(">");
			tw.WriteLine();
			tw.WriteLine("<!ELEMENT BulNumFontInfo EMPTY>");
			tw.WriteLine("<!ATTLIST BulNumFontInfo");
			tw.WriteLine("    backcolor CDATA #IMPLIED");
			tw.WriteLine("    bold (off | on | invert) #IMPLIED");
			tw.WriteLine("    fontFamily CDATA #IMPLIED");
			tw.WriteLine("    fontsize CDATA #IMPLIED");		// has the format "%dmpt"
			tw.WriteLine("    forecolor CDATA #IMPLIED");
			tw.WriteLine("    italic (off | on | invert) #IMPLIED");
			tw.WriteLine("    offset CDATA #IMPLIED");			// has the format "%dmpt"
			tw.WriteLine("    superscript (off | super | sub) #IMPLIED");
			tw.WriteLine("    undercolor CDATA #IMPLIED");
			tw.WriteLine("    underline (none | dotted | dashed | strikethrough | single | double | squiggle) #IMPLIED");
			tw.WriteLine(">");
			tw.WriteLine();
			tw.WriteLine("<!ELEMENT WsStyles9999 (WsProp)*>");
			tw.WriteLine();
			tw.WriteLine("<!ELEMENT WsProp EMPTY>");
			tw.WriteLine("<!ATTLIST WsProp");
			tw.WriteLine("    ws CDATA #REQUIRED");
			tw.WriteLine("    backcolor CDATA #IMPLIED");
			tw.WriteLine("    bold (off | on | invert) #IMPLIED");
			tw.WriteLine("    fontFamily CDATA #IMPLIED");
			tw.WriteLine("    fontVariations CDATA #IMPLIED");
			tw.WriteLine("    fontsize CDATA #IMPLIED");			// number
			tw.WriteLine("    fontsizeUnit (mpt | rel) #IMPLIED");	// actually always mpt
			tw.WriteLine("    forecolor CDATA #IMPLIED");
			tw.WriteLine("    italic (off | on | invert) #IMPLIED");
			tw.WriteLine("    offset CDATA #IMPLIED");				// number
			tw.WriteLine("    offsetUnit (mpt | rel) #IMPLIED");	// actually always mpt
			tw.WriteLine("    superscript (off | super | sub) #IMPLIED");
			tw.WriteLine("    undercolor CDATA #IMPLIED");
			tw.WriteLine("    underline (none | dotted | dashed | strikethrough | single | double | squiggle) #IMPLIED");
			tw.WriteLine(">");
			tw.WriteLine();
			tw.WriteLine("<!ELEMENT Custom ((objsur)* | (Uni)* | (AUni)* | (Str)* | (AStr)* | (Binary)* | EMPTY)>");
			tw.WriteLine("<!ATTLIST Custom");
			tw.WriteLine("  name CDATA #REQUIRED");
			tw.WriteLine("  val CDATA #IMPLIED>");
		}

		private void WriteOldDtd(string sDtdFile)
		{
			Dictionary<string, List<ModelProperty>> mapNameProps = CreatePropertyList();
			using (TextWriter tw = new StreamWriter(sDtdFile, false, Encoding.UTF8))
			{
				tw.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
				tw.WriteLine("<!-- DTD created {0} for FieldWorks model version {1} -->",
					DateTime.Now.ToString(), m_sVersion);
				tw.WriteLine();
				tw.WriteLine("<!ELEMENT languageproject (AdditionalFields?, (rt)+)>");
				tw.WriteLine("<!ATTLIST languageproject");
				tw.WriteLine("  version CDATA #FIXED '{0}'>", m_sVersion);
				tw.WriteLine();
				tw.WriteLine("<!ELEMENT AdditionalFields (CustomField)*>");
				tw.WriteLine();
				tw.WriteLine("<!ELEMENT CustomField EMPTY>");
				tw.WriteLine("<!ATTLIST CustomField");
				tw.WriteLine("  name CDATA #REQUIRED");
				tw.WriteLine("  class CDATA #REQUIRED");
				tw.WriteLine("  type CDATA #REQUIRED");
				tw.WriteLine("  destclass CDATA #IMPLIED");
				tw.WriteLine("  wsSelector CDATA #IMPLIED");
				tw.WriteLine("  listRoot CDATA #IMPLIED");
				tw.WriteLine("  helpString CDATA #IMPLIED>");
				tw.WriteLine();
				tw.Write("<!ELEMENT rt (");
				bool fFirst = true;
				foreach (string sClass in m_mapNameClass.Keys)
				{
					if (!fFirst)
						tw.Write("    |");
					tw.WriteLine(sClass);
					fFirst = false;
				}
				tw.WriteLine(")*>");
				tw.WriteLine("<!ATTLIST rt");
				tw.WriteLine("  class NMTOKEN #REQUIRED");
				tw.WriteLine("  guid CDATA #REQUIRED");
				tw.WriteLine("  ownerguid CDATA #IMPLIED");
				tw.WriteLine("  owningflid CDATA #IMPLIED");
				tw.WriteLine("  owningord CDATA #IMPLIED>");
				tw.WriteLine();

				foreach (ModelClass mclass in m_mapNameClass.Values)
				{
					mclass.WriteOldDtdElement(tw, mapNameProps);
				}
				WritePropertyDtdElements(mapNameProps, tw);
				WriteDtdBoilerplate(tw, true);
			}
		}

		/// <summary>
		/// Write a DTD for the loaded model.
		/// </summary>
		public void WriteDtd(string sSchemaFile, bool fOld)
		{
			if (fOld)
				WriteOldDtd(sSchemaFile);
			else
				WriteDtd(sSchemaFile);
		}
	}
}
