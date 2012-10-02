#!/bin/sh

#
#	Create the new-format strings-en.txt from the old-format rc file
#
#	Neil Mayhew - 2007-03-02
#

g++ -E -include CompileStringTable.h -I ../Generic -x c $1 |
sed -e '
	s/^[ 	]*//
	/^$/d
	/^#/d
	s/ /	/
	s/	"/	/
	s/"$//
'
