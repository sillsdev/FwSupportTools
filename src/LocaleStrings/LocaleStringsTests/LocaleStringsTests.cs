using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using NUnit.Framework;
using LocaleStrings;
using System.IO;

namespace LocaleStringsTests
{
	[TestFixture]
	public class LocaleStringsTests
	{
		[Test]
		public void TestComputeAutoCommentFilePath()
		{
			var sAuto = Program.ComputeAutoCommentFilePath(@"E:\fwrepo\fw\Src\FwCoreDlgs\AddCnvtrDlg.resx", @"E:\fwrepo\fw");
			Assert.AreEqual(@"/Src/FwCoreDlgs/AddCnvtrDlg.resx", sAuto);

			sAuto = Program.ComputeAutoCommentFilePath(@"E:\fwrepo\fw\Src\FwCoreDlgs\AddCnvtrDlg.resx", @"C:\fwrepo\fw");
			Assert.AreEqual(@"E:/fwrepo/fw/Src/FwCoreDlgs/AddCnvtrDlg.resx", sAuto);

			sAuto = Program.ComputeAutoCommentFilePath("/home/steve/fwrepo/fw/DistFiles/Language Explorer/Configuration/Parts/LexEntry.fwlayout", "/home/steve/fwrepo/fw");
			Assert.AreEqual("/Language Explorer/Configuration/Parts/LexEntry.fwlayout", sAuto);

			sAuto = Program.ComputeAutoCommentFilePath("/home/steve/fwrepo/fw/DistFiles/Language Explorer/Configuration/Parts/LexEntry.fwlayout", "/home/john/fwrepo/fw");
			Assert.AreEqual("/home/steve/fwrepo/fw/DistFiles/Language Explorer/Configuration/Parts/LexEntry.fwlayout", sAuto);
		}

#region TestData
		private readonly string _sResxData =
"<?xml version=\"1.0\" encoding=\"utf-8\"?>" + Environment.NewLine +
"<root>" + Environment.NewLine +
"  <xsd:schema id=\"root\" xmlns=\"\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:msdata=\"urn:schemas-microsoft-com:xml-msdata\">" + Environment.NewLine +
"    <xsd:import namespace=\"http://www.w3.org/XML/1998/namespace\" />" + Environment.NewLine +
"  </xsd:schema>" + Environment.NewLine +
"  <resheader name=\"resmimetype\">" + Environment.NewLine +
"    <value>text/microsoft-resx</value>" + Environment.NewLine +
"  </resheader>" + Environment.NewLine +
"  <resheader name=\"version\">" + Environment.NewLine +
"    <value>2.0</value>" + Environment.NewLine +
"  </resheader>" + Environment.NewLine +
"  <resheader name=\"reader\">" + Environment.NewLine +
"    <value>System.Resources.ResXResourceReader, System.Windows.Forms, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>" + Environment.NewLine +
"  </resheader>" + Environment.NewLine +
"  <metadata name=\"btnHelp.GenerateMember\" type=\"System.Boolean, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\">" + Environment.NewLine +
"    <value>False</value>" + Environment.NewLine +
"  </metadata>" + Environment.NewLine +
"  <assembly alias=\"System.Windows.Forms\" name=\"System.Windows.Forms, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\" />" + Environment.NewLine +
"  <data name=\"btnHelp.TabIndex\" type=\"System.Int32, mscorlib\">" + Environment.NewLine +
"        <value>4</value>" + Environment.NewLine +
"  </data>" + Environment.NewLine +
"  <data name=\"btnHelp.Text\" xml:space=\"preserve\">" + Environment.NewLine +
"        <value>Help</value>" + Environment.NewLine +
"  </data>" + Environment.NewLine +
"  <data name=\"&gt;&gt;btnHelp.Name\" xml:space=\"preserve\">" + Environment.NewLine +
"        <value>btnHelp</value>" + Environment.NewLine +
"  </data>" + Environment.NewLine +
"  <data name=\"btnClose.TabIndex\" type=\"System.Int32, mscorlib\">" + Environment.NewLine +
"        <value>5</value>" + Environment.NewLine +
"  </data>" + Environment.NewLine +
"  <data name=\"btnClose.Text\" xml:space=\"preserve\">" + Environment.NewLine +
"        <value>Close</value>" + Environment.NewLine +
"  </data>" + Environment.NewLine +
"  <data name=\"&gt;&gt;btnClose.Name\" xml:space=\"preserve\">" + Environment.NewLine +
"        <value>btnClose</value>" + Environment.NewLine +
"  </data>" + Environment.NewLine +
"  <data name=\"label1.TabIndex\" type=\"System.Int32, mscorlib\">" + Environment.NewLine +
"        <value>0</value>" + Environment.NewLine +
"  </data>" + Environment.NewLine +
"  <data name=\"label1.Text\" xml:space=\"preserve\">" + Environment.NewLine +
"        <value>&amp;Available Converters:</value>" + Environment.NewLine +
"  </data>" + Environment.NewLine +
"  <data name=\"label1.TextAlign\" type=\"System.Drawing.ContentAlignment, System.Drawing\">" + Environment.NewLine +
"        <value>MiddleLeft</value>" + Environment.NewLine +
"  </data>" + Environment.NewLine +
"  <data name=\"propertiesTab.TabIndex\" type=\"System.Int32, mscorlib\">" + Environment.NewLine +
"        <value>0</value>" + Environment.NewLine +
"  </data>" + Environment.NewLine +
"  <data name=\"propertiesTab.Text\" xml:space=\"preserve\">" + Environment.NewLine +
"        <value>Properties</value>" + Environment.NewLine +
"  </data>" + Environment.NewLine +
"  <data name=\"&gt;&gt;propertiesTab.Name\" xml:space=\"preserve\">" + Environment.NewLine +
"        <value>propertiesTab</value>" + Environment.NewLine +
"  </data>" + Environment.NewLine +
"  <data name=\"$this.StartPosition\" type=\"System.Windows.Forms.FormStartPosition, System.Windows.Forms\">" + Environment.NewLine +
"        <value>CenterParent</value>" + Environment.NewLine +
"  </data>" + Environment.NewLine +
"  <data name=\"$this.Text\" xml:space=\"preserve\">" + Environment.NewLine +
"        <value>Encoding Converters</value>" + Environment.NewLine +
"  </data>" + Environment.NewLine +
"  <data name=\"&gt;&gt;$this.Name\" xml:space=\"preserve\">" + Environment.NewLine +
"        <value>AddCnvtrDlg</value>" + Environment.NewLine +
"  </data>" + Environment.NewLine +
"  <data name=\"kstidPleaseEmailThisTo0WithASuitableSubject\" xml:space=\"preserve\">" + Environment.NewLine +
"    <value>Please email this report to {0} with a suitable subject:" + Environment.NewLine +
"" + Environment.NewLine +
"{1}</value>" + Environment.NewLine +
"    <comment>{1} will be a long string...don't leave it out.</comment>" + Environment.NewLine +
"  </data>" + Environment.NewLine +
"</root>";
#endregion

		[Test]
		public void TestReadingResxData()
		{
			var rgsPoStrings = new List<POString>();
			var xdoc = new XmlDocument();
			xdoc.LoadXml(_sResxData);
			Assert.IsNotNull(xdoc.DocumentElement);
			Program.ProcessResxData(xdoc.DocumentElement, @"/Src/FwCoreDlgs/AddCnvtrDlg.resx", rgsPoStrings);
			//SUT
			Assert.AreEqual(6, rgsPoStrings.Count, "Five localizable strings found in resx data");
			var postr2 = rgsPoStrings[2];
			Assert.IsNotNull(postr2, "Third resx string has data");
			Assert.IsNotNull(postr2.MsgId, "Third resx string has MsgId data");
			Assert.AreEqual(1, postr2.MsgId.Count, "Third resx string has one line of MsgId data");
			Assert.AreEqual("&Available Converters:", postr2.MsgId[0], "Third resx string has the expected MsgId data");
			Assert.IsTrue(postr2.HasEmptyMsgStr, "Third resx string has no MsgStr data (as expected)");
			Assert.IsNull(postr2.UserComments, "Third resx string has no User Comments (as expected)");
			Assert.IsNull(postr2.Reference, "Third resx string has no Reference data (as expected)");
			Assert.IsNull(postr2.Flags, "Third resx string has no Flags data (as expected)");
			Assert.IsNotNull(postr2.AutoComments, "Third resx string has Auto Comments");
			Assert.AreEqual(1, postr2.AutoComments.Count, "Third resx string has one line of Auto Comments");
			Assert.AreEqual("/Src/FwCoreDlgs/AddCnvtrDlg.resx::label1.Text", postr2.AutoComments[0], "Third resx string has the expected Auto Comment");

			var postr4 = rgsPoStrings[4];
			Assert.IsNotNull(postr4, "Fifth resx string has data");
			Assert.IsNotNull(postr4.MsgId, "Fifth resx string has MsgId data");
			Assert.AreEqual(1, postr4.MsgId.Count, "Fifth resx string has one line of MsgId data");
			Assert.AreEqual("Encoding Converters", postr4.MsgId[0], "Fifth resx string has the expected MsgId data");
			Assert.IsTrue(postr4.HasEmptyMsgStr, "Fifth resx string has no MsgStr data");
			Assert.IsNull(postr4.UserComments, "Fifth resx string has no User Comments");
			Assert.IsNull(postr4.Reference, "Fifth resx string has no Reference data");
			Assert.IsNull(postr4.Flags, "Fifth resx string has no Flags data");
			Assert.IsNotNull(postr4.AutoComments, "Fifth resx string has Auto Comments");
			Assert.AreEqual(1, postr4.AutoComments.Count, "Fifth resx string has one line of Auto Comments");
			Assert.AreEqual("/Src/FwCoreDlgs/AddCnvtrDlg.resx::$this.Text", postr4.AutoComments[0], "Fifth resx string has the expected Auto Comment");

			var postr5 = rgsPoStrings[5];
			Assert.IsNotNull(postr5, "Sixth resx string has data");
			Assert.IsNotNull(postr5.MsgId, "Sixth resx string has MsgId data");
			Assert.AreEqual(3, postr5.MsgId.Count, "Sixth resx string has three lines of MsgId data");
			Assert.AreEqual("Please email this report to {0} with a suitable subject:\\n", postr5.MsgId[0], "Sixth resx string has the expected MsgId[0] data");
			Assert.AreEqual("\\n", postr5.MsgId[1], "Sixth resx string has the expected MsgId[1] data");
			Assert.AreEqual("{1}", postr5.MsgId[2], "Sixth resx string has the expected MsgId[2] data");
			Assert.IsTrue(postr5.HasEmptyMsgStr, "Sixth resx string has no MsgStr data");
			Assert.IsNull(postr5.UserComments, "Sixth resx string has no User Comments");
			Assert.IsNull(postr5.Reference, "Sixth resx string has no Reference data");
			Assert.IsNull(postr5.Flags, "Sixth resx string has no Flags data");
			Assert.IsNotNull(postr5.AutoComments, "Sixth resx string has Auto Comments");
			Assert.AreEqual(2, postr5.AutoComments.Count, "Sixth resx string has two lines of Auto Comments");
			Assert.AreEqual("{1} will be a long string...don't leave it out.", postr5.AutoComments[0], "Sixth resx string has the expected AutoComment0]");
			Assert.AreEqual("/Src/FwCoreDlgs/AddCnvtrDlg.resx::kstidPleaseEmailThisTo0WithASuitableSubject", postr5.AutoComments[1], "Sixth resx string has the expected AutoComment[1]");
		}

#region TestData
		private readonly string _sConfigData =
"<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + Environment.NewLine +
"<LayoutInventory xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:noNamespaceSchemaLocation='ViewsLayout.xsd'>" + Environment.NewLine +
"  <layout class=\"LexEntry\" type=\"detail\" name=\"Normal\">" + Environment.NewLine +
"    <part label=\"Lexeme Form\" ref=\"LexemeForm\"/>" + Environment.NewLine +
"    <part label=\"Citation Form\" ref=\"CitationFormAllV\"/>" + Environment.NewLine +
"    <part ref=\"ComplexFormEntries\" visibility=\"ifdata\"/>" + Environment.NewLine +
"    <part ref=\"EntryRefs\" param=\"Normal\" visibility=\"ifdata\"/>" + Environment.NewLine +
"    <part ref=\"SummaryDefinitionAllA\" visibility=\"ifdata\"/>" + Environment.NewLine +
"    <part ref=\"CurrentLexReferences\"   visibility=\"ifdata\" />" + Environment.NewLine +
"    <part customFields=\"here\" />" + Environment.NewLine +
"    <part ref=\"ImportResidue\" label=\"Import Residue\" visibility=\"ifdata\"/>" + Environment.NewLine +
"    <part ref=\"DateCreatedAllA\"  visibility=\"never\"/>" + Environment.NewLine +
"    <part ref=\"DateModifiedAllA\"  visibility=\"never\"/>" + Environment.NewLine +
"    <part ref=\"Messages\" visibility=\"always\"/>" + Environment.NewLine +
"      <part ref=\"Senses\" param=\"Normal\" expansion=\"expanded\"/>" + Environment.NewLine +
"    <part ref=\"VariantFormsSection\" expansion=\"expanded\" label=\"Variants\" menu=\"mnuDataTree-VariantForms\" hotlinks=\"mnuDataTree-VariantForms-Hotlinks\">" + Environment.NewLine +
"      <indent><part ref=\"VariantForms\"/></indent>" + Environment.NewLine +
"    </part>" + Environment.NewLine +
"    <part ref=\"AlternateFormsSection\" expansion=\"expanded\" label=\"Allomorphs\" menu=\"mnuDataTree-AlternateForms\" hotlinks=\"mnuDataTree-AlternateForms-Hotlinks\">" + Environment.NewLine +
"      <indent><part ref=\"AlternateForms\" param=\"Normal\"/></indent>" + Environment.NewLine +
"    </part>" + Environment.NewLine +
"    <part ref=\"GrammaticalFunctionsSection\" label=\"Grammatical Info. Details\" menu=\"mnuDataTree-Help\" hotlinks=\"mnuDataTree-Help\">" + Environment.NewLine +
"      <indent><part ref=\"MorphoSyntaxAnalyses\" param=\"Normal\"/></indent>" + Environment.NewLine +
"    </part>" + Environment.NewLine +
"    <part ref=\"PublicationSection\" label=\"Publication Settings\" menu=\"mnuDataTree-Help\" hotlinks=\"mnuDataTree-Help\">" + Environment.NewLine +
"      <indent>" + Environment.NewLine +
"        <part ref=\"PublishIn\"   visibility=\"always\" />" + Environment.NewLine +
"        <part ref=\"ShowMainEntryIn\" label=\"Show As Headword In\" visibility=\"always\" />" + Environment.NewLine +
"        <part ref=\"EntryRefs\" param=\"Publication\" visibility=\"ifdata\"/>" + Environment.NewLine +
"      </indent>" + Environment.NewLine +
"    </part>" + Environment.NewLine +
"  </layout>" + Environment.NewLine +
"  <layout class=\"LexEntry\" type=\"detail\" name=\"AsVariantForm\">" + Environment.NewLine +
"    <part ref=\"AsVariantForm\"/>" + Environment.NewLine +
"  </layout>" + Environment.NewLine +
"  <layout class=\"LexEntry\" type=\"jtview\" name=\"CrossRefPub\">" + Environment.NewLine +
"    <part ref=\"MLHeadWordPub\" label=\"Headword\" before=\" CrossRef:\" after=\". \" visibility=\"ifdata\" ws=\"vernacular\" sep=\"; \" showLabels=\"false\" style=\"Dictionary-CrossReferences\"/>" + Environment.NewLine +
"  </layout>" + Environment.NewLine +
"  <layout class=\"LexEntry\" type=\"jtview\" name=\"SubentryUnderPub\">" + Environment.NewLine +
"    <part ref=\"MLHeadWordPub\" label=\"Headword\" before=\" [\" after=\"]\" visibility=\"ifdata\" ws=\"vernacular\" sep=\"; \" hideConfig=\"true\" showLabels=\"false\" style=\"Dictionary-CrossReferences\"/>" + Environment.NewLine +
"  </layout>" + Environment.NewLine +
"</LayoutInventory>";
#endregion

