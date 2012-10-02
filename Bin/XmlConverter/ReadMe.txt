XmlConverter: XML conversion for FW 6.0 -> 7.0 migration

Run nant to create convert.xsl from MasterFieldWorksModel.xml.

Use convert.xsl on whatever file you want to convert.

If you are using the msxsl processor, the command would be:

msxsl OLDFILE convert.xsl -o NEWFILE

Note that flatten.xsl is a sub-stylesheet of convert.xsl and will need to be present
in the same directory as convert.xsl.

A private copy of MasterFieldWorksModel.xml is kept here because it needs to be the one
that was used in FW 6.0.
