RootNamespace = SIL.FieldWorks.Tools

DEFS += DEBUG;TRACE
SYSS += -r:System.Windows.Forms

SRCS = AssemblyInfo.cs IDLImp.cs
REFS = IDLImporter/IDLImporter.dll
RESS =
ICON = App.ico
DATA =

all: IDLImp.exe

IDLImp.exe: $(SRCS) $(REFS) $(RESS) $(ICON)
	gmcs -debug -out:$@ $(patsubst %, -define:"%", $(DEFS)) $(SRCS) \
		$(REFS:%=-r:%) $(RESS:%=-resource:%) $(ICON:%=-win32icon:%) \
		$(SYSS)

$(REFS)::
	@$(MAKE) -C $(@D) $(@F) -q || \
	 $(MAKE) -C $(@D) $(@F)

clean:
	$(RM) IDLImp.exe IDLImp.exe.mdb
	$(MAKE) -C IDLImporter clean

PKGDIR = ../..
PKGFILES = IDLImp.exe IDLImp.exe.mdb

package: all
	cp -pf $(PKGFILES) $(PKGDIR)
	$(MAKE) -C IDLImporter $@ PKGDIR=../$(PKGDIR)
