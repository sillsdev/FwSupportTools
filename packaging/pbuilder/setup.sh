#!/bin/bash

# setup.sh
# Setup or update mirrors

set -e

. $(dirname $0)/common.sh
init

# Process arguments.
while (( $# )); do
	case $1 in
		# Process individual arguments here. Use shift and $1 to get an argument value.
		--update) update=true ;;
		*) echo "Error: Unexpected argument \"$1\". Exiting." ; exit 1 ;;
	esac
	shift
done

PBUILDERDIR="${PBUILDERDIR:-$(dirname "$0")}"
LOCALMIRROR=${LOCALMIRROR:-$PBUILDERDIR}

cd "$PBUILDERDIR"

KEYRINGLLSO="$PBUILDERDIR/sil-testing.gpg"
KEYRINGPSO="$PBUILDERDIR/sil.gpg"

if [ ! -f $KEYRINGPSO ]; then
	wget --output-document=$KEYRINGPSO http://packages.sil.org/sil.gpg
fi

for D in ${DISTRIBUTIONS-precise saucy trusty utopic} # jessie}
do
	for A in ${ARCHES-amd64 i386}
	do
		[ -e $D/$A/base.tgz -a -z "$update" ] && continue
		[ ! -e $D/$A/base.tgz -a -n "$update" ] && continue

		echo "Processing $D/$A"

		mkdir -p $D/$A/{aptcache,build,result}
		[ ! -e $D/$A/result/Packages ] && touch $D/$A/result/Packages

		OTHERMIRROR="deb file://$LOCALMIRROR/$D/$A/result/ ./"

		case $D in precise|quantal|raring|saucy|trusty|utopic)
			MIRROR="${UBUNTU_MIRROR:-http://archive.ubuntu.com/ubuntu/}"
			COMPONENTS="main universe multiverse"
			KEYRING1="/usr/share/keyrings/ubuntu-archive-keyring.gpg"
			PROXY="$http_proxy"
			for S in backports updates; do
				OTHERMIRROR+="|deb $MIRROR $D-$S $COMPONENTS"
			done
			LLSO="http://linux.lsdev.sil.org/ubuntu/"
			KEYRING2=$KEYRINGLLSO
			PSO="http://packages.sil.org/ubuntu/"
			KEYRING3=$KEYRINGPSO
			for S in "" "-experimental"; do
				OTHERMIRROR+="|deb $LLSO $D$S $COMPONENTS|deb $PSO $D$S $COMPONENTS"
			done
			;;
		*)
			MIRROR="$local_debian_mirror"
			COMPONENTS="main"
			LLSO="http://linux.lsdev.sil.org/debian/"
			PSO="http://packages.sil.org/debian/"
			KEYRING1=$KEYRINGLLSO
			KEYRING2=$KEYRINGPSO
			KEYRING3=""
			OTHERMIRROR+="|deb $LLSO $D $COMPONENTS|deb $PSO $D $COMPONENTS"
			PROXY=""
		esac

		if [ -z "$update" ]; then
			options="--create --debootstrap debootstrap"
		else
			options="--update --override-config"
		fi

		sudo HOME=~ DIST=$D ARCH=$A pbuilder $options \
			${KEYRING1:+--debootstrapopts --keyring=}$KEYRING1 \
			${KEYRING2:+--keyring }$KEYRING2 \
			${KEYRING3:+--keyring }$KEYRING3 \
			--extrapackages "apt-utils devscripts lsb-release" \
			--othermirror "$OTHERMIRROR" \
			--mirror "$MIRROR" \
			--components "$COMPONENTS" \
			${PROXY:+--http-proxy }$PROXY
	done
done