#!/bin/bash
# Perform test on one file

DoTest()
{
	mono --debug ../bin/Debug/IDLImp.exe /x 0 \
		/n SIL.Fieldworks.Test \
		/c ../IDLImporter/IDLImp.xml \
		/u LanguageLib \
		/u FwKernelLib \
		/u Views $1 \
			&& $DIFF $2.ok $2
}

[ -n "$DIFF" ] || DIFF="diff --brief"

if [ $# -eq 0 ]
then
	set -- FwKernelTlb ViewsTlb DbAccessTlb MigrateDataTlb FwCellarTlb LanguageTlb
fi

for i
do
	DoTest $i.idl $i.cs
done
