#!/bin/bash

. "${0%.sh}.config" # Settings for PBUILDERDIR, DISTRIBUTIONS, etc.

NOOP=

if [ "$1" = "-n" ]
then
	NOOP=:
	shift
fi

cpuarch()
{
	case $1 in
	amd64) echo x86_64;;
	*)     echo $1;;
	esac
}

get_field() # FIELD SRC
{
	if grep -q "BEGIN PGP SIGNED MESSAGE" $2
	then
		DECRYPT="gpg -d"
	else
		DECRYPT="cat"
	fi

	$DECRYPT $2 2>/dev/null | grep-dctrl -s$1 -n :
}

has_arch() # ARCH SRC
{
	for A in $(get_field Architecture $2)
	do
		if [ $A = $1 -o $A = any ]
		then
			return 0
		fi
	done
	return 1
}

binaries()
{
	get_field Binary $1 | sed 's/,//g'
}

export DIST ARCH

for SRC
do
	for DIST in $DISTRIBUTIONS
	do
		for ARCH in $ARCHES
		do
			PACKAGE=${SRC%.dsc}
			RESULT="$PBUILDERDIR/$DIST/$ARCH/result"
			DEB1="$RESULT/${PACKAGE}_${ARCH}.deb"
			DEB2="$RESULT/${PACKAGE}+${DIST}1_${ARCH}.deb"

			OPTS=()

			# Build arch-independent packages only once
			if [ $ARCH != amd64 ]
			then
				OPTS+="--binary-arch"

				# Don't build if not for this arch
				if ! has_arch $ARCH $SRC
				then
					continue
				fi
			fi

			if [ $SRC -nt $DEB1 -a $SRC -nt $DEB2 ]
			then
				echo PACKAGE=$PACKAGE DIST=$DIST ARCH=$ARCH
				$NOOP setarch $(cpuarch $ARCH) pbuilder --build "${OPTS[@]}" $SRC
				echo $? | $NOOP tee $RESULT/${PACKAGE}_$ARCH.status
				$NOOP rm -f $RESULT/${PACKAGE}.{dsc,{debian.,orig.,}tar.gz}
			fi
		done
	done
done
