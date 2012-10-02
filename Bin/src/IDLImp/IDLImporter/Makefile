# Makefile for IDL grammar

DEFS +=

ANTLRDIR = ../antlr-2.7.7

ANTLR = java -cp $(ANTLRDIR)/lib/antlr.jar antlr.Tool
#ANTLR_OPTS=-trace

IDLFILE = IDL
IDHFILE = Idh
SURVFILE = Surveyor

GENDIR = Output

GENIDL = \
	$(GENDIR)/$(IDLFILE)Parser.cs \
	$(GENDIR)/$(IDLFILE)Lexer.cs \
	$(GENDIR)/$(IDLFILE)TokenTypes.cs
GENIDH = \
	$(GENDIR)/$(IDHFILE)Parser.cs \
	$(GENDIR)/$(IDHFILE)Lexer.cs \
	$(GENDIR)/$(IDHFILE)ParserTokenTypes.cs
GENSURV = \
	$(GENDIR)/$(SURVFILE)Parser.cs \
	$(GENDIR)/$(SURVFILE)Lexer.cs \
	$(GENDIR)/$(SURVFILE)ParserTokenTypes.cs

GENSRCS = $(GENIDL) $(GENIDH) $(GENSURV)

all: IDLImporter.dll

rebuild: clean all

clean:
	$(RM) $(GENSRCS) $(GENSRCS:%.cs=%.txt)
	$(RM) IDLImporter.dll IDLImporter.dll.mdb

$(GENDIR)/%Parser.cs $(GENDIR)/%Lexer.cs $(GENDIR)/%TokenTypes.cs $(GENDIR)/%ParserTokenTypes.cs: %.g
	$(ANTLR) $(ANTLR_OPTS) -o $(GENDIR) $<
	./antlr-fixup.sh $*

$(GENIDL): idl.g
	@echo Running ANTLR on $<
	$(ANTLR) $(ANTLR_OPTS) -o $(GENDIR) $<
	./antlr-fixup.sh $@

$(GENIDH): idh.g
	@echo Running ANTLR on $<
	$(ANTLR) $(ANTLR_OPTS) -o $(GENDIR) $<
	./antlr-fixup.sh $@

$(GENSURV): SurveyorTags.g
	@echo Running ANTLR on $<
	$(ANTLR) $(ANTLR_OPTS) -o $(GENDIR) $<
	./antlr-fixup.sh $@

DEFS += DEBUG;TRACE
SYSS +=

SRCS = \
	AssemblyInfo.cs \
	CSharpCodeGenerator.cs \
	CSharpCodeProviderEx.cs \
	IdhCommentProcessor.cs \
	IDLConversions.cs \
	IDLImporter.cs \
	$(GENSRCS)

REFS = $(ANTLRDIR)/lib/antlr.runtime.dll
RESS =
ICON =
DATA = IDLConversions.xsd IDLImp.xml
COMPILE.net = \
	gmcs -debug -t:library -out:$@ $(patsubst %, -define:"%", $(DEFS)) $(SRCS) \
		$(REFS:%=-r:%) $(RESS:%=-resource:%) $(ICON:%=-win32icon:%) \
		$(SYSS)

IDLImporter.dll: $(SRCS) $(REFS) $(RESS) $(ICON)
	$(COMPILE.net)

PKGDIR = ../../..
PKGFILES = IDLImporter.dll IDLImporter.dll.mdb $(REFS) $(DATA)

package: all
	@mkdir -p $(PKGDIR)
	cp -pf $(PKGFILES) $(PKGDIR)