		[Test]
		public void TestReadingDetailConfigData()
		{
			var rgsPoStrings = new List<POString>();
			var xdoc = new XmlDocument();
			xdoc.LoadXml(_sConfigData);
			Assert.IsNotNull(xdoc.DocumentElement);
			Program.ProcessConfigElement(xdoc.DocumentElement, "/Language Explorer/Configuration/Parts/LexEntry.fwlayout", rgsPoStrings);
			//SUT
			Assert.AreEqual(14, rgsPoStrings.Count);
			var postr5 = rgsPoStrings[5];
			Assert.IsNotNull(postr5, "Detail Config string[5] has data");
			Assert.IsNotNull(postr5.MsgId, "Detail Config string[5] has MsgId data");
			Assert.AreEqual(1, postr5.MsgId.Count, "Detail Config string[5] has one line of MsgId data");
			Assert.AreEqual("Grammatical Info. Details", postr5.MsgId[0], "Detail Config string[5] has the expected MsgId data");
			Assert.AreEqual("Grammatical Info. Details", postr5.MsgIdAsString(), "Detail Config string[5] is 'Grammatical Info. Details'");
			Assert.IsTrue(postr5.HasEmptyMsgStr, "Detail Config string[5] has no MsgStr data (as expected)");
			Assert.IsNull(postr5.UserComments, "Detail Config string[5] has no User Comments (as expected)");
			Assert.IsNull(postr5.Reference, "Detail Config string[5] has no Reference data (as expected)");
			Assert.IsNull(postr5.Flags, "Detail Config string[5] has no Flags data (as expected)");
			Assert.IsNotNull(postr5.AutoComments, "Detail Config string[5] has Auto Comments");
			Assert.AreEqual(1, postr5.AutoComments.Count, "Detail Config string[5] has one line of Auto Comments");
			Assert.AreEqual(
				"/Language Explorer/Configuration/Parts/LexEntry.fwlayout::/LayoutInventory/layout[\"LexEntry-detail-Normal\"]/part[@ref=\"GrammaticalFunctionsSection\"]/@label",
				postr5.AutoComments[0], "Detail Config string[5] has the expected Auto Comment");

			var postr8 = rgsPoStrings[8];
			Assert.IsNotNull(postr8, "Detail Config string[8] has data");
			Assert.IsNotNull(postr8.MsgId, "Detail Config string[8] has MsgId data");
			Assert.AreEqual(1, postr8.MsgId.Count, "Detail Config string[8] has one line of MsgId data");
			Assert.AreEqual("Headword", postr8.MsgId[0], "Detail Config string[8] has the expected MsgId data");
			Assert.AreEqual("Headword", rgsPoStrings[8].MsgIdAsString(), "Detail Config string[8] is 'Headword'");
			Assert.IsTrue(postr8.HasEmptyMsgStr, "Detail Config string[8] has no MsgStr data (as expected)");
			Assert.IsNull(postr8.UserComments, "Detail Config string[8] has no User Comments (as expected)");
			Assert.IsNull(postr8.Reference, "Detail Config string[8] has no Reference data (as expected)");
			Assert.IsNull(postr8.Flags, "Detail Config string[8] has no Flags data (as expected)");
			Assert.IsNotNull(postr8.AutoComments, "Detail Config string[8] has Auto Comments");
			Assert.AreEqual(1, postr8.AutoComments.Count, "Detail Config string[8] has one line of Auto Comments");
			Assert.AreEqual(
				"/Language Explorer/Configuration/Parts/LexEntry.fwlayout::/LayoutInventory/layout[\"LexEntry-jtview-CrossRefPub\"]/part[@ref=\"MLHeadWordPub\"]/@label",
				postr8.AutoComments[0], "Detail Config string[8] has the expected Auto Comment");

			var postr10 = rgsPoStrings[10];
			Assert.IsNotNull(postr10, "Detail Config string[10] has data");
			Assert.IsNotNull(postr10.MsgId, "Detail Config string[10] has MsgId data");
			Assert.AreEqual(1, postr10.MsgId.Count, "Detail Config string[10] has one line of MsgId data");
			Assert.AreEqual(" CrossRef:", postr10.MsgId[0], "Detail Config string[10] has the expected MsgId data");
			Assert.AreEqual(" CrossRef:", rgsPoStrings[10].MsgIdAsString(), "Detail Config string[8] is ' CrossRef:'");
			Assert.IsTrue(postr10.HasEmptyMsgStr, "Detail Config string[10] has no MsgStr data (as expected)");
			Assert.IsNull(postr10.UserComments, "Detail Config string[10] has no User Comments (as expected)");
			Assert.IsNull(postr10.Reference, "Detail Config string[10] has no Reference data (as expected)");
			Assert.IsNull(postr10.Flags, "Detail Config string[10] has no Flags data (as expected)");
			Assert.IsNotNull(postr10.AutoComments, "Detail Config string[10] has Auto Comments");
			Assert.AreEqual(1, postr10.AutoComments.Count, "Detail Config string[10] has one line of Auto Comments");
			Assert.AreEqual(
				"/Language Explorer/Configuration/Parts/LexEntry.fwlayout::/LayoutInventory/layout[\"LexEntry-jtview-CrossRefPub\"]/part[@ref=\"MLHeadWordPub\"]/@before",
				postr10.AutoComments[0], "Detail Config string[10] has the expected Auto Comment");

			var postr11 = rgsPoStrings[11];
			Assert.IsNotNull(postr11, "Detail Config string[11] has data");
			Assert.IsNotNull(postr11.MsgId, "Detail Config string[11] has MsgId data");
			Assert.AreEqual(1, postr11.MsgId.Count, "Detail Config string[11] has one line of MsgId data");
			Assert.AreEqual("Headword", postr11.MsgId[0], "Detail Config string[11] has the expected MsgId data");
			Assert.AreEqual("Headword", rgsPoStrings[11].MsgIdAsString(), "Detail Config string[8] is 'Headword'");
			Assert.IsTrue(postr11.HasEmptyMsgStr, "Detail Config string[11] has no MsgStr data (as expected)");
			Assert.IsNull(postr11.UserComments, "Detail Config string[11] has no User Comments (as expected)");
			Assert.IsNull(postr11.Reference, "Detail Config string[11] has no Reference data (as expected)");
			Assert.IsNull(postr11.Flags, "Detail Config string[11] has no Flags data (as expected)");
			Assert.IsNotNull(postr11.AutoComments, "Detail Config string[11] has Auto Comments");
			Assert.AreEqual(1, postr11.AutoComments.Count, "Detail Config string[11] has one line of Auto Comments");
			Assert.AreEqual(
				"/Language Explorer/Configuration/Parts/LexEntry.fwlayout::/LayoutInventory/layout[\"LexEntry-jtview-SubentryUnderPub\"]/part[@ref=\"MLHeadWordPub\"]/@label",
				postr11.AutoComments[0], "Detail Config string[11] has the expected Auto Comment");
		}

#region TestData
		private readonly string _sStringsEnData =
"<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + Environment.NewLine +
"<strings>" + Environment.NewLine +
"  <group id=\"Misc\">" + Environment.NewLine +
"    <string id=\"No Records\" txt=\"No Records\"/>" + Environment.NewLine +
"  </group>" + Environment.NewLine +
"  <group id=\"ClassNames\">" + Environment.NewLine +
"    <string id=\"LexEntry\" txt=\"Entry\"/>" + Environment.NewLine +
"    <string id=\"LexSense\" txt=\"Sense\"/>" + Environment.NewLine +
"  </group>" + Environment.NewLine +
"  <group id=\"PossibilityListItemTypeNames\">" + Environment.NewLine +
"    <string id=\"DomainTypes\" txt=\"Academic Domain\"/>" + Environment.NewLine +
"  </group>" + Environment.NewLine +
"  <group id=\"DialogStrings\">" + Environment.NewLine +
"    <string id=\"EditMorphBreaks-Example1\" txt=\"blackbird → {0}black{1} {0}bird{1}\"/>" + Environment.NewLine +
"    <string id=\"ChangeMorphTypeLoseStemNameGramInfo\" txt=\"The stem name and some grammatical information will be lost! Do you still want to continue?\"/>" + Environment.NewLine +
"  </group>" + Environment.NewLine +
"  <group id=\"Linguistics\">" + Environment.NewLine +
"    <group id=\"Morphology\">" + Environment.NewLine +
"      <group id=\"Adjacency\">" + Environment.NewLine +
"        <string id=\"Anywhere\" txt=\"anywhere around\"/>" + Environment.NewLine +
"        <string id=\"Somewhere to left\" txt=\"somewhere before\"/>" + Environment.NewLine +
"        <string id=\"Somewhere to right\" txt=\"somewhere after\"/>" + Environment.NewLine +
"      </group>" + Environment.NewLine +
"      <group id=\"TemplateTable\">" + Environment.NewLine +
"        <string id=\"Stem\" txt=\"STEM\"/>" + Environment.NewLine +
"        <string id=\"SlotChooserTitle\" txt=\"Choose Slot\"/>" + Environment.NewLine +
"        <string id=\"SlotChooserInstructionalText\" txt=\"The following slots are available for the category {0}.\"/>" + Environment.NewLine +
"      </group>" + Environment.NewLine +
"    </group>" + Environment.NewLine +
"  </group>" + Environment.NewLine +
"</strings>";
#endregion

