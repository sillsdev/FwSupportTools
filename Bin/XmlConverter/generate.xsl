<?xml version="1.0" encoding="us-ascii"?>

<!--
  -		generate.xsl
  -
  -		Generate stylesheets from Conceptual Models
  -
  -		Neil Mayhew - 2009-06-23
  -->

<xsl:stylesheet version="1.0"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:out="http://www.w3.org/1999/XSL/TransformAlias"
	xmlns:xi="http://www.w3.org/2001/XInclude"
	exclude-result-prefixes="xi">

	<xsl:namespace-alias stylesheet-prefix="out" result-prefix="xsl"/>

	<xsl:output method="xml" encoding="us-ascii" indent="yes"/>

	<xsl:template match="EntireModel">
		<xsl:if test="not(@version)">
			<xsl:message terminate="yes">Error: model has no version</xsl:message>
		</xsl:if>
		<out:stylesheet version="1.0">
			<out:import href="flatten.xsl"/>
			<out:param name="model-version" select="{@version}"/>
			<xsl:apply-templates select="*">
				<xsl:sort select="@id"/>
			</xsl:apply-templates>
		</out:stylesheet>
	</xsl:template>

	<xsl:template match="xi:include">
		<xsl:apply-templates select="document(@href, .)"/>
	</xsl:template>

	<xsl:template match="CellarModule">
		<xsl:apply-templates select="*">
			<xsl:sort select="@id"/>
		</xsl:apply-templates>
	</xsl:template>

	<xsl:template match="class">
		<xsl:if test="@num != 0">
			<out:template name="{@id}-grouping" match="{@id}" mode="grouping">
				<out:call-template name="{@base}-grouping"/>
				<xsl:element name="{@id}">
					<xsl:comment>Basic attributes</xsl:comment>
					<xsl:apply-templates select="props/basic" mode="grouping">
						<xsl:sort select="@id"/>
					</xsl:apply-templates>
					<xsl:comment>Owning attributes</xsl:comment>
					<xsl:apply-templates select="props/owning" mode="grouping">
						<xsl:sort select="@id"/>
					</xsl:apply-templates>
					<xsl:comment>Reference attributes</xsl:comment>
					<xsl:apply-templates select="props/rel" mode="grouping">
						<xsl:sort select="@id"/>
					</xsl:apply-templates>
				</xsl:element>
			</out:template>
			<out:template name="{@id}-children" match="{@id}" mode="children">
				<out:call-template name="{@base}-children"/>
				<xsl:apply-templates select="props/owning" mode="children">
					<xsl:sort select="@id"/>
				</xsl:apply-templates>
			</out:template>
		</xsl:if>
	</xsl:template>

	<xsl:template match="basic|owning|rel" mode="grouping">
		<xsl:variable name="suffix"
			select="ancestor::CellarModule/@num * 1000 + ancestor::class/@num"/>
		<out:apply-templates select="{@id}{$suffix}" mode="property"/>
	</xsl:template>

	<xsl:template match="owning" mode="children">
		<xsl:variable name="suffix"
			select="ancestor::CellarModule/@num * 1000 + ancestor::class/@num"/>
		<out:apply-templates select="{@id}{$suffix}/*">
			<out:with-param name="owningflid">
				<xsl:value-of select="$suffix * 1000 + @num"/>
			</out:with-param>
		</out:apply-templates>
	</xsl:template>

	<xsl:template match="*">
		<xsl:message terminate="yes">Unrecognised tag: <xsl:value-of select="name()"/></xsl:message>
	</xsl:template>

</xsl:stylesheet>
