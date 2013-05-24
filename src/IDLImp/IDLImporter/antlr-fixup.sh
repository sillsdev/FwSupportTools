#!/bin/sh

#	$Id$
#
#	Fix the output of ANTLR so that it compiles
#
#	Neil Mayhew - 2007-12-06

set -e

for F in "${1?}Parser.cs" "${1?}Lexer.cs" "${1?}TokenTypes.cs" "${1?}ParserTokenTypes.cs"
do
	[ -f "$F" ] || continue
	echo "$F"
	ed -s "$F" <<-\EOF
		g/public static readonly string\[\] tokenNames_ = /;#\
			s/new string\[\] //
		g/\\"/;#\
			s/@"""/"/\
			s/"""/"/
		w
		q
	EOF
done