		[Test]
		public void TestReadingStringsEnData()
		{
			var rgsPoStrings = new List<POString>();
			var xdoc = new XmlDocument();
			xdoc.LoadXml(_sStringsEnData);
			Assert.IsNotNull(xdoc.DocumentElement);
			Program.ProcessStringsElement(xdoc.DocumentElement, rgsPoStrings);
			//SUT
			Assert.AreEqual(12, rgsPoStrings.Count);
			var postr0 = rgsPoStrings[0];
			Assert.IsNotNull(postr0, "strings-en string[0] has data");
			Assert.IsNotNull(postr0.MsgId, "strings-en string[0] has MsgId data");
			Assert.AreEqual(1, postr0.MsgId.Count, "strings-en string[0] has one line of MsgId data");
			Assert.AreEqual("No Records", postr0.MsgId[0], "strings-en string[0] has the expected MsgId data");
			Assert.AreEqual("No Records", postr0.MsgIdAsString(), "strings-en string[0] is 'No Records'");
			Assert.IsTrue(postr0.HasEmptyMsgStr, "strings-en string[0] has no MsgStr data (as expected)");
			Assert.IsNull(postr0.UserComments, "strings-en string[0] has no User Comments (as expected)");
			Assert.IsNull(postr0.Reference, "strings-en string[0] has no Reference data (as expected)");
			Assert.IsNull(postr0.Flags, "strings-en string[0] has no Flags data (as expected)");
			Assert.IsNotNull(postr0.AutoComments, "strings-en string[0] has Auto Comments");
			Assert.AreEqual(1, postr0.AutoComments.Count, "strings-en string[0] has one line of Auto Comments");
			Assert.AreEqual("/|strings-en.xml::/Misc/No Records|", postr0.AutoComments[0], "strings-en string[0] has the expected Auto Comment");

			var postr6 = rgsPoStrings[6];
			Assert.IsNotNull(postr6, "strings-en string[6] has data");
			Assert.IsNotNull(postr6.MsgId, "strings-en string[6] has MsgId data");
			Assert.AreEqual(1, postr6.MsgId.Count, "strings-en string[6] has one line of MsgId data");
			Assert.AreEqual("anywhere around", postr6.MsgId[0], "strings-en string[6] has the expected MsgId data");
			Assert.AreEqual("anywhere around", postr6.MsgIdAsString(), "strings-en string[6] is 'anywhere around'");
			Assert.IsTrue(postr6.HasEmptyMsgStr, "strings-en string[6] has no MsgStr data (as expected)");
			Assert.IsNull(postr6.UserComments, "strings-en string[6] has no User Comments (as expected)");
			Assert.IsNull(postr6.Reference, "strings-en string[6] has no Reference data (as expected)");
			Assert.IsNull(postr6.Flags, "strings-en string[6] has no Flags data (as expected)");
			Assert.IsNotNull(postr6.AutoComments, "strings-en string[6] has Auto Comments");
			Assert.AreEqual(1, postr6.AutoComments.Count, "strings-en string[6] has one line of Auto Comments");
			Assert.AreEqual("/|strings-en.xml::/Linguistics/Morphology/Adjacency/Anywhere|", postr6.AutoComments[0], "strings-en string[6] has the expected Auto Comment");
		}

#region TestData
		private readonly string _sDictConfigData =
"<?xml version=\"1.0\" encoding=\"utf-8\"?>" + Environment.NewLine +
"<DictionaryConfiguration name=\"Root-based (complex forms as subentries)\" allPublications=\"true\" version=\"1\" lastModified=\"2014-10-07\">" + Environment.NewLine +
"  <ConfigurationItem name=\"Main Entry\" style=\"Dictionary-Normal\" isEnabled=\"true\" field=\"LexEntry\" cssClassNameOverride=\"entry\">" + Environment.NewLine +
"    <ParagraphOptions paragraphStyle=\"Dictionary-Normal\" continuationParagraphStyle=\"Dictionary-Continuation\" />" + Environment.NewLine +
"    <ConfigurationItem name=\"Headword\" between=\" \" after=\"  \" style=\"Dictionary-Headword\" isEnabled=\"true\" field=\"MLHeadWord\" cssClassNameOverride=\"mainheadword\">" + Environment.NewLine +
"      <WritingSystemOptions writingSystemType=\"vernacular\" displayWSAbreviation=\"false\">" + Environment.NewLine +
"        <Option id=\"vernacular\" isEnabled=\"true\"/>" + Environment.NewLine +
"      </WritingSystemOptions>" + Environment.NewLine +
"    </ConfigurationItem>" + Environment.NewLine +
"    <ConfigurationItem name=\"Summary Definition\" before=\" \" between=\" \" after=\" \" isEnabled=\"false\" field=\"SummaryDefinition\"/>" + Environment.NewLine +
"    <ConfigurationItem name=\"Senses\" between=\"  \" after=\" \" isEnabled=\"true\" field=\"SensesOS\" cssClassNameOverride=\"senses\">" + Environment.NewLine +
"      <SenseOptions displayEachSenseInParagraph=\"false\" numberStyle=\"Dictionary-SenseNumber\" numberBefore=\"\" numberAfter=\") \" numberingStyle=\"%d\" numberSingleSense=\"false\" showSingleGramInfoFirst=\"true\"/>" + Environment.NewLine +
"      <ConfigurationItem name=\"Grammatical Info.\" after=\" \" style=\"Dictionary-Contrasting\" isEnabled=\"true\" field=\"MorphoSyntaxAnalysisRA\" cssClassNameOverride=\"morphosyntaxanalysis\">" + Environment.NewLine +
"	<ConfigurationItem name=\"Gram Info (Name)\" between=\" \" after=\" \" isEnabled=\"false\" field=\"InterlinearNameTSS\" cssClassNameOverride=\"graminfoname\"/>" + Environment.NewLine +
"	<ConfigurationItem name=\"Gram Info (Abbrev)\" between=\" \" after=\" \" isEnabled=\"false\" field=\"InterlinearAbbrTSS\" cssClassNameOverride=\"graminfoabbrev\"/>" + Environment.NewLine +
"	<ConfigurationItem name=\"Category Info.\" between=\" \" after=\" \" isEnabled=\"true\" field=\"MLPartOfSpeech\"/>" + Environment.NewLine +
"      </ConfigurationItem>" + Environment.NewLine +
"      <ConfigurationItem name=\"Definition (or Gloss)\" between=\" \" after=\" \" isEnabled=\"true\" field=\"DefinitionOrGloss\"/>" + Environment.NewLine +
"      <ConfigurationItem name=\"Semantic Domains\" before=\"(sem. domains: \" between=\", \" after=\".) \" isEnabled=\"true\" field=\"SemanticDomainsRC\" cssClassNameOverride=\"semanticdomains\">" + Environment.NewLine +
"	<ConfigurationItem name=\"Abbreviation\" between=\" \" after=\" - \" isEnabled=\"true\" field=\"Abbreviation\"/>" + Environment.NewLine +
"	<ConfigurationItem name=\"Name\" between=\" \" isEnabled=\"true\" field=\"Name\"/>" + Environment.NewLine +
"      </ConfigurationItem>" + Environment.NewLine +
"    </ConfigurationItem>" + Environment.NewLine +
"    <ConfigurationItem name=\"Date Created\" before=\"created on: \" after=\" \" isEnabled=\"false\" field=\"DateCreated\"/>" + Environment.NewLine +
"    <ConfigurationItem name=\"Date Modified\" before=\"modified on: \" after=\" \" isEnabled=\"false\" field=\"DateModified\"/>" + Environment.NewLine +
"  </ConfigurationItem>" + Environment.NewLine +
"  <ConfigurationItem name=\"Minor Entry (Complex Forms)\" style=\"Dictionary-Minor\" isEnabled=\"true\" field=\"LexEntry\" cssClassNameOverride=\"minorentrycomplex\">" + Environment.NewLine +
"    <ListTypeOptions list=\"complex\">" + Environment.NewLine +
"      <Option isEnabled=\"true\"  id=\"a0000000-dd15-4a03-9032-b40faaa9a754\"/>" + Environment.NewLine +
"    </ListTypeOptions>" + Environment.NewLine +
"    <ConfigurationItem name=\"Headword\" between=\" \" after=\"  \" style=\"Dictionary-Headword\" isEnabled=\"true\" field=\"MLHeadWord\" cssClassNameOverride=\"headword\"/>" + Environment.NewLine +
"    <ConfigurationItem name=\"Allomorphs\" between=\", \" after=\" \" isEnabled=\"false\" field=\"AlternateFormsOS\" cssClassNameOverride=\"allomorphs\">" + Environment.NewLine +
"      <ConfigurationItem name=\"Morph Type\" between=\" \" after=\" \" isEnabled=\"false\" field=\"MorphTypeRA\" cssClassNameOverride=\"morphtype\">" + Environment.NewLine +
"	<ConfigurationItem name=\"Abbreviation\" between=\" \" isEnabled=\"false\" field=\"Abbreviation\"/>" + Environment.NewLine +
"	<ConfigurationItem name=\"Name\" between=\" \" isEnabled=\"false\" field=\"Name\"/>" + Environment.NewLine +
"      </ConfigurationItem>" + Environment.NewLine +
"      <ConfigurationItem name=\"Allomorph\" between=\" \" after=\" \" isEnabled=\"false\" field=\"Form\"/>" + Environment.NewLine +
"      <ConfigurationItem name=\"Environments\" isEnabled=\"false\" between=\", \" after=\" \" field=\"AllomorphEnvironments\">" + Environment.NewLine +
"	<ConfigurationItem name=\"String Representation\" isEnabled=\"true\" field=\"StringRepresentation\"/>" + Environment.NewLine +
"      </ConfigurationItem>" + Environment.NewLine +
"    </ConfigurationItem>" + Environment.NewLine +
"    <ConfigurationItem name=\"Date Created\" before=\"created on: \" after=\" \" isEnabled=\"false\" field=\"DateCreated\"/>" + Environment.NewLine +
"    <ConfigurationItem name=\"Date Modified\" before=\"modified on: \" after=\" \" isEnabled=\"false\" field=\"DateModified\"/>" + Environment.NewLine +
"  </ConfigurationItem>" + Environment.NewLine +
"</DictionaryConfiguration>";
#endregion

		[Test]
		public void TestReadingDictConfigData()
		{
			var rgsPoStrings = new List<POString>();
			var xdoc = new XmlDocument();
			xdoc.LoadXml(_sDictConfigData);
			Assert.IsNotNull(xdoc.DocumentElement);
			Program.ProcessFwDictConfigElement(xdoc.DocumentElement, "/Language Explorer/DefaultConfigurations/Dictionary/Root.fwdictconfig", rgsPoStrings);
			//SUT
			Assert.AreEqual(36, rgsPoStrings.Count);
			var postr0 = rgsPoStrings[0];
			Assert.IsNotNull(postr0, "fwdictconfig string[0] has data");
			Assert.IsNotNull(postr0.MsgId, "fwdictconfig string[0] has MsgId data");
			Assert.AreEqual(1, postr0.MsgId.Count, "fwdictconfig string[0] has one line of MsgId data");
			Assert.AreEqual("Root-based (complex forms as subentries)", postr0.MsgId[0], "fwdictconfig string[0] has the expected MsgId data");
			Assert.AreEqual("Root-based (complex forms as subentries)", postr0.MsgIdAsString(), "fwdictconfig string[0] is 'Root-based (complex forms as subentries)'");
			Assert.IsTrue(postr0.HasEmptyMsgStr, "fwdictconfig string[0] has no MsgStr data (as expected)");
			Assert.IsNull(postr0.UserComments, "fwdictconfig string[0] has no User Comments (as expected)");
			Assert.IsNull(postr0.Reference, "fwdictconfig string[0] has no Reference data (as expected)");
			Assert.IsNull(postr0.Flags, "fwdictconfig string[0] has no Flags data (as expected)");
			Assert.IsNotNull(postr0.AutoComments, "fwdictconfig string[0] has Auto Comments");
			Assert.AreEqual(1, postr0.AutoComments.Count, "fwdictconfig string[0] has one line of Auto Comments");
			Assert.AreEqual("/Language Explorer/DefaultConfigurations/Dictionary/Root.fwdictconfig:://DictionaryConfiguration/@name",
				postr0.AutoComments[0], "fwdictconfig string[0] has the expected Auto Comment");

			var postr5 = rgsPoStrings[5];
			Assert.IsNotNull(postr5, "fwdictconfig string[5] has data");
			Assert.IsNotNull(postr5.MsgId, "fwdictconfig string[5] has MsgId data");
			Assert.AreEqual(1, postr5.MsgId.Count, "fwdictconfig string[5] has one line of MsgId data");
			Assert.AreEqual("Grammatical Info.", postr5.MsgId[0], "fwdictconfig string[5] has the expected MsgId data");
			Assert.AreEqual("Grammatical Info.", postr5.MsgIdAsString(), "fwdictconfig string[5] is 'Grammatical Info.'");
			Assert.IsTrue(postr5.HasEmptyMsgStr, "fwdictconfig string[5] has no MsgStr data (as expected)");
			Assert.IsNull(postr5.UserComments, "fwdictconfig string[5] has no User Comments (as expected)");
			Assert.IsNull(postr5.Reference, "fwdictconfig string[5] has no Reference data (as expected)");
			Assert.IsNull(postr5.Flags, "fwdictconfig string[5] has no Flags data (as expected)");
			Assert.IsNotNull(postr5.AutoComments, "fwdictconfig string[5] has Auto Comments");
			Assert.AreEqual(1, postr5.AutoComments.Count, "fwdictconfig string[5] has one line of Auto Comments");
			Assert.AreEqual("/Language Explorer/DefaultConfigurations/Dictionary/Root.fwdictconfig:://ConfigurationItem/@name",
				postr5.AutoComments[0], "fwdictconfig string[5] has the expected Auto Comment");

			var postr34 = rgsPoStrings[34];
			Assert.IsNotNull(postr34, "fwdictconfig string[34] has data");
			Assert.IsNotNull(postr34.MsgId, "fwdictconfig string[34] has MsgId data");
			Assert.AreEqual(1, postr34.MsgId.Count, "fwdictconfig string[34] has one line of MsgId data");
			Assert.AreEqual("Date Modified", postr34.MsgId[0], "fwdictconfig string[34] has the expected MsgId data");
			Assert.AreEqual("Date Modified", postr34.MsgIdAsString(), "fwdictconfig string[34] is 'Date Modified'");
			Assert.IsTrue(postr34.HasEmptyMsgStr, "fwdictconfig string[34] has no MsgStr data (as expected)");
			Assert.IsNull(postr34.UserComments, "fwdictconfig string[34] has no User Comments (as expected)");
			Assert.IsNull(postr34.Reference, "fwdictconfig string[34] has no Reference data (as expected)");
			Assert.IsNull(postr34.Flags, "fwdictconfig string[34] has no Flags data (as expected)");
			Assert.IsNotNull(postr34.AutoComments, "fwdictconfig string[34] has Auto Comments");
			Assert.AreEqual(1, postr34.AutoComments.Count, "fwdictconfig string[34] has one line of Auto Comments");
			Assert.AreEqual("/Language Explorer/DefaultConfigurations/Dictionary/Root.fwdictconfig:://ConfigurationItem/@name",
				postr34.AutoComments[0], "fwdictconfig string[34] has the expected Auto Comment");

			var postr35 = rgsPoStrings[35];
			Assert.IsNotNull(postr35, "fwdictconfig string[35] has data");
			Assert.IsNotNull(postr35.MsgId, "fwdictconfig string[35] has MsgId data");
			Assert.AreEqual(1, postr35.MsgId.Count, "fwdictconfig string[35] has one line of MsgId data");
			Assert.AreEqual("modified on: ", postr35.MsgId[0], "fwdictconfig string[35] has the expected MsgId data");
			Assert.AreEqual("modified on: ", postr35.MsgIdAsString(), "fwdictconfig string[35] is 'modified on: '");
			Assert.IsTrue(postr35.HasEmptyMsgStr, "fwdictconfig string[35] has no MsgStr data (as expected)");
			Assert.IsNull(postr35.UserComments, "fwdictconfig string[35] has no User Comments (as expected)");
			Assert.IsNull(postr35.Reference, "fwdictconfig string[35] has no Reference data (as expected)");
			Assert.IsNull(postr35.Flags, "fwdictconfig string[35] has no Flags data (as expected)");
			Assert.IsNotNull(postr35.AutoComments, "fwdictconfig string[35] has Auto Comments");
			Assert.AreEqual(1, postr35.AutoComments.Count, "fwdictconfig string[35] has one line of Auto Comments");
			Assert.AreEqual("/Language Explorer/DefaultConfigurations/Dictionary/Root.fwdictconfig:://ConfigurationItem/@before",
				postr35.AutoComments[0], "fwdictconfig string[35] has the expected Auto Comment");
		}

