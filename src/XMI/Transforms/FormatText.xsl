<?xml version="1.0" encoding="utf-8"?>

<!--
	FormatText.xsl

	A template to convert hard formatting in plain text to markup

	Neil Mayhew - 2009-08-26

	Individual lines are enclosed in <para> elements, and
	leading spaces are converted to non-breaking spaces.
-->

<xsl:stylesheet version="1.0"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

	<xsl:template name="FormatText">
		<xsl:param name="text"/>
		<xsl:param name="indent" select="''"/>

		<xsl:choose>
			<xsl:when test="not(normalize-space($text))">
				<!-- Return nothing, which strips trailing space -->
			</xsl:when>

			<xsl:when test="substring($text, 1, 1) = '&#10;'">
				<!-- Ignore leading newlines, which also collapses multiple newlines -->
				<xsl:call-template name="FormatText">
					<xsl:with-param name="text" select="substring($text, 2)"/>
				</xsl:call-template>
			</xsl:when>

			<xsl:when test="substring($text, 1, 1) = ' '">
				<!-- Indent with non-breaking spaces -->
				<xsl:call-template name="FormatText">
					<xsl:with-param name="text" select="substring($text, 2)"/>
					<xsl:with-param name="indent" select="concat($indent, '&#160;')"/>
				</xsl:call-template>
			</xsl:when>

			<xsl:otherwise>
				<!-- Enclose lines in paras -->
				<para>
					<xsl:value-of select="$indent"/>
					<xsl:value-of select="substring-before($text, '&#10;')"/>
					<xsl:if test="not(contains($text, '&#10;'))">
						<xsl:value-of select="$text"/>
					</xsl:if>
				</para>
				<xsl:call-template name="FormatText">
					<xsl:with-param name="text" select="substring-after($text, '&#10;')"/>
				</xsl:call-template>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
</xsl:stylesheet>
