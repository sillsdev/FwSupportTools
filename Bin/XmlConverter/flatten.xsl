<?xml version='1.0' encoding='us-ascii'?>

<!--
	flatten.xsl

	Flatten FW XML dumps

	Neil Mayhew - 2009-05-10
-->

<xsl:stylesheet version='1.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>

<xsl:output method='xml' version='1.0' encoding='utf-8' indent='no'/>
<xsl:strip-space elements="*"/>

<!-- Roots -->

<!-- Database root -->
<xsl:template match="FwDatabase">
	<languageproject version="{$model-version}">
		<xsl:text>&#10;</xsl:text>
		<!-- Process top-level classes -->
		<xsl:apply-templates select="*"/>
	</languageproject>
</xsl:template>

<!-- Root objects -->
<xsl:template match="*">
	<xsl:param name="owningflid"/>
	<rt class="{name()}">
		<xsl:attribute name="guid">
			<xsl:if test="not(@id)">
				<xsl:message>Missing @id for rt on <xsl:value-of select="name()"/>&#10;</xsl:message>
				<xsl:value-of select="generate-id()"/>
			</xsl:if>
			<xsl:value-of select="substring(@id, 2)"/>
		</xsl:attribute>
		<xsl:if test="../../@id">
			<xsl:attribute name="ownerguid">
				<xsl:value-of select="substring(../../@id, 2)"/>
			</xsl:attribute>
			<xsl:attribute name="owningflid">
				<xsl:value-of select="$owningflid"/>
			</xsl:attribute>
			<xsl:attribute name="owningord">
				<xsl:value-of select="position()"/>
			</xsl:attribute>
		</xsl:if>
		<xsl:apply-templates select="@*" mode="copy"/>
		<xsl:apply-templates select="." mode="grouping"/>
	</rt>
	<xsl:text>&#10;</xsl:text>
	<xsl:apply-templates select="." mode="children"/>
</xsl:template>

<xsl:template match="*/@id" mode="copy"/>

<!-- Custom field definitions -->

<xsl:template match="FwDatabase/AdditionalFields">
	<xsl:apply-templates select="." mode="copy"/>
	<xsl:text>&#10;</xsl:text>
</xsl:template>

<xsl:template match="CustomField/@flid" mode="copy"/>

<!-- Built-in classes -->

<xsl:template name="CmObject-grouping" match="CmObject" mode="grouping">
	<CmObject>
		<xsl:apply-templates select="Custom|CustomStr" mode="property"/>
	</CmObject>
</xsl:template>

<xsl:template name="CmObject-children" match="CmObject" mode="children"/>

<!-- Properties -->

<xsl:template match="*[AStr|AUni]" mode="property">
	<xsl:element name="{translate(name(), '0123456789', '')}">
		<xsl:apply-templates select="@*" mode="copy"/>
		<xsl:apply-templates select="node()" mode="value"/>
	</xsl:element>
	<xsl:text>&#10;</xsl:text>
</xsl:template>

<xsl:template match="*[*/@val]" mode="property">
	<xsl:if test="count(*) != 1">
		<xsl:message terminate="yes">Basic in a sequence!&#10;</xsl:message>
	</xsl:if>
	<xsl:element name="{translate(name(), '0123456789', '')}">
		<xsl:apply-templates select="@*" mode="copy"/>
		<xsl:apply-templates select="*/@val" mode="copy"/>
	</xsl:element>
	<xsl:text>&#10;</xsl:text>
</xsl:template>

<xsl:template match="*" mode="property">
	<xsl:element name="{translate(name(), '0123456789', '')}">
		<xsl:apply-templates select="@*" mode="copy"/>
		<xsl:apply-templates select="*" mode="value"/>
	</xsl:element>
	<xsl:text>&#10;</xsl:text>
</xsl:template>

<!-- Values -->

<!-- MultiString|MultiUnicode type -->
<xsl:template match="AStr|AUni" mode="value">
	<xsl:apply-templates select="." mode="copy"/>
</xsl:template>

<!-- Contained type -->
<xsl:template match="Str|Uni|Binary|Image|Prop|WsProp" mode="value">
	<xsl:if test="count(../*) != 1">
		<xsl:message terminate="yes"><xsl:value-of select="name()"/> in a sequence!&#10;</xsl:message>
	</xsl:if>
	<xsl:apply-templates select="." mode="copy"/>
</xsl:template>

<!-- Basic type -->
<xsl:template match="*[@val]" mode="value">
	<xsl:message terminate="yes"><xsl:value-of select="name()"/> as object!&#10;</xsl:message>
</xsl:template>

<!-- Reference -->
<xsl:template match="Link" mode="value">
	<objsur t="r" guid="{substring(@target, 2)}"/>
</xsl:template>

<!-- Owned object -->
<xsl:template match="*" mode="value">
	<objsur t="o">
		<xsl:attribute name="guid">
			<xsl:if test="not(@id)">
				<xsl:message>Missing @id for objsur on <xsl:value-of select="name()"/>&#10;</xsl:message>
				<xsl:value-of select="generate-id()"/>
			</xsl:if>
			<xsl:value-of select="substring(@id, 2)"/>
		</xsl:attribute>
	</objsur>
</xsl:template>

<!-- Copying input to output -->

<xsl:template match="@*|node()" mode="copy">
	<xsl:copy>
		<xsl:apply-templates select="@*|node()" mode="copy"/>
	</xsl:copy>
</xsl:template>

</xsl:stylesheet>