		[Test]
		public void TestWriteAndReadPotFile()
		{
			var rgsPoStrings = new List<POString>();
			var xdoc = new XmlDocument();
			xdoc.LoadXml(_sResxData);
			Assert.IsNotNull(xdoc.DocumentElement);
			Program.ProcessResxData(xdoc.DocumentElement, @"/Src/FwCoreDlgs/AddCnvtrDlg.resx", rgsPoStrings);
			xdoc.LoadXml(_sConfigData);
			Assert.IsNotNull(xdoc.DocumentElement);
			Program.ProcessConfigElement(xdoc.DocumentElement, "/Language Explorer/Configuration/Parts/LexEntry.fwlayout", rgsPoStrings);
			xdoc.LoadXml(_sStringsEnData);
			Assert.IsNotNull(xdoc.DocumentElement);
			Program.ProcessStringsElement(xdoc.DocumentElement, rgsPoStrings);
			xdoc.LoadXml(_sDictConfigData);
			Assert.IsNotNull(xdoc.DocumentElement);
			Program.ProcessFwDictConfigElement(xdoc.DocumentElement, "/Language Explorer/DefaultConfigurations/Dictionary/Root.fwdictconfig", rgsPoStrings);
			Assert.AreEqual(68, rgsPoStrings.Count);
			Assert.AreEqual("Help", rgsPoStrings[0].MsgIdAsString());
			Assert.AreEqual("modified on: ", rgsPoStrings[67].MsgIdAsString());
			rgsPoStrings.Sort(POString.CompareMsgIds);
			POString.MergeDuplicateStrings(rgsPoStrings);
			// SUT (everything before is tested already in another test)
			Assert.AreEqual(56, rgsPoStrings.Count);
			Assert.AreEqual(" - ", rgsPoStrings[0].MsgIdAsString());
			Assert.AreEqual("Variants", rgsPoStrings[55].MsgIdAsString());
			StringWriter sw = new StringWriter();
			Program.WritePotFile(sw, "/home/testing/fw", rgsPoStrings);
			var potFileStr = sw.ToString();
			Assert.IsNotNull(potFileStr);
			StringReader sr = new StringReader(potFileStr);
			POString posHeader;
			var dictPot = Program.ReadPotFile(sr, out posHeader);
			Assert.AreEqual(56, dictPot.Count);
			var rgsPot = dictPot.ToList();
			Assert.AreEqual(" - ", rgsPot[0].Value.MsgIdAsString());
			Assert.AreEqual("Variants", rgsPot[55].Value.MsgIdAsString());

			var posHeadword = dictPot["Headword"];
			Assert.AreEqual(4, posHeadword.AutoComments.Count);
			Assert.AreEqual("/Language Explorer/Configuration/Parts/LexEntry.fwlayout::/LayoutInventory/layout[\"LexEntry-jtview-CrossRefPub\"]/part[@ref=\"MLHeadWordPub\"]/@label", posHeadword.AutoComments[0]);
			Assert.AreEqual("/Language Explorer/Configuration/Parts/LexEntry.fwlayout::/LayoutInventory/layout[\"LexEntry-jtview-SubentryUnderPub\"]/part[@ref=\"MLHeadWordPub\"]/@label", posHeadword.AutoComments[1]);
			Assert.AreEqual("/Language Explorer/DefaultConfigurations/Dictionary/Root.fwdictconfig:://ConfigurationItem/@name", posHeadword.AutoComments[2]);
			Assert.AreEqual("(String used 4 times.)", posHeadword.AutoComments[3]);

			var posComma = dictPot[", "];
			Assert.AreEqual(2, posComma.AutoComments.Count);
			Assert.AreEqual("/Language Explorer/DefaultConfigurations/Dictionary/Root.fwdictconfig:://ConfigurationItem/@between", posComma.AutoComments[0]);
			Assert.AreEqual("(String used 3 times.)", posComma.AutoComments[1]);
		}

#region TestData
		static private readonly string _sFrenchPoData =
"#  Copyright (c) 2005-2015 SIL International" + Environment.NewLine +
"#  This software is licensed under the LGPL, version 2.1 or later" + Environment.NewLine +
"#  (http://www.gnu.org/licenses/lgpl-2.1.html)" + Environment.NewLine +
"msgid \"\"" + Environment.NewLine +
"msgstr \"\"" + Environment.NewLine +
"\"Project-Id-Version: FieldWorks 8.1.3\\n\"" + Environment.NewLine +
"\"Report-Msgid-Bugs-To: FlexErrors@sil.org\\n\"" + Environment.NewLine +
"\"POT-Creation-Date: 2015-02-03T11:52:27.7123661-06:00\\n\"" + Environment.NewLine +
"\"PO-Revision-Date: 2015-02-03 19:31-0800\\n\"" + Environment.NewLine +
"\"Last-Translator: John Doe <john_doe@nowhere.org>\\n\"" + Environment.NewLine +
"\"Language-Team: French <John_Doe@nowhere.org>\\n\"" + Environment.NewLine +
"\"MIME-Version: 1.0\\n\"" + Environment.NewLine +
"\"Content-Type: text/plain; charset=UTF-8\\n\"" + Environment.NewLine +
"\"Content-Transfer-Encoding: 8bit\\n\"" + Environment.NewLine +
"\"X-Poedit-Language: French\\n\"" + Environment.NewLine +
"" + Environment.NewLine +
"#. separate name and abbreviation (space dash space)" + Environment.NewLine +
"#. /Language Explorer/Configuration/Parts/CmPossibilityParts.xml::/PartInventory/bin/part[@id=\"CmPossibility-Jt-AbbrAndName\"]/lit" + Environment.NewLine +
"#. /Src/FDO/Strings.resx::ksNameAbbrSep" + Environment.NewLine +
"msgid \" - \"" + Environment.NewLine +
"msgstr \" - \"" + Environment.NewLine +
"" + Environment.NewLine +
"#. Used in Description on the General tab of the Style dialog box to separate detail items" + Environment.NewLine +
"#. Separator to use between multiple slot names when an irregularly inflected form variant fills two or more inflectional affix slots" + Environment.NewLine +
"#. /Src/FDO/Strings.resx::ksListSep" + Environment.NewLine +
"#. /Src/LexText/ParserUI/ParserUIStrings.resx::ksSlotNameSeparator" + Environment.NewLine +
"msgid \", \"" + Environment.NewLine +
"msgstr \", \"" + Environment.NewLine +
"" + Environment.NewLine +
"#. /Language Explorer/Configuration/Parts/CellarParts.xml::/PartInventory/bin/part[@id=\"CmPossibility-Jt-AbbreviationDot\"]/lit" + Environment.NewLine +
"msgid \". \"" + Environment.NewLine +
"msgstr \". \"" + Environment.NewLine +
"" + Environment.NewLine +
"#. /Src/LexText/Interlinear/ITextStrings.resx::ksEndTagSymbol" + Environment.NewLine +
"#. /Src/LexText/LexTextControls/OccurrenceDlg.resx::m_bracketLabel.Text" + Environment.NewLine +
"msgid \"]\"" + Environment.NewLine +
"msgstr \"]\"" + Environment.NewLine +
"" + Environment.NewLine +
"#. Used in CustomListDlg - Display items by combobox" + Environment.NewLine +
"#. /Language Explorer/Configuration/Parts/ReversalParts.xml::/PartInventory/bin/part[@id=\"CmPossibility-Detail-AbbreviationRevPOS\"]/slice/@label" + Environment.NewLine +
"#. /Src/xWorks/xWorksStrings.resx::ksAbbreviation" + Environment.NewLine +
"msgid \"Abbreviation\"" + Environment.NewLine +
"msgstr \"Abréviation\"" + Environment.NewLine +
"" + Environment.NewLine +
"# JDX:JN" + Environment.NewLine +
"#. /|strings-en.xml::/PossibilityListItemTypeNames/DomainTypes|" + Environment.NewLine +
"msgid \"Academic Domain\"" + Environment.NewLine +
"msgstr \"Domaine technique\"" + Environment.NewLine +
"" + Environment.NewLine +
"#. Label for possible class to add custom fields to" + Environment.NewLine +
"#. /Language Explorer/Configuration/Parts/MorphologyParts.xml::/PartInventory/bin/part[@id=\"MoAlloAdhocProhib-Jt-Type\"]/if/lit" + Environment.NewLine +
"#. /Src/xWorks/xWorksStrings.resx::Allomorph" + Environment.NewLine +
"msgid \"Allomorph\"" + Environment.NewLine +
"msgstr \"Allomorphe\"" + Environment.NewLine +
"" + Environment.NewLine +
"#. /|strings-en.xml::/AlternativeTitles/MoForm-Plural|" + Environment.NewLine +
"#. /Language Explorer/Configuration/Lexicon/browseDialogColumns.xml::/doc/browseColumns/column/@label" + Environment.NewLine +
"msgid \"Allomorphs\"" + Environment.NewLine +
"msgstr \"Les allomorphes\"" + Environment.NewLine +
"" + Environment.NewLine +
"#. /|strings-en.xml::/Linguistics/Morphology/Adjacency/Anywhere|" + Environment.NewLine +
"msgid \"anywhere around\"" + Environment.NewLine +
"msgstr \"n'importe où autour\"" + Environment.NewLine +
"" + Environment.NewLine +
"#. /Src/FwCoreDlgs/AddCnvtrDlg.resx::label1.Text" + Environment.NewLine +
"msgid \"&Available Converters:\"" + Environment.NewLine +
"msgstr \"Convertisseurs disponibles:\"" + Environment.NewLine +
"" + Environment.NewLine +
"#. /|strings-en.xml::/DialogStrings/EditMorphBreaks-Example1|" + Environment.NewLine +
"msgid \"blackbird → {0}black{1} {0}bird{1}\"" + Environment.NewLine +
"msgstr \"blackbird → {0}black{1} {0}bird{1}\"" + Environment.NewLine +
"" + Environment.NewLine +
"#. /Language Explorer/Configuration/Parts/MorphologyParts.xml::/PartInventory/bin/part[@id=\"MoMorphSynAnalysis-Detail-MainEdit\"]/slice/@label" + Environment.NewLine +
"msgid \"Category Info.\"" + Environment.NewLine +
"msgstr \"Catégorie info.\"" + Environment.NewLine +
"" + Environment.NewLine +
"#. /|strings-en.xml::/Linguistics/Morphology/TemplateTable/SlotChooserTitle|" + Environment.NewLine +
"msgid \"Choose Slot\"" + Environment.NewLine +
"msgstr \"Choisir case\"" + Environment.NewLine +
"" + Environment.NewLine +
"#. /Language Explorer/Configuration/Lexicon/browseDialogColumns.xml::/doc/browseColumns/column/@label" + Environment.NewLine +
"msgid \"Citation Form\"" + Environment.NewLine +
"msgstr \"Autonyme\"" + Environment.NewLine +
"" + Environment.NewLine +
"#. Button text for Cancel/Close button" + Environment.NewLine +
"#. /Src/Common/FieldWorks/ShareProjectsFolderDlg.resx::m_btnClose.Text" + Environment.NewLine +
"#. /Src/FwCoreDlgs/FwCoreDlgs.resx::kstidClose" + Environment.NewLine +
"msgid \"Close\"" + Environment.NewLine +
"msgstr \"Fermer\"" + Environment.NewLine +
"" + Environment.NewLine +
"# JDX (was \"Date de Création\")" + Environment.NewLine +
"#. field name for Data Notebook records" + Environment.NewLine +
"#. /Language Explorer/Configuration/Lexicon/browseDialogColumns.xml::/doc/browseColumns/column/@label" + Environment.NewLine +
"#. /Src/FwResources/FwStrings.resx::kstidDateCreated" + Environment.NewLine +
"msgid \"Date Created\"" + Environment.NewLine +
"msgstr \"Créé le\"" + Environment.NewLine +
"" + Environment.NewLine +
"# JDX (was \"Date de Modification\")" + Environment.NewLine +
"#. field name for Data Notebook records" + Environment.NewLine +
"#. /Language Explorer/Configuration/Lexicon/browseDialogColumns.xml::/doc/browseColumns/column/@label" + Environment.NewLine +
"#. /Src/FwResources/FwStrings.resx::kstidDateModified" + Environment.NewLine +
"msgid \"Date Modified\"" + Environment.NewLine +
"msgstr \"Modifié le\"" + Environment.NewLine +
"" + Environment.NewLine +
"#. /Src/FwCoreDlgs/AddCnvtrDlg.resx::$this.Text" + Environment.NewLine +
"msgid \"Encoding Converters\"" + Environment.NewLine +
"msgstr \"Convertisseurs d'encodage\"" + Environment.NewLine +
"" + Environment.NewLine +
"#. /|strings-en.xml::/ClassNames/LexEntry|" + Environment.NewLine +
"#. /Language Explorer/Configuration/Main.xml::/window/contextMenus/menu/menu/item/@label" + Environment.NewLine +
"#. /Src/LexText/LexTextControls/LexTextControls.resx::ksEntry" + Environment.NewLine +
"msgid \"Entry\"" + Environment.NewLine +
"msgstr \"Entrée\"" + Environment.NewLine +
"" + Environment.NewLine +
"#. /|strings-en.xml::/AlternativeTitles/PhEnvironment-Plural|" + Environment.NewLine +
"#. /Language Explorer/Configuration/Grammar/Edit/toolConfiguration.xml::/root/tools/tool/@label" + Environment.NewLine +
"msgid \"Environments\"" + Environment.NewLine +
"msgstr \"Environnements\"" + Environment.NewLine +
"" + Environment.NewLine +
"#. /Language Explorer/Configuration/Lexicon/browseDialogColumns.xml::/doc/browseColumns/column/@headerlabel" + Environment.NewLine +
"#. /Language Explorer/Configuration/Parts/LexSenseParts.xml::/PartInventory/bin/part[@id=\"LexSense-Detail-MsaCombo\"]/slice/@label" + Environment.NewLine +
"#. /Src/LexText/LexTextControls/MSAGroupBox.resx::m_groupBox.Text" + Environment.NewLine +
"msgid \"Grammatical Info.\"" + Environment.NewLine +
"msgstr \"Info. grammaticale\"" + Environment.NewLine +
"" + Environment.NewLine +
"# JDX" + Environment.NewLine +
"#. /Language Explorer/Configuration/Parts/LexEntryParts.xml::/PartInventory/bin/part[@id=\"LexEntry-Jt-GrammaticalFunctionsSummary\"]/para/lit" + Environment.NewLine +
"msgid \"Grammatical Info. Details\"" + Environment.NewLine +
"msgstr \"Détails d'info. grammaticale\"" + Environment.NewLine +
"" + Environment.NewLine +
"#. /Language Explorer/Configuration/Lexicon/browseDialogColumns.xml::/doc/browseColumns/column/@label" + Environment.NewLine +
"msgid \"Headword\"" + Environment.NewLine +
"msgstr \"Entrée de dictionnaire\"" + Environment.NewLine +
"" + Environment.NewLine +
"#. This is the text for the Help menu on the main menu bar." + Environment.NewLine +
"#. This is the help category for toolbar/menu items. This string is used in the dialog that allows users to customize their toolbars." + Environment.NewLine +
"#. /Src/UnicodeCharEditor/CharEditorWindow.resx::m_btnHelp.Text" + Environment.NewLine +
"#. /Src/xWorks/ExportDialog.resx::buttonHelp.Text" + Environment.NewLine +
"msgid \"Help\"" + Environment.NewLine +
"msgstr \"Aide\"" + Environment.NewLine +
"" + Environment.NewLine +
"#  This message does not actually appear anywhere, but is here for unit testing!" + Environment.NewLine +
"msgid \"Junk Test Message\"" + Environment.NewLine +
"msgstr \"\"" + Environment.NewLine +
"" + Environment.NewLine +
"#. /Language Explorer/Configuration/Lexicon/browseDialogColumns.xml::/doc/browseColumns/column/@label" + Environment.NewLine +
"msgid \"Lexeme Form\"" + Environment.NewLine +
"msgstr \"Forme de lexème\"" + Environment.NewLine +
"" + Environment.NewLine +
"# @" + Environment.NewLine +
"#. /Language Explorer/Configuration/Lexicon/browseDialogColumns.xml::/doc/browseColumns/column/@label" + Environment.NewLine +
"msgid \"Morph Type\"" + Environment.NewLine +
"msgstr \"Type de morph\"" + Environment.NewLine +
"" + Environment.NewLine +
"#. Used in CustomListDlg - Display items by combobox" + Environment.NewLine +
"#. /Language Explorer/Configuration/Grammar/areaConfiguration.xml::/root/controls/parameters/guicontrol/parameters/columns/column/@label" + Environment.NewLine +
"#. /Src/LexText/LexTextControls/LexOptionsDlg.resx::m_chName.Text" + Environment.NewLine +
"#. /Src/LexText/LexTextControls/LexTextControls.resx::ksName" + Environment.NewLine +
"msgid \"Name\"" + Environment.NewLine +
"msgstr \"Nom\"" + Environment.NewLine +
"" + Environment.NewLine +
"# JDX" + Environment.NewLine +
"#. /|strings-en.xml::/EmptyTitles/No-RnGenericRecs|" + Environment.NewLine +
"#. /|strings-en.xml::/Misc/No Records|" + Environment.NewLine +
"#. /Src/LexText/Morphology/MEStrings.resx::ksNoRecords" + Environment.NewLine +
"msgid \"No Records\"" + Environment.NewLine +
"msgstr \"Aucun enregistrement\"" + Environment.NewLine +
"" + Environment.NewLine +
"# JDX" + Environment.NewLine +
"#. {1} will be a long string...don't leave it out." + Environment.NewLine +
"#. /Src/Utilities/Reporting/ReportingStrings.resx::kstidPleaseEmailThisTo0WithASuitableSubject" + Environment.NewLine +
"msgid \"\"" + Environment.NewLine +
"\"Please email this report to {0} with a suitable subject:\\n\"" + Environment.NewLine +
"\"\\n\"" + Environment.NewLine +
"\"{1}\"" + Environment.NewLine +
"msgstr \"\"" + Environment.NewLine +
"\"Veuillez envoyer ce rapport à {0} avec un sujet approprié:\\n\"" + Environment.NewLine +
"\"\\n\"" + Environment.NewLine +
"\"{1}\"" + Environment.NewLine +
"" + Environment.NewLine +
"#. Tooltip for the properties menu item" + Environment.NewLine +
"#. /Src/FwCoreDlgs/AddCnvtrDlg.resx::propertiesTab.Text" + Environment.NewLine +
"msgid \"Properties\"" + Environment.NewLine +
"msgstr \"Propriétés\"" + Environment.NewLine +
"" + Environment.NewLine +
"# JDX" + Environment.NewLine +
"#. /|strings-en.xml::/AlternativeTitles/PubSettings|" + Environment.NewLine +
"msgid \"Publication Settings\"" + Environment.NewLine +
"msgstr \"Configuration de publication\"" + Environment.NewLine +
"" + Environment.NewLine +
"# JDX:JN" + Environment.NewLine +
"#. /Language Explorer/Configuration/Lexicon/Dictionary/toolConfiguration.xml::/root/reusableControls/control/parameters/configureLayouts/layoutType/@label" + Environment.NewLine +
"msgid \"Root-based (complex forms as subentries)\"" + Environment.NewLine +
"msgstr \"Basé sur les radicals (formes complexes comme sous-entrées)\"" + Environment.NewLine +
"" + Environment.NewLine +
"#. /|strings-en.xml::/AlternativeTitles/SemanticDomain-Plural|" + Environment.NewLine +
"#. /Language Explorer/Configuration/Lexicon/browseDialogColumns.xml::/doc/browseColumns/column/@label" + Environment.NewLine +
"msgid \"Semantic Domains\"" + Environment.NewLine +
"msgstr \"Domaines sémantiques\"" + Environment.NewLine +
"" + Environment.NewLine +
"#. /|strings-en.xml::/ClassNames/LexSense|" + Environment.NewLine +
"#. /Language Explorer/Configuration/Main.xml::/window/contextMenus/menu/menu/item/@label" + Environment.NewLine +
"#. /Src/LexText/LexTextControls/LexTextControls.resx::ksSense" + Environment.NewLine +
"#. /Src/xWorks/xWorksStrings.resx::Sense" + Environment.NewLine +
"msgid \"Sense\"" + Environment.NewLine +
"msgstr \"Sens\"" + Environment.NewLine +
"" + Environment.NewLine +
"#. /|strings-en.xml::/AlternativeTitles/LexSense-Plural|" + Environment.NewLine +
"#. /Language Explorer/Configuration/Parts/ReversalParts.xml::/PartInventory/bin/part[@id=\"ReversalIndexEntry-Detail-CurrentSenses\"]/slice/@label" + Environment.NewLine +
"#. /Src/LexText/Lexicon/LexEdStrings.resx::ksSenses" + Environment.NewLine +
"msgid \"Senses\"" + Environment.NewLine +
"msgstr \"Les sens\"" + Environment.NewLine +
"" + Environment.NewLine +
"# JDX" + Environment.NewLine +
"#. /Language Explorer/Configuration/Lexicon/browseDialogColumns.xml::/doc/browseColumns/column/@label" + Environment.NewLine +
"#. /Src/Common/Controls/XMLViews/XMLViewsStrings.resx::ksShowAsHeadwordIn" + Environment.NewLine +
"msgid \"Show As Headword In\"" + Environment.NewLine +
"msgstr \"Afficher comme entrée de dictionnaire dans\"" + Environment.NewLine +
"" + Environment.NewLine +
"#. /|strings-en.xml::/Linguistics/Morphology/Adjacency/Somewhere to right|" + Environment.NewLine +
"msgid \"somewhere after\"" + Environment.NewLine +
"msgstr \"quelque part après\"" + Environment.NewLine +
"" + Environment.NewLine +
"#. /|strings-en.xml::/Linguistics/Morphology/Adjacency/Somewhere to left|" + Environment.NewLine +
"msgid \"somewhere before\"" + Environment.NewLine +
"msgstr \"quelque part avant\"" + Environment.NewLine +
"" + Environment.NewLine +
"# JDX:JN" + Environment.NewLine +
"#. /|strings-en.xml::/Linguistics/Morphology/TemplateTable/Stem|" + Environment.NewLine +
"#. /Language Explorer/Configuration/Parts/MorphologyParts.xml::/PartInventory/bin/part[@id=\"MoInflAffixTemplate-Jt-TemplateTabley\"]/table/if/row/cell/para/lit" + Environment.NewLine +
"msgid \"STEM\"" + Environment.NewLine +
"msgstr \"BASE\"" + Environment.NewLine +
"" + Environment.NewLine +
"#. /Language Explorer/Configuration/Parts/MorphologyParts.xml::/PartInventory/bin/part[@id=\"PhEnvironment-Detail-StringRepresentation\"]/slice/@label" + Environment.NewLine +
"msgid \"String Representation\"" + Environment.NewLine +
"msgstr \"Représentation de chaîne\"" + Environment.NewLine +
"" + Environment.NewLine +
"#  cfr contexte" + Environment.NewLine +
"#. /Language Explorer/Configuration/Lexicon/browseDialogColumns.xml::/doc/browseColumns/column/@label" + Environment.NewLine +
"msgid \"Summary Definition\"" + Environment.NewLine +
"msgstr \"Résumé de la définition\"" + Environment.NewLine +
"" + Environment.NewLine +
"# JDX" + Environment.NewLine +
"#. /|strings-en.xml::/Linguistics/Morphology/TemplateTable/SlotChooserInstructionalText|" + Environment.NewLine +
"msgid \"The following slots are available for the category {0}.\"" + Environment.NewLine +
"msgstr \"Les cases suivantes sont disponibles pour la categorie {0}.\"" + Environment.NewLine +
"" + Environment.NewLine +
"# JDX:JN" + Environment.NewLine +
"#. /|strings-en.xml::/DialogStrings/ChangeMorphTypeLoseStemNameGramInfo|" + Environment.NewLine +
"msgid \"The stem name and some grammatical information will be lost! Do you still want to continue?\"" + Environment.NewLine +
"msgstr \"Le nom de base et certaine information grammaticale seront perdus! Voulez-vous continuer?\"" + Environment.NewLine +
"" + Environment.NewLine +
"# JDX" + Environment.NewLine +
"#. /Language Explorer/Configuration/Lexicon/browseDialogColumns.xml::/doc/browseColumns/column/@label" + Environment.NewLine +
"msgid \"Variants\"" + Environment.NewLine +
"msgstr \"Variantes\"" + Environment.NewLine +
"" + Environment.NewLine +
"#. Used in the diffTool window when there is a writing system difference in the text" + Environment.NewLine +
"#. /Src/TE/DiffView/TeDiffViewResources.resx::kstidWritingSystemDiff" + Environment.NewLine +
"msgid \"Writing System Difference\"" + Environment.NewLine +
"msgstr \"\"" + Environment.NewLine +
"" + Environment.NewLine +
"#. /Src/TE/TeDialogs/FilesOverwriteDialog.resx::btnYesToAll.Text" + Environment.NewLine +
"msgid \"Yes to &All\"" + Environment.NewLine +
"msgstr \"Oui a tous\"" + Environment.NewLine +
"" + Environment.NewLine +
"#. {0} is a vernacular string. Sometimes it might not render well in the message box font. It's remotely possible it might be empty." + Environment.NewLine +
"#. /Src/FDO/Strings.resx::ksWordformUsedByChkRef" + Environment.NewLine +
"msgid \"You cannot delete this wordform because it is used as an occurrence of a biblical term ({0}) in Translation Editor.\"" + Environment.NewLine +
"msgstr \"\"" + Environment.NewLine +
"" + Environment.NewLine +
"# JDX" + Environment.NewLine +
"#. This text will be displayed if a user tries to exit the diff dialog before all the differences have been taken care of." + Environment.NewLine +
"#. /Src/TE/TeResources/TeStrings.resx::kstidExitDiffMsg" + Environment.NewLine +
"msgid \"You still have {0} difference(s) left.  Are you sure you want to exit?\"" + Environment.NewLine +
"msgstr \"Il reste {0} différences. Êtes-vous sûr de vouloir quitter?\"" + Environment.NewLine +
"" + Environment.NewLine +
"# JDX" + Environment.NewLine +
"#~ msgid \"Check for _Updates...\"" + Environment.NewLine +
"#~ msgstr \"Rechercher les mises à jo_ur...\"" + Environment.NewLine +
"# JDX" + Environment.NewLine +
"#~ msgid \"Get Lexicon and _Merge with this Project...\"" + Environment.NewLine +
"#~ msgstr \"Obtenir un lexique et le fusionner avec ce projet...\"" + Environment.NewLine +
"# JDX" + Environment.NewLine +
"#, fuzzy" + Environment.NewLine +
"#~ msgid \"Send this Lexicon for the first time...\"" + Environment.NewLine +
"#~ msgstr \"Envoyer ce lexique pour la première fois...\"" + Environment.NewLine +
"" + Environment.NewLine;
#endregion

