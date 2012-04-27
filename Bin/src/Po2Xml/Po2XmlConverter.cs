using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.AccessControl;
using System.Text.RegularExpressions;
using System.Xml;

namespace Po2Xml
{
	class Po2XmlConverter
	{
		public string PoFilePath { get; set; }
		public string XmlFilePath { get; set; }
		public bool Roundtrip { get; set; }

		public Po2XmlConverter()
		{
			PoFilePath = null;
			XmlFilePath = null;
			Roundtrip = false;
		}

		public int Run()
		{
			var writer = XmlWriter.Create(XmlFilePath);
			writer.WriteWhitespace(Environment.NewLine);
			writer.WriteProcessingInstruction("xml-stylesheet", "type=\"text/xsl\" href=\"format-html.xsl\"");
			writer.WriteWhitespace(Environment.NewLine);
			writer.WriteStartElement("messages");

			var msg = new PoMessageWriter(writer, Roundtrip);

			var fileInput = new StreamReader(PoFilePath);
			while (fileInput.Peek() >= 0)
			{
				var l = fileInput.ReadLine();
				if (l == null)
					break;

				// Continuation string?
				var m = (new Regex("\\s*\"(.*)\"")).Match(l);
				if(m.Success)
				{
					//Debug.Assert(msg.Current != null);
					msg.Current.Add(RemoveEscapeChars(m.Groups[1].ToString()));
				}
				else
				{
					msg.Flush();
				}
				m = new Regex("msgid \"(.*)\"", RegexOptions.Singleline).Match(l);
				if(m.Success)
				{
					msg.Msgid = new List<String> { RemoveEscapeChars(m.Groups[1].ToString()) };
					msg.Current = msg.Msgid;
				}
				m = new Regex("msgstr \"(.*)\"", RegexOptions.Singleline).Match(l);
				if (m.Success)
				{
					msg.Msgstr = new List<String> { RemoveEscapeChars(m.Groups[1].ToString()) };
					msg.Current = msg.Msgstr;
				}

				m = new Regex("# \\s*(.*)").Match(l);
				if (m.Success)
				{
					msg.UsrComment.Add(m.Groups[1].ToString());
				}

				m = new Regex("#\\.\\s*(.*)").Match(l);
				if (m.Success)
				{
					msg.DotComment.Add(m.Groups[1].ToString());
				}

				m = new Regex("#:\\s*(.*)").Match(l);
				if (m.Success)
				{
					msg.Reference.Add(m.Groups[1].ToString());
				}

				m = new Regex("#,\\s*(.*)").Match(l);
				if (m.Success)
				{
					msg.Flags.Add(m.Groups[1].ToString());
				}

			}
			msg.Flush();
			writer.WriteWhitespace(Environment.NewLine);
			writer.WriteEndDocument();
			writer.Close();
			return 0;
		}

		/// <summary>
		/// This will remove all escape characters with the exception of \n from the given string.
		/// </summary>
		/// <param name="p"></param>
		/// <returns></returns>
		private string RemoveEscapeChars(string p)
		{
			var result = p.Replace("\\n", "&#0xA;");
			result = result.Replace("\\", "");
			return result;
		}

		internal class PoMessageWriter
		{
			private readonly XmlWriter m_writer;
			private readonly bool m_roundtrip;
			private readonly string m_indent;
			private List<string> m_msgid;
			private List<string> m_msgstr;
			private List<string> m_usrcomment = new List<string>();
			private List<string> m_dotcomment = new List<string>();
			private List<string> m_reference = new List<string>();
			private List<string> m_flags = new List<string>();
			private List<string> m_current;

			public List<String> Current
			{
				get { return m_current ?? (m_current = new List<string>()); }
				set { m_current = value; }
			}

			public List<string> Msgid
			{
				get { return m_msgid; }
				set { m_msgid = value; }
			}

			public List<string> Msgstr
			{
				get { return m_msgstr; }
				set { m_msgstr = value; }
			}

			public List<string> UsrComment
			{
				get { return m_usrcomment; }
			}

			public List<string> DotComment
			{
				get { return m_dotcomment; }
			}

			public List<string> Reference
			{
				get { return m_reference; }
			}

			public List<string> Flags
			{
				get { return m_flags; }
			}

			public PoMessageWriter(XmlWriter writer, bool roundtrip = false, string indent = "  ")
			{
				m_writer = writer;
				m_roundtrip = roundtrip;
				m_indent = indent;
				Reset();
			}

			private void Reset()
			{
				m_msgid = new List<string>();
				m_msgstr = new List<string>();
				m_usrcomment = new List<string>();
				m_dotcomment = new List<string>();
				m_reference = new List<string>();
				m_flags = new List<string>();
				m_current = null;
			}

			public void Flush()
			{
				if (m_current != m_msgstr)
					return;
				Write();
				Reset();
			}

			private void Write()
			{
				m_writer.WriteWhitespace(Environment.NewLine);
				m_writer.WriteStartElement("msg");
				m_writer.WriteWhitespace(Environment.NewLine);

				if (m_roundtrip)
				{
					// Support exact round-tripping using multiple <key> and <str> child elements:
					foreach (var t in m_msgid)
						WriteElement("key", t);
					foreach (var t in m_msgstr)
						WriteElement("str", t);
				}
				else
				{
					// Concatenate parts into single <key> and <str> child elements:
					WriteElement("key", string.Join("", m_msgid));
					WriteElement("str", string.Join("", m_msgstr));
				}
				foreach (var t in m_usrcomment)
					WriteElement("comment", t);
				foreach (var t in m_dotcomment)
					WriteElement("info", t);
				foreach (var t in m_reference)
					WriteElement("ref", t);
				foreach (var t in m_flags)
					WriteElement("flags", t);
				m_writer.WriteEndElement();
			}

			private void WriteElement(string name, string data, params string[] attrs)
			{
				m_writer.WriteWhitespace(m_indent);
				m_writer.WriteStartElement(name);
				for (int i = 0; i < attrs.Length; i++)
				{
					if (i + 1 >= attrs.Length)
						throw new ArgumentException("ERROR: List of attributes for XML element " + name +
													" is missing data for attribute " + attrs[i]);
					m_writer.WriteAttributeString(attrs[i], attrs[i + 1]);
				}
				m_writer.WriteRaw(data);
				m_writer.WriteEndElement();
				m_writer.WriteWhitespace(Environment.NewLine);
			}
		}
	}
}