<?xml version="1.0" encoding="UTF-8"?>
<!--This transform should be applied to the results of the
MagicToCellarStage1.xsl transform (bin\src\xmi\transforms\xmiTempOuts\xmi2cellar1.xml.)
Below will order the superclasses before the subclasses so that the code generator
can build classes in the correct order.-->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<xsl:output method="xml" version="1.0" encoding="UTF-8" indent="yes"/>
	<xsl:strip-space elements="*"/>

	<xsl:template match="CellarModule">
		<xsl:copy>
			<xsl:apply-templates select="@*"/>
			<xsl:apply-templates select="*[not(self::class)]"/>
			<xsl:apply-templates select="class">
				<xsl:sort select="@depth" order="ascending"/>
			</xsl:apply-templates>
		</xsl:copy>
	</xsl:template>

	<xsl:template match="@*|node()">
		<xsl:copy>
			<xsl:apply-templates select="@*|node()"/>
		</xsl:copy>
	</xsl:template>
</xsl:stylesheet>