		[Test]
		public void TestReadPoData()
		{
			POString posHeader;
			POString posObsolete;
			var srIn = new StringReader(_sFrenchPoData);
			var dictFrenchPo = Program.ReadPoFile(srIn, out posHeader, out posObsolete);
			Assert.IsNotNull(posHeader);
			Assert.IsNotNull(posObsolete);
			Assert.IsNull(posObsolete.AutoComments);
			Assert.IsNull(posObsolete.Flags);
			Assert.IsNull(posObsolete.Reference);
			Assert.IsNull(posObsolete.MsgId);
			Assert.IsNull(posObsolete.MsgStr);
			Assert.IsNotNull(posObsolete.UserComments);
			Assert.AreEqual(10, posObsolete.UserComments.Count);
			Assert.AreEqual(" JDX", posObsolete.UserComments[0]);
			Assert.AreEqual("~ msgid \"Check for _Updates...\"", posObsolete.UserComments[1]);
			Assert.AreEqual(", fuzzy", posObsolete.UserComments[7]);
			Assert.AreEqual(49, dictFrenchPo.Count);
			var rgsPoStrings = dictFrenchPo.ToList();
			var postr0 = rgsPoStrings[0].Value;
			Assert.IsNotNull(postr0, "French po string[0] has data");
			Assert.IsNotNull(postr0.MsgId, "French po string[0] has MsgId data");
			Assert.AreEqual(1, postr0.MsgId.Count, "French po string[0] has one line of MsgId data");
			Assert.AreEqual(" - ", postr0.MsgId[0], "French po string[0] has the expected MsgId data");
			Assert.AreEqual(" - ", postr0.MsgIdAsString(), "French po string[0] is ' - '");
			Assert.AreEqual(1, postr0.MsgStr.Count, "French po string[0] has one line of MsgStr data");
			Assert.AreEqual(" - ", postr0.MsgStr[0], "French po string[0] MsgStr is ' - '");
			Assert.IsNull(postr0.UserComments, "French po string[0] has no User Comments (as expected)");
			Assert.IsNull(postr0.Reference, "French po string[0] has no Reference data (as expected)");
			Assert.IsNull(postr0.Flags, "French po string[0] has no Flags data (as expected)");
			Assert.IsNotNull(postr0.AutoComments, "French po string[0] has Auto Comments");
			Assert.AreEqual(3, postr0.AutoComments.Count, "French po string[0] has three lines of Auto Comments");
			Assert.AreEqual("separate name and abbreviation (space dash space)", postr0.AutoComments[0], "French po string[0] has the expected first line of Auto Comment");

			var postr5 = rgsPoStrings[5].Value;
			Assert.IsNotNull(postr5, "French po string[5] has data");
			Assert.IsNotNull(postr5.MsgId, "French po string[5] has MsgId data");
			Assert.AreEqual(1, postr5.MsgId.Count, "French po string[5] has one line of MsgId data");
			Assert.AreEqual("Academic Domain", postr5.MsgId[0], "French po string[5] has the expected MsgId data");
			Assert.AreEqual("Academic Domain", postr5.MsgIdAsString(), "French po string[5] is 'Academic Domain'");
			Assert.AreEqual(1, postr5.MsgStr.Count, "French po string[5] has one line of MsgStr data");
			Assert.AreEqual("Domaine technique", postr5.MsgStr[0], "French po string[5] has the expected MsgStr data");
			Assert.IsNotNull(postr5.UserComments, "French po string[5] has User Comments");
			Assert.AreEqual(1, postr5.UserComments.Count, "French po string[5] has one line of User Comments");
			Assert.AreEqual("JDX:JN", postr5.UserComments[0], "French po string[5] has the expected User Comment");
			Assert.IsNull(postr5.Reference, "French po string[5] has no Reference data (as expected)");
			Assert.IsNull(postr5.Flags, "French po string[5] has no Flags data (as expected)");
			Assert.IsNotNull(postr5.AutoComments, "French po string[5] has Auto Comments");
			Assert.AreEqual(1, postr5.AutoComments.Count, "French po string[5] has one line of Auto Comments");
			Assert.AreEqual("/|strings-en.xml::/PossibilityListItemTypeNames/DomainTypes|", postr5.AutoComments[0], "French po string[5] has the expected Auto Comment");

			var postr48 = rgsPoStrings[48].Value;
			Assert.IsNotNull(postr48, "French po string[48] has data");
			Assert.IsNotNull(postr48.MsgId, "French po string[48] has MsgId data");
			Assert.AreEqual(1, postr48.MsgId.Count, "French po string[48] has one line of MsgId data");
			Assert.AreEqual("You still have {0} difference(s) left.  Are you sure you want to exit?", postr48.MsgId[0], "French po string[48] has the expected MsgId data");
			Assert.AreEqual("You still have {0} difference(s) left.  Are you sure you want to exit?", postr48.MsgIdAsString(),
				"French po string[48] is 'You still have {0} difference(s) left.  Are you sure you want to exit?'");
			Assert.AreEqual(1, postr48.MsgStr.Count, "French po string[48] has one line of MsgStr data");
			Assert.AreEqual("Il reste {0} différences. Êtes-vous sûr de vouloir quitter?", postr48.MsgStr[0], "French po string[48] has the expected MsgStr data");
			Assert.IsNotNull(postr48.UserComments, "French po string[48] has User Comments");
			Assert.AreEqual(1, postr48.UserComments.Count, "French po string[48] has one line of User Comments");
			Assert.AreEqual("JDX", postr48.UserComments[0], "French po string[48] has the expected User Comment");
			Assert.IsNull(postr48.Reference, "French po string[48] has no Reference data (as expected)");
			Assert.IsNull(postr48.Flags, "French po string[48] has no Flags data (as expected)");
			Assert.IsNotNull(postr48.AutoComments, "French po string[48] has Auto Comments");
			Assert.AreEqual(2, postr48.AutoComments.Count, "French po string[48] has two lines of Auto Comments");
			Assert.AreEqual("This text will be displayed if a user tries to exit the diff dialog before all the differences have been taken care of.",
				postr48.AutoComments[0], "French po string[48] has the expected first line of Auto Comment");
			Assert.AreEqual("/Src/TE/TeResources/TeStrings.resx::kstidExitDiffMsg",
				postr48.AutoComments[1], "French po string[48] has the expected second line of Auto Comment");
		}

#region TestData
		static private readonly string _sFrenchPoData2 =
"#  Copyright (c) 2005-2015 SIL International" + Environment.NewLine +
"#  This software is licensed under the LGPL, version 2.1 or later" + Environment.NewLine +
"#  (http://www.gnu.org/licenses/lgpl-2.1.html)" + Environment.NewLine +
"msgid \"\"" + Environment.NewLine +
"msgstr \"\"" + Environment.NewLine +
"\"Project-Id-Version: FieldWorks 8.1.3\\n\"" + Environment.NewLine +
"\"Report-Msgid-Bugs-To: FlexErrors@sil.org\\n\"" + Environment.NewLine +
"\"POT-Creation-Date: 2015-02-04T10:52:27.7123661-06:00\\n\"" + Environment.NewLine +
"\"PO-Revision-Date: 2015-02-04 18:31-0800\\n\"" + Environment.NewLine +
"\"Last-Translator: David Doe <david_rowe@sil.org>\\n\"" + Environment.NewLine +
"\"Language-Team: French <David_Doe@nowhere.org>\\n\"" + Environment.NewLine +
"\"MIME-Version: 1.0\\n\"" + Environment.NewLine +
"\"Content-Type: text/plain; charset=UTF-8\\n\"" + Environment.NewLine +
"\"Content-Transfer-Encoding: 8bit\\n\"" + Environment.NewLine +
"\"X-Poedit-Language: French\\n\"" + Environment.NewLine +
"" + Environment.NewLine +
"#. Marker for missing items." + Environment.NewLine +
"#. /Src/LexText/Discourse/DiscourseStrings.resx::ksMissingMarker" + Environment.NewLine +
"msgid \"---\"" + Environment.NewLine +
"msgstr \"---\"" + Environment.NewLine +
"" + Environment.NewLine +
"#. Label for possible class to add custom fields to" + Environment.NewLine +
"#. /Language Explorer/Configuration/Parts/MorphologyParts.xml::/PartInventory/bin/part[@id=\"MoAlloAdhocProhib-Jt-Type\"]/if/lit" + Environment.NewLine +
"#. /Src/xWorks/xWorksStrings.resx::Allomorph" + Environment.NewLine +
"msgid \"Allomorph\"" + Environment.NewLine +
"msgstr \"\"" + Environment.NewLine +
"" + Environment.NewLine +
"#. /|strings-en.xml::/AlternativeTitles/MoForm-Plural|" + Environment.NewLine +
"#. /Language Explorer/Configuration/Lexicon/browseDialogColumns.xml::/doc/browseColumns/column/@label" + Environment.NewLine +
"msgid \"Allomorphs\"" + Environment.NewLine +
"msgstr \"\"" + Environment.NewLine +
"" + Environment.NewLine +
"# [?? in the English string show is more commonly used than view] DDZ" + Environment.NewLine +
"#. Menu item" + Environment.NewLine +
"#. /Src/TE/TeResources/TeTMStrings.resx::kstidCorrectMisspelledWords" + Environment.NewLine +
"msgid \"View Incorrect Words in Use...\"" + Environment.NewLine +
"msgstr \"Afficher les mots incorrects en\"" + Environment.NewLine +
"" + Environment.NewLine +
"# DDZ \"in Use\" Does this mean \"used\" or \"in their context\"?" + Environment.NewLine +
"#. Tooltip (currently unused)" + Environment.NewLine +
"#. /Src/TE/TeResources/TeTMStrings.resx::kstidCorrectMisspelledWordsToolTip" + Environment.NewLine +
"msgid \"View incorrectly spelled words in use\"" + Environment.NewLine +
"msgstr \"\"" + Environment.NewLine +
"" + Environment.NewLine +
"#. This text will appear as a tab in the task bar in the Notes window." + Environment.NewLine +
"#. /Src/TE/TeResources/TeStrings.resx::kstidViews" + Environment.NewLine +
"msgid \"Views\"" + Environment.NewLine +
"msgstr \"Affichages\"" + Environment.NewLine +
"" + Environment.NewLine +
"# DDZ" + Environment.NewLine +
"#. /Src/TE/TeImportExport/ImportWizard.resx::label30.Text" + Environment.NewLine +
"msgid \"What kind of data do you want to import?\"" + Environment.NewLine +
"msgstr \"Quel type de données voulez-vous importer?\"" + Environment.NewLine +
"" + Environment.NewLine +
"# DDZ" + Environment.NewLine +
"#. /Src/TE/TeImportExport/ImportDialog.resx::grpImportWhat.Text" + Environment.NewLine +
"msgid \"What to Import\"" + Environment.NewLine +
"msgstr \"Quoi importer\"" + Environment.NewLine +
"" + Environment.NewLine +
"#. /Src/TE/TeImportExport/ScrImportComponents.resx::kstidSFFileBookChooserQuestion" + Environment.NewLine +
"msgid \"Which books are contained in {0}?\"" + Environment.NewLine +
"msgstr \"Quels livres sont contenus dans {0}?\"" + Environment.NewLine +
"" + Environment.NewLine +
"# DDZ" + Environment.NewLine +
"#. Mixed Capitalization check message" + Environment.NewLine +
"#. /Src/FDO/DomainServices/ScrFdoResources.resx::Word_has_mixed_capitalization" + Environment.NewLine +
"msgid \"Word has mixed capitalization\"" + Environment.NewLine +
"msgstr \"Mot a un mélange de casse\"" + Environment.NewLine +
"" + Environment.NewLine +
"#. /Src/TE/TeDialogs/BookPropertiesDialog.resx::dataGridViewColumn1.HeaderText" + Environment.NewLine +
"msgid \"Writing System \"" + Environment.NewLine +
"msgstr \"Système d'écriture \"" + Environment.NewLine +
"" + Environment.NewLine +
"# DDZ" + Environment.NewLine +
"#. Occurs if the writing system is unknown during an OXES import." + Environment.NewLine +
"#. /Src/TE/TeResources/TeStrings.resx::kstidUndefinedWritingSystemDetails" + Environment.NewLine +
"msgid \"\"" + Environment.NewLine +
"\"Writing system code: {0}\\n\"" + Environment.NewLine +
"\"Required writing system file: {2}\\n\"" + Environment.NewLine +
"\"Writing system folder:\\n\"" + Environment.NewLine +
"\"{1}\\n\"" + Environment.NewLine +
"\"\\t\"" + Environment.NewLine +
"msgstr \"\"" + Environment.NewLine +
"\"Code du système d'écriture: {0}\\n\"" + Environment.NewLine +
"\"Fichier exigé par le système d'écriture: {2}\\n\"" + Environment.NewLine +
"\"Dossier du système d'écriture: \\n\"" + Environment.NewLine +
"\"{1}\\n\"" + Environment.NewLine +
"\"\\t\"" + Environment.NewLine +
"" + Environment.NewLine +
"# DDZ" + Environment.NewLine +
"#. Used in the diffTool window when there is a writing system difference in the text" + Environment.NewLine +
"#. /Src/TE/DiffView/TeDiffViewResources.resx::kstidWritingSystemDiff" + Environment.NewLine +
"msgid \"Writing System Difference\"" + Environment.NewLine +
"msgstr \"Différence de système d'écriture\"" + Environment.NewLine +
"" + Environment.NewLine +
"# DDZ" + Environment.NewLine +
"#. Used in the diffTool window when there are multiple writing system differences in the text" + Environment.NewLine +
"#. /Src/TE/DiffView/TeDiffViewResources.resx::kstidMultipleWritingSystemDiffs" + Environment.NewLine +
"msgid \"Writing System Differences\"" + Environment.NewLine +
"msgstr \"Différences de système d'écriture\"" + Environment.NewLine +
"" + Environment.NewLine +
"#. /Src/Common/FwPrintLayoutComponents/PageSetupDlg.resx::cboPubPageSize.Items1" + Environment.NewLine +
"msgid \"5.25 x 8.25 in\"" + Environment.NewLine +
"msgstr \"5.25 x 8.25 po\"" + Environment.NewLine +
"" + Environment.NewLine +
"#. /Src/Common/FwPrintLayoutComponents/PageSetupDlg.resx::cboPubPageSize.Items2" + Environment.NewLine +
"msgid \"5.8 x 8.7 in\"" + Environment.NewLine +
"msgstr \"5.8 x 8.7 po\"" + Environment.NewLine +
"" + Environment.NewLine +
"#. Text for the export XHTML menu item" + Environment.NewLine +
"#. /Src/TE/TeResources/TeTMStrings.resx::kstidExportXhtmlItemName" + Environment.NewLine +
"msgid \"X&HTML...\"" + Environment.NewLine +
"msgstr \"X&HTML...\"" + Environment.NewLine +
"" + Environment.NewLine +
"#. /Src/TE/TeDialogs/FilesOverwriteDialog.resx::btnYesToAll.Text" + Environment.NewLine +
"msgid \"Yes to &All\"" + Environment.NewLine +
"msgstr \"Oui à &tous\"" + Environment.NewLine +
"" + Environment.NewLine +
"# DDZ" + Environment.NewLine +
"#. /Src/FwCoreDlgs/FwCoreDlgControls/FwCoreDlgControls.resx::kstidMissingScrAbbr" + Environment.NewLine +
"msgid \"You must have a script abbreviation if you specify a script name.\"" + Environment.NewLine +
"msgstr \"Il faut une abréviation d'écriture si vous spécifiez un nom d'écriture.\"" + Environment.NewLine +
"" + Environment.NewLine +
"#. This text will be displayed if a user tries to exit the diff dialog before all the differences have been taken care of." + Environment.NewLine +
"#. /Src/TE/TeResources/TeStrings.resx::kstidExitDiffMsg" + Environment.NewLine +
"msgid \"You still have {0} difference(s) left.  Are you sure you want to exit?\"" + Environment.NewLine +
"msgstr \"Il reste {0} différences. Êtes-vous sûr de vouloir quitter?\"" + Environment.NewLine +
"" + Environment.NewLine +
"#. UI Book name for scripture book 38" + Environment.NewLine +
"#. /Src/FDO/DomainServices/ScrFdoResources.resx::kstidBookName38" + Environment.NewLine +
"msgid \"Zechariah\"" + Environment.NewLine +
"msgstr \"Zacharie\"" + Environment.NewLine +
"" + Environment.NewLine;
#endregion

