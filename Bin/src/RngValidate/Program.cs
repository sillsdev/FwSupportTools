using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Commons.Xml.Relaxng;
using System.Xml;

namespace RngValidate
{
	/// <summary>
	/// This program is a thin wrapper around RelaxngValidatingReader.  It merely reads the XML
	/// file, thus validating it.  It doesn't do anything with the data read.
	/// </summary>
	class Program
	{
		static void Main(string[] args)
		{
			if (args.Length == 2)
			{
				string msg = GetFirstValidationError(args[0], args[1]);
				if (msg == null)
				{
					Console.WriteLine("{0} is valid.", args[1]);
				}
				else
				{
					Console.WriteLine(msg);
				}
			}
			else
			{
				Console.WriteLine("Usage: RngValidate <rngSchemaFile> <xmlFile>");
			}
		}

		static public string GetFirstValidationError(string rngFile, string document)
		{
			RelaxngValidatingReader reader = new RelaxngValidatingReader(
				new XmlTextReader(document), new XmlTextReader(rngFile));
			reader.ReportDetails = true;
			string lastGuy = "";
			try
			{
				while (!reader.EOF)
				{
					reader.Read();
					lastGuy = reader.Name;
				}
			}
			catch (Exception e)
			{
				return String.Format("{0}\r\nError near: {1} {2} '{3}'",
					e.Message, lastGuy, reader.Name, reader.Value);
			}
			return null;
		}
	}
}
