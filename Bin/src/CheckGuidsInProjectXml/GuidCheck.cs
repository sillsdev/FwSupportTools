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
// File: GuidCheck.cs
// Responsibility: mcconnel
//
// <remarks>
// </remarks>
// ---------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace SIL.FieldWorks.src.CheckGuidsInProjectXml
{
	/// ----------------------------------------------------------------------------------------
	/// <summary>
	/// This class handles loading an XML file and checking that all Guid references are
	/// satisfied.
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	public class GuidCheck
	{
		HashSet<Guid> m_guidsLayout = new HashSet<Guid>();
		HashSet<Guid> m_guidObjs = new HashSet<Guid>();
		/// <summary>
		/// This stores the information for a pending Guid reference.
		/// </summary>
		protected class GuidReference
		{
			Guid m_guid;
			String m_sAttribute;
			String m_sElement;
			int m_lineNumber;

			/// <summary>
			/// Constructor.
			/// </summary>
			public GuidReference(Guid guid, string sAttribute, string sElement, int lineNumber)
			{
				m_guid = guid;
				m_sAttribute = sAttribute;
				m_sElement = sElement;
				m_lineNumber = lineNumber;
			}

			/// <summary>
			/// Gets the guid value.
			/// </summary>
			public Guid Guid
			{
				get { return m_guid; }
			}

			/// <summary>
			/// Write an error message for this particular reference.
			/// </summary>
			public void WriteErrorMessage()
			{
				string sMsg = String.Format(
					"At line {0}, the guid value {1}=\"{2}\" in a <{3}> element does not link to anything.",
					m_lineNumber, m_sAttribute, m_guid.ToString(), m_sElement);
				Console.WriteLine(sMsg);
			}
		}
		List<GuidReference> m_guidRefs = new List<GuidReference>();

		// These values are copied from Src/Common/FwPrintLayoutComponents/HeaderFooterVc.cs
		// Since referencing them directly would require referencing multiple DLLs, I chose to
		// copy them instead.
		static readonly Guid PageNumberGuid = new Guid("644DF48A-3B60-45f4-80C7-739BE6E56A96");
		static readonly Guid FirstReferenceGuid = new Guid("397F43AE-E2B2-4f20-928A-1DF193C07674");
		static readonly Guid LastReferenceGuid = new Guid("85EE15C6-0799-46c6-8769-F9B3CE313AE2");
		static readonly Guid TotalPagesGuid = new Guid("E0EF9EDA-E4E2-4fcf-8720-5BC361BCE110");
		static readonly Guid PrintDateGuid = new Guid("C4556A21-41A8-4675-A74D-59B2C1A7E2B8");
		static readonly Guid DivisionNameGuid = new Guid("2277B85F-47BB-45c9-BC7A-7232E26E901C");
		static readonly Guid PublicationTitleGuid = new Guid("C8136D98-6957-43bd-BEA9-7DCE35200900");
		static readonly Guid PageReferenceGuid = new Guid("8978089A-8969-424e-AE54-B94C554F882D");
		static readonly Guid ProjectNameGuid = new Guid("5610D086-635F-4ae2-8E85-A95896F3D62D");
		static readonly Guid BookNameGuid = new Guid("48C0E5E3-C909-42e1-8F82-3489E3DE96FA");
		/// <summary>
		/// Constructor.  It actually does all the work of scanning the XML project file,
		/// storing the guids found as object ids and as object references.
		/// </summary>
		public GuidCheck(string sFilename)
		{
			// Store the fixed guids used for printing headers and footers.
			m_guidsLayout.Add(BookNameGuid);
			m_guidsLayout.Add(DivisionNameGuid);
			m_guidsLayout.Add(FirstReferenceGuid);
			m_guidsLayout.Add(LastReferenceGuid);
			m_guidsLayout.Add(PageNumberGuid);
			m_guidsLayout.Add(PageReferenceGuid);
			m_guidsLayout.Add(PrintDateGuid);
			m_guidsLayout.Add(ProjectNameGuid);
			m_guidsLayout.Add(PublicationTitleGuid);
			m_guidsLayout.Add(TotalPagesGuid);

			XmlTextReader reader = new XmlTextReader(sFilename);
			while (reader.Read())
			{
				if (reader.IsStartElement())
				{
					if (reader.Name == "rt")
					{
						Guid guid = ParseGuidValue(reader, "guid", "rt", true);
						if (guid != Guid.Empty)
							StoreObjectGuid(guid);
						Guid guidOwner = ParseGuidValue(reader, "ownerguid", "rt", false);
						if (guidOwner != Guid.Empty)
							StoreOrCheckGuidReference(guidOwner, "ownerguid", "rt", reader.LineNumber);
					}
					else if (reader.Name == "objsur")
					{
						Guid guid = ParseGuidValue(reader, "guid", "objsur", true);
						if (guid != Guid.Empty)
							StoreOrCheckGuidReference(guid, "guid", "objsur", reader.LineNumber);
					}
					else if (reader.Name == "Run")
					{
						Guid guid = ParseGuidValue(reader, "contextString", "Run", false);
						if (guid != Guid.Empty)
						{
							if (!m_guidsLayout.Contains(guid))
							{
								string sMsg = String.Format(
									"At line {0}, the guid value contextString=\"{2}\" in a <Run> element is not a proper Header/Footer Guid.",
									reader.LineNumber, guid.ToString());
								Console.WriteLine(sMsg);
							}
						}
						guid = ParseGuidValue(reader, "link", "Run", false);
						if (guid != Guid.Empty)
							StoreOrCheckGuidReference(guid, "link", "Run", reader.LineNumber);
						guid = ParseGuidValue(reader, "moveableObj", "Run", false);
						if (guid != Guid.Empty)
							StoreOrCheckGuidReference(guid, "moveableObj", "Run", reader.LineNumber);
						guid = ParseGuidValue(reader, "ownlink", "Run", false);
						if (guid != Guid.Empty)
							StoreOrCheckGuidReference(guid, "ownlink", "Run", reader.LineNumber);
						string sTags = reader.GetAttribute("tags");
						if (!String.IsNullOrEmpty(sTags))
						{
							string[] rgsTags = sTags.Split(new char[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
							for (int i = 0; i < rgsTags.Length; ++i)
							{
								guid = ParseGuid(rgsTags[i], "tags", "Run", reader.LineNumber);
								if (guid != Guid.Empty)
									StoreOrCheckGuidReference(guid, "tags", "Run", reader.LineNumber);
							}
						}
					}
				}
			}
		}

		private void StoreObjectGuid(Guid guid)
		{
			m_guidObjs.Add(guid);
		}

		private void StoreOrCheckGuidReference(Guid guid, string sAttribute, string sElement,
			int lineNumber)
		{
			if (m_guidObjs.Contains(guid))
				return;
			GuidReference gref = new GuidReference(guid, sAttribute, sElement, lineNumber);
			m_guidRefs.Add(gref);
		}

		private Guid ParseGuidValue(XmlTextReader reader, string sAttribute, string sElement,
			bool fRequired)
		{
			string sGuid = reader.GetAttribute(sAttribute);
			if (String.IsNullOrEmpty(sGuid))
			{
				if (fRequired)
				{
					string sMsg = String.Format(
						"At line {0}, the {1} attribute is missing from a <{2}> element.",
						reader.LineNumber, sAttribute, sElement);
					Console.WriteLine(sMsg);
				}
				return Guid.Empty;
			}
			return ParseGuid(sGuid, sAttribute, sElement, reader.LineNumber);
		}

		private Guid ParseGuid(string sGuid, string sAttribute, string sElement, int lineNumber)
		{
			try
			{
				Guid guid = new Guid(sGuid);
				return guid;
			}
			catch
			{
				string sMsg = String.Format(
					"At line {0}, the Guid format for {1}=\"{2}\" in a <{3}> element is invalid.",
					lineNumber, sAttribute, sGuid, sElement);
				Console.WriteLine(sMsg);
				return Guid.Empty;
			}
		}

		/// <summary>
		/// Check all pending guid references, and write any needed error messages.
		/// </summary>
		public void WriteBadReferences()
		{
			foreach (GuidReference gref in m_guidRefs)
			{
				if (!m_guidObjs.Contains(gref.Guid))
					gref.WriteErrorMessage();
			}
		}
	}
}