		[Test]
		public void TestMergePoFiles()
		{
			POString posHeader;
			POString posObsolete;
			var srMain = new StringReader(_sFrenchPoData);
			var srNew = new StringReader(_sFrenchPoData2);
			var swLog = new StringWriter();
			var dictMerged = Program.MergePoFileData(swLog, srMain, srNew, "Test.fr.po", "Test2.fr.po", out posHeader, out posObsolete);
			Assert.IsNotNull(posHeader, "Merged po files should still have a header!");
			Assert.IsNotNull(posObsolete, "Merged po files should have the obsolete data from the first");
			// first "file" has 49 messages, second "file" has 20 messages but 4 overlap with first file and 1 new string is untranslated.  48 + 20 - 5 = 63.
			Assert.AreEqual(64, dictMerged.Count, "Merged po files should have 64 messages");
			POString posTest;
			// test word that is translated in the first file, but left untranslated in the second file
			var exists = dictMerged.TryGetValue("Allomorph", out posTest);
			Assert.IsTrue(exists, "'Allomorph' exists in the merged po file data");
			Assert.IsNotNull(posTest, "'Allomorph' exists in the merged po file data (posTest not null)");
			Assert.IsNotNull(posTest.MsgStr);
			Assert.AreEqual(1, posTest.MsgStr.Count);
			Assert.AreEqual("Allomorphe", posTest.MsgStr[0]);
			// test word that is translated in the second file, but left untranslated in the second file
			exists = dictMerged.TryGetValue("Writing System Difference", out posTest);
			Assert.IsTrue(exists, "'Writing System Difference' exists in the merged po file data");
			Assert.IsNotNull(posTest, "'Writing System Difference' exists in the merged po file data (posTest not null)");
			Assert.IsNotNull(posTest.MsgStr);
			Assert.AreEqual(1, posTest.MsgStr.Count);
			Assert.AreEqual("Différence de système d'écriture", posTest.MsgStr[0]);
			// test word that is present in the second file, but missing from the first file
			exists = dictMerged.TryGetValue("Zechariah", out posTest);
			Assert.IsTrue(exists, "'Zechariah' exists in the merged po file data");
			Assert.IsNotNull(posTest, "'Zechariah' exists in the merged po file data (posTest not null)");
			Assert.IsNotNull(posTest.MsgStr);
			Assert.AreEqual(1, posTest.MsgStr.Count);
			Assert.AreEqual("Zacharie", posTest.MsgStr[0]);
			// test word that is present in the first file, but missing from the second file
			exists = dictMerged.TryGetValue("Senses", out posTest);
			Assert.IsTrue(exists, "'Senses' exists in the merged po file data");
			Assert.IsNotNull(posTest, "'Senses' exists in the merged po file data (posTest not null)");
			Assert.IsNotNull(posTest.MsgStr);
			Assert.AreEqual(1, posTest.MsgStr.Count);
			Assert.AreEqual("Les sens", posTest.MsgStr[0]);
			// test word that is present in both files, and translated the same in both
			exists = dictMerged.TryGetValue("You still have {0} difference(s) left.  Are you sure you want to exit?", out posTest);
			Assert.IsTrue(exists, "'You still have {0} difference(s) left.  Are you sure you want to exit?' exists in the merged po file data");
			Assert.IsNotNull(posTest, "'You still have {0} difference(s) left.  Are you sure you want to exit?' exists in the merged po file data (posTest not null)");
			Assert.IsNotNull(posTest.MsgStr);
			Assert.AreEqual(1, posTest.MsgStr.Count);
			Assert.AreEqual("Il reste {0} différences. Êtes-vous sûr de vouloir quitter?", posTest.MsgStr[0]);
			Assert.IsNotNull(posTest.UserComments);			// Test that the result comes from the first file when the translation is the same.
			Assert.AreEqual(1, posTest.UserComments.Count);
			// test word that is present in both files, but translated differently
			exists = dictMerged.TryGetValue("Yes to &All", out posTest);
			Assert.IsTrue(exists, "'Yes to &All' exists in the merged po file data");
			Assert.IsNotNull(posTest, "'Yes to &All' exists in the merged po file data (posTest not null)");
			Assert.IsNotNull(posTest.MsgStr);
			Assert.AreEqual(1, posTest.MsgStr.Count);
			Assert.AreEqual("Oui à &tous", posTest.MsgStr[0]);	// translation comes from the second file
		}

