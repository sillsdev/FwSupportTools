#!/bin/bash

. $(dirname $0)/common.sh
init

set -e

NOOP=

if [ "$1" = "-n" ]; then
	NOOP=:
	shift
elif [ "$1" = "-nn" ]; then
	NOOP=echo
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
	local A

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

if [[ "$1" != *.dsc ]]; then
	log "Usage: $0 <package>.dsc [<package2>.dsc]"
	exit 1
fi

export DIST ARCH

for SRC
do
	PACKAGE=$(basename "$SRC" .dsc)

	for DIST in $DISTRIBUTIONS
	do
		for ARCH in $ARCHES
		do
			RESULT="$PBUILDERDIR/$DIST/$ARCH/result"
			log "Processing ${DIST}/${ARCH}"

			if [ ! -d "$RESULT" ]
			then
				log "Directory $RESULT doesn't exist - skipping"
				continue
			fi

			CHANGES="$RESULT/${PACKAGE}_${ARCH}.changes"
			if [ ! -e $CHANGES ]; then
				CHANGES="$RESULT/${PACKAGE}+${DIST}1_${ARCH}.changes"
			fi

			OPTS=()

			if [ $ARCH = amd64 ]
			then
				# Don't build a source package - we already have one
				OPTS+=(--debbuildopts -b)
			else
				# Don't build arch-independent packages - that's done with amd64
				OPTS+=(--binary-arch)

				# Don't build if not for this arch
				if ! has_arch $ARCH $SRC
				then
					continue
				fi
			fi

			if [ $SRC -nt $CHANGES ]; then
				log "PACKAGE=$PACKAGE DIST=$DIST ARCH=$ARCH"
				$NOOP $PBUILDERSUDO setarch $(cpuarch $ARCH) pbuilder --build \
					--distribution $DIST --architecture $ARCH --logfile ${PACKAGE}-$DIST-$ARCH.log \
					"${OPTS[@]}" $SRC
				log "Done building: PACKAGE=$PACKAGE DIST=$DIST ARCH=$ARCH"
				echo $? | $NOOP tee $RESULT/${PACKAGE}_$ARCH.status
				#$NOOP $PBUILDERSUDO rm -f $RESULT/${PACKAGE}.{dsc,{debian.,orig.,}tar.*}
			else
				if [ -e $CHANGES ]; then
					err "Not building $PACKAGE for $DIST/$ARCH because it already exists"
				elif [ ! -e $SRC ]; then
					err "Not building $PACKAGE for $DIST/$ARCH - \"$SRC\" doesn't exist"
				else
					err "Not building $PACKAGE for $DIST/$ARCH - unknown reason (SRC=$SRC, CHANGES=$CHANGES)"
				fi
			fi
		done
	done
done