		[Test]
		public void TestUpdatePoFile()
		{
			var rgsPoStrings = new List<POString>();
			var xdoc = new XmlDocument();
			xdoc.LoadXml(_sResxData);
			Assert.IsNotNull(xdoc.DocumentElement);
			Program.ProcessResxData(xdoc.DocumentElement, @"/Src/FwCoreDlgs/AddCnvtrDlg.resx", rgsPoStrings);
			xdoc.LoadXml(_sConfigData);
			Assert.IsNotNull(xdoc.DocumentElement);
			Program.ProcessConfigElement(xdoc.DocumentElement, "/Language Explorer/Configuration/Parts/LexEntry.fwlayout", rgsPoStrings);
			xdoc.LoadXml(_sStringsEnData);
			Assert.IsNotNull(xdoc.DocumentElement);
			Program.ProcessStringsElement(xdoc.DocumentElement, rgsPoStrings);
			xdoc.LoadXml(_sDictConfigData);
			Assert.IsNotNull(xdoc.DocumentElement);
			Program.ProcessFwDictConfigElement(xdoc.DocumentElement, "/Language Explorer/DefaultConfigurations/Dictionary/Root.fwdictconfig", rgsPoStrings);
			rgsPoStrings.Sort(POString.CompareMsgIds);
			POString.MergeDuplicateStrings(rgsPoStrings);
			var sw = new StringWriter();
			Program.WritePotFile(sw, "/home/testing/fw", rgsPoStrings);
			var potFileStr = sw.ToString();
			Assert.IsNotNull(potFileStr);
			var srPot = new StringReader(potFileStr);
			var srFrenchPo = new StringReader(_sFrenchPoData);
			POString posHeader, posObsolete;
			var dictFrenchPo = Program.ReadPoFile(srFrenchPo, out posHeader, out posObsolete);
			Assert.AreEqual(49, dictFrenchPo.Count);
			POString posNewHeader;
			var dictPot = Program.ReadPotFile(srPot, out posNewHeader);
			Assert.AreEqual(56, dictPot.Count);
			var swOut = new StringWriter();
			Program.WriteUpdatedPoFile(swOut, dictFrenchPo, dictPot, posHeader, posObsolete, posNewHeader);
			// SUT (everything before is tested already in another test)
			Assert.IsNotNull(swOut);
			var srIn = new StringReader(swOut.ToString());
			POString posUpdatedHeader, posUpdatedObsolete;
			var dictUpdatedPo = Program.ReadPoFile(srIn, out posUpdatedHeader, out posUpdatedObsolete);
			// SUT
			Assert.AreEqual(61, dictUpdatedPo.Count);
			POString posTest;

			// test message that is translated in the French po data and is found in the pot data
			var exists = dictUpdatedPo.TryGetValue("Allomorph", out posTest);
			Assert.IsTrue(exists, "'Allomorph' exists in the updated po file data");
			Assert.IsNotNull(posTest, "'Allomorph' exists in the updated po file data (posTest not null)");
			Assert.IsNotNull(posTest.MsgStr, "'Allomorph' has a translation in the updated po file data");
			Assert.AreEqual(1, posTest.MsgStr.Count, "'Allomorph' has one line of translation in the updated po file data");
			Assert.AreEqual("Allomorphe", posTest.MsgStr[0], "'Allomorph' has the expected translation in the updated po file data");

			// test message that exists in the pot data but not in the French po data
			exists = dictUpdatedPo.TryGetValue("created on: ", out posTest);
			Assert.IsTrue(exists, "'created on: ' exists in the updated po file data");
			Assert.IsNotNull(posTest, "'created on: ' exists in the updated po file data (posTest not null)");
			Assert.IsTrue(posTest.HasEmptyMsgStr, "'created on: ' does not have a translation in the updated po file data");

			// test message that is translated in the French po data but is not found in the pot data
			exists = dictUpdatedPo.TryGetValue("Yes to &All", out posTest);
			Assert.IsTrue(exists, "'Yes to &All' exists in the updated po file data even though it doesn't exist in the pot data");
			Assert.IsNotNull(posTest, "'Yes to &All' exists in the updated po file data (posTest not null)");
			Assert.IsNotNull(posTest.MsgStr, "'Yes to &All' has a translation in the updated po file data");
			Assert.AreEqual(1, posTest.MsgStr.Count, "'Yes to &All' has one line of translation in the updated po file data");
			Assert.AreEqual("Oui a tous", posTest.MsgStr[0], "'Yes to &All' has the expected translation in the updated po file data");
			Assert.IsNotNull(posTest.AutoComments, "'Yes to &All' has AutoComments in the updated po file data");
			Assert.AreEqual(2, posTest.AutoComments.Count, "'Yes to &All' has two lines of AutoComments in the updated po file data");
			Assert.AreEqual("(Not used by FieldWorks 1.2.3)", posTest.AutoComments[0], "'Yes to &All' has the expected first line of AutoComments in the updated po file data");
			Assert.AreEqual("/Src/TE/TeDialogs/FilesOverwriteDialog.resx::btnYesToAll.Text", posTest.AutoComments[1], "'Yes to &All' has the expected second line of AutoComments in the updated po file data");

			// test message that exists but is not translated in the French po data, and is not found in the pot data
			exists = dictUpdatedPo.TryGetValue("Junk Test Message", out posTest);
			Assert.IsTrue(exists, "'Junk Test Message' exists in the updated po file data");
			Assert.IsNotNull(posTest, "'Junk Test Message' exists in the updated po file data (posTest not null)");
			Assert.IsTrue(posTest.HasEmptyMsgStr, "'Junk Test Message' does not have a translation in the updated po file data");
			Assert.IsNotNull(posTest.AutoComments, "'Junk Test Message' has AutoComments in the updated po file data");
			Assert.AreEqual(1, posTest.AutoComments.Count, "'Junk Test Message' has one line of AutoComments in the updated po file data");
			Assert.AreEqual("(Not used by FieldWorks 1.2.3)", posTest.AutoComments[0], "'Junk Test Message' has the expected first line of AutoComments in the updated po file data");
			Assert.IsNotNull(posTest.UserComments, "'Junk Test Message' has UserComments in the updated po file data");
			Assert.AreEqual(1, posTest.UserComments.Count, "'Junk Test Message' has one line of UserComments in the updated po file data");
			Assert.AreEqual(" This message does not actually appear anywhere, but is here for unit testing!", posTest.UserComments[0], "'Junk Test Message' has the expected first line of UserComments in the updated po file data");
		}

#region TestData
		static private readonly string _sResxLeadingNewlines =
"<?xml version=\"1.0\" encoding=\"utf-8\"?>" + Environment.NewLine +
"<root>" + Environment.NewLine +
"  <xsd:schema id=\"root\" xmlns=\"\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:msdata=\"urn:schemas-microsoft-com:xml-msdata\">" + Environment.NewLine +
"    <xsd:import namespace=\"http://www.w3.org/XML/1998/namespace\" />" + Environment.NewLine +
"  </xsd:schema>" + Environment.NewLine +
"  <data name=\"kstidFatalError2\" xml:space=\"preserve\">" + Environment.NewLine +
"    <value>" + Environment.NewLine +
"" + Environment.NewLine +
"In order to protect your data, the FieldWorks program needs to close." + Environment.NewLine +
"" + Environment.NewLine +
"You should be able to restart it normally." + Environment.NewLine +
"</value>" + Environment.NewLine +
"    <comment>Displayed in a message box.</comment>" + Environment.NewLine +
"  </data>" + Environment.NewLine +
"  <data name=\"kstidFatalError1\" xml:space=\"preserve\">" + Environment.NewLine +
"    <value>" + Environment.NewLine +
"In order to protect your data, the FieldWorks program needs to close." + Environment.NewLine +
"" + Environment.NewLine +
"You should be able to restart it normally.</value>" + Environment.NewLine +
"    <comment>Displayed in a message box.</comment>" + Environment.NewLine +
"  </data>" + Environment.NewLine +
"  <data name=\"kstidFatalError0\" xml:space=\"preserve\">" + Environment.NewLine +
"    <value>In order to protect your data, the FieldWorks program needs to close." + Environment.NewLine +
"" + Environment.NewLine +
"You should be able to restart it normally.</value>" + Environment.NewLine +
"    <comment>Displayed in a message box.</comment>" + Environment.NewLine +
"  </data>" + Environment.NewLine +
"</root>";

		#endregion

		[Test]
		public void TestReadWriteLeadingNewlines()
		{
			var rgsPoStrings = new List<POString>();
			var xdoc = new XmlDocument();
			xdoc.LoadXml(_sResxLeadingNewlines);
			Assert.IsNotNull(xdoc.DocumentElement);
			Program.ProcessResxData(xdoc.DocumentElement, @"/Src/FwResources//FwStrings.resx", rgsPoStrings);
			//SUT
			Assert.AreEqual(3, rgsPoStrings.Count, "Three localizable strings found in resx data");
			var postr0 = rgsPoStrings[0];
			Assert.IsNotNull(postr0.MsgId, "First resx string has MsgId data");
			//Assert.AreEqual(6, postr0.MsgId.Count, "First resx string has five lines of MsgId data");
			//Assert.AreEqual("", postr0.MsgId[0], "First resx string has the expected MsgId data line one");
			//Assert.AreEqual("", postr0.MsgId[1], "First resx string has the expected MsgId data line two");
			//Assert.AreEqual("In order to protect your data, the FieldWorks program needs to close.", postr0.MsgId[2], "First resx string has the expected MsgId data line three");
			//Assert.AreEqual("", postr0.MsgId[3], "First resx string has the expected MsgId data line four");
			//Assert.AreEqual("You should be able to restart it normally.", postr0.MsgId[4], "First resx string has the expected MsgId data line five");
			//Assert.AreEqual("", postr0.MsgId[5], "First resx string has the expected MsgId data line six");
			Assert.AreEqual(5, postr0.MsgId.Count, "First resx string has five lines of MsgId data");
			Assert.AreEqual("\\n", postr0.MsgId[0], "First resx string has the expected MsgId data line one");
			Assert.AreEqual("\\n", postr0.MsgId[1], "First resx string has the expected MsgId data line two");
			Assert.AreEqual("In order to protect your data, the FieldWorks program needs to close.\\n", postr0.MsgId[2], "First resx string has the expected MsgId data line three");
			Assert.AreEqual("\\n", postr0.MsgId[3], "First resx string has the expected MsgId data line four");
			Assert.AreEqual("You should be able to restart it normally.\\n", postr0.MsgId[4], "First resx string has the expected MsgId data line five");
			Assert.IsTrue(postr0.HasEmptyMsgStr, "First resx string has no MsgStr data (as expected)");
			Assert.IsNull(postr0.UserComments, "First resx string has no User Comments (as expected)");
			Assert.IsNull(postr0.Reference, "First resx string has no Reference data (as expected)");
			Assert.IsNull(postr0.Flags, "First resx string has no Flags data (as expected)");
			Assert.IsNotNull(postr0.AutoComments, "Third resx string has Auto Comments");
			Assert.AreEqual(2, postr0.AutoComments.Count, "First resx string has two lines of Auto Comments");
			Assert.AreEqual("Displayed in a message box.", postr0.AutoComments[0], "First resx string has the expected Auto Comment line one");
			Assert.AreEqual("/Src/FwResources//FwStrings.resx::kstidFatalError2", postr0.AutoComments[1], "First resx string has the expected Auto Comment line two");

			var postr1 = rgsPoStrings[1];
			Assert.IsNotNull(postr1.MsgId, "Second resx string has MsgId data");
			Assert.AreEqual(4, postr1.MsgId.Count, "Second resx string has four lines of MsgId data");
			Assert.AreEqual("\\n", postr1.MsgId[0], "Second resx string has the expected MsgId data line one");
			Assert.AreEqual("In order to protect your data, the FieldWorks program needs to close.\\n", postr1.MsgId[1], "Second resx string has the expected MsgId data line two");
			Assert.AreEqual("\\n", postr1.MsgId[2], "Second resx string has the expected MsgId data line three");
			Assert.AreEqual("You should be able to restart it normally.", postr1.MsgId[3], "Second resx string has the expected MsgId data line four");
			Assert.IsTrue(postr1.HasEmptyMsgStr, "Second resx string has no MsgStr data (as expected)");
			Assert.IsNull(postr1.UserComments, "Second resx string has no User Comments (as expected)");
			Assert.IsNull(postr1.Reference, "Second resx string has no Reference data (as expected)");
			Assert.IsNull(postr1.Flags, "Second resx string has no Flags data (as expected)");
			Assert.IsNotNull(postr1.AutoComments, "Third resx string has Auto Comments");
			Assert.AreEqual(2, postr1.AutoComments.Count, "Second resx string has two lines of Auto Comments");
			Assert.AreEqual("Displayed in a message box.", postr1.AutoComments[0], "Second resx string has the expected Auto Comment line one");
			Assert.AreEqual("/Src/FwResources//FwStrings.resx::kstidFatalError1", postr1.AutoComments[1], "Second resx string has the expected Auto Comment line two");

			var postr2 = rgsPoStrings[2];
			Assert.IsNotNull(postr2.MsgId, "Third resx string has MsgId data");
			Assert.AreEqual(3, postr2.MsgId.Count, "Third resx string has three lines of MsgId data");
			Assert.AreEqual("In order to protect your data, the FieldWorks program needs to close.\\n", postr2.MsgId[0], "Third resx string has the expected MsgId data line one");
			Assert.AreEqual("\\n", postr2.MsgId[1], "Third resx string has the expected MsgId data line two");
			Assert.AreEqual("You should be able to restart it normally.", postr2.MsgId[2], "Third resx string has the expected MsgId data line three");
			Assert.IsTrue(postr2.HasEmptyMsgStr, "Third resx string has no MsgStr data (as expected)");
			Assert.IsNull(postr2.UserComments, "Third resx string has no User Comments (as expected)");
			Assert.IsNull(postr2.Reference, "Third resx string has no Reference data (as expected)");
			Assert.IsNull(postr2.Flags, "Third resx string has no Flags data (as expected)");
			Assert.IsNotNull(postr2.AutoComments, "Third resx string has Auto Comments");
			Assert.AreEqual(2, postr2.AutoComments.Count, "Third resx string has two lines of Auto Comments");
			Assert.AreEqual("Displayed in a message box.", postr2.AutoComments[0], "Third resx string has the expected Auto Comment line one");
			Assert.AreEqual("/Src/FwResources//FwStrings.resx::kstidFatalError0", postr2.AutoComments[1], "Third resx string has the expected Auto Comment line two");

			var sw = new StringWriter();
			postr0.Write(sw);
			postr1.Write(sw);
			postr2.Write(sw);
			var sPo = sw.ToString();
			Assert.IsNotNull(sPo, "Writing resx strings' po data produced output");
			var poLines = sPo.Split(new[] { Environment.NewLine }, 100, StringSplitOptions.None);
			Assert.AreEqual("#. Displayed in a message box.", poLines[0]);
			Assert.AreEqual("#. /Src/FwResources//FwStrings.resx::kstidFatalError2", poLines[1]);
			Assert.AreEqual("msgid \"\"", poLines[2]);
			Assert.AreEqual("\"\\n\"", poLines[3]);
			Assert.AreEqual("\"\\n\"", poLines[4]);
			Assert.AreEqual("\"In order to protect your data, the FieldWorks program needs to close.\\n\"", poLines[5]);
			Assert.AreEqual("\"\\n\"", poLines[6]);
			Assert.AreEqual("\"You should be able to restart it normally.\\n\"", poLines[7]);
			Assert.AreEqual("msgstr \"\"", poLines[8]);
			Assert.AreEqual("", poLines[9]);
			Assert.AreEqual("#. Displayed in a message box.", poLines[10]);
			Assert.AreEqual("#. /Src/FwResources//FwStrings.resx::kstidFatalError1", poLines[11]);
			Assert.AreEqual("msgid \"\"", poLines[12]);
			Assert.AreEqual("\"\\n\"", poLines[13]);
			Assert.AreEqual("\"In order to protect your data, the FieldWorks program needs to close.\\n\"", poLines[14]);
			Assert.AreEqual("\"\\n\"", poLines[15]);
			Assert.AreEqual("\"You should be able to restart it normally.\"", poLines[16]);
			Assert.AreEqual("msgstr \"\"", poLines[17]);
			Assert.AreEqual("", poLines[18]);
			Assert.AreEqual("#. Displayed in a message box.", poLines[19]);
			Assert.AreEqual("#. /Src/FwResources//FwStrings.resx::kstidFatalError0", poLines[20]);
			Assert.AreEqual("msgid \"\"", poLines[21]);
			Assert.AreEqual("\"In order to protect your data, the FieldWorks program needs to close.\\n\"", poLines[22]);
			Assert.AreEqual("\"\\n\"", poLines[23]);
			Assert.AreEqual("\"You should be able to restart it normally.\"", poLines[24]);
			Assert.AreEqual("msgstr \"\"", poLines[25]);
			Assert.AreEqual("", poLines[26]);
			Assert.AreEqual("", poLines[27]);
			Assert.AreEqual(28, poLines.Length);

			var sr = new StringReader(sPo);
			var postr0A = POString.ReadFromFile(sr);
			var postr1A = POString.ReadFromFile(sr);
			var postr2A = POString.ReadFromFile(sr);
			var postr3A = POString.ReadFromFile(sr);
			Assert.IsNotNull(postr0A, "Read first message from leading newline test data");
			Assert.IsNotNull(postr1A, "Read second message from leading newline test data");
			Assert.IsNotNull(postr2A, "Read third message from leading newline test data");
			Assert.IsNull(postr3A, "Only three messages in leading newline test data");

			CheckStringList(postr0.MsgId, postr0A.MsgId, "Preserve MsgId in first message from leading newline test data");
			CheckStringList(postr0.MsgStr, postr0A.MsgStr, "Preserve MsgStr in first message from leading newline test data");
			CheckStringList(postr0.UserComments, postr0A.UserComments, "Preserve UserComments in first message from leading newline test data");
			CheckStringList(postr0.Reference, postr0A.Reference, "Preserve Reference in first message from leading newline test data");
			CheckStringList(postr0.Flags, postr0A.Flags, "Preserve Flags in first message from leading newline test data");
			CheckStringList(postr0.AutoComments, postr0A.AutoComments, "Preserve AutoComments in first message from leading newline test data");

			CheckStringList(postr1.MsgId, postr1A.MsgId, "Preserve MsgId in second message from leading newline test data");
			CheckStringList(postr1.MsgStr, postr1A.MsgStr, "Preserve MsgStr in second message from leading newline test data");
			CheckStringList(postr1.UserComments, postr1A.UserComments, "Preserve UserComments in second message from leading newline test data");
			CheckStringList(postr1.Reference, postr1A.Reference, "Preserve Reference in second message from leading newline test data");
			CheckStringList(postr1.Flags, postr1A.Flags, "Preserve Flags in second message from leading newline test data");
			CheckStringList(postr1.AutoComments, postr1A.AutoComments, "Preserve AutoComments in second message from leading newline test data");

			CheckStringList(postr2.MsgId, postr2A.MsgId, "Preserve MsgId in third message from leading newline test data");
			CheckStringList(postr2.MsgStr, postr2A.MsgStr, "Preserve MsgStr in third message from leading newline test data");
			CheckStringList(postr2.UserComments, postr2A.UserComments, "Preserve UserComments in third message from leading newline test data");
			CheckStringList(postr2.Reference, postr2A.Reference, "Preserve Reference in third message from leading newline test data");
			CheckStringList(postr2.Flags, postr2A.Flags, "Preserve Flags in third message from leading newline test data");
			CheckStringList(postr2.AutoComments, postr2A.AutoComments, "Preserve AutoComments in third message from leading newline test data");
		}

		private void CheckStringList(List<string> list1, List<string> list2, string msg)
		{
			if (list1 == null)
			{
				Assert.IsNull(list2, msg + " (both null)");
				return;
			}
			Assert.IsNotNull(list2, msg + " (both not null)");
			Assert.AreEqual(list1.Count, list2.Count, msg + " (same number of lines)");
			for (int i = 0; i < list1.Count; ++i)
				Assert.AreEqual(list1[i], list2[i], String.Format("{0} - line {1} is same", msg, i));
		}
	}
}
