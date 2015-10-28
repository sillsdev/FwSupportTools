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

function CheckOrLinkDebootstrapScript()
{
	if [ ! -f /usr/share/debootstrap/scripts/$1 ]; then
		if [[ $UBUNTU_DISTROS == *$1* ]]; then
			basedistro=gutsy
		elif [[ $UBUNTU_OLDDISTROS == *$1* ]]; then
			basedistro=gutsy
		else
			basedistro=sid
		fi
		sudo ln -s /usr/share/debootstrap/scripts/$basedistro /usr/share/debootstrap/scripts/$1
	fi
}

function addmirror()
{
	OTHERMIRROR="$OTHERMIRROR${OTHERMIRROR:+|}$1"
}

PBUILDERDIR="${PBUILDERDIR:-$(dirname "$0")}"

cd "$PBUILDERDIR"

KEYRINGLLSO="$PBUILDERDIR/sil-testing.gpg"
KEYRINGPSO="$PBUILDERDIR/sil.gpg"

if [ ! -f $KEYRINGPSO ]; then
	wget --output-document=$KEYRINGPSO http://packages.sil.org/sil.gpg
fi

for D in ${DISTRIBUTIONS:-$UBUNTU_DISTROS $UBUNTU_OLDDISTROS $DEBIAN_DISTROS}
do
	for A in ${ARCHES-amd64 i386}
	do
		[ -e $D/$A/base.tgz -a -z "$update" ] && continue
		[ ! -e $D/$A/base.tgz -a -n "$update" ] && continue

		echo "Processing $D/$A"

		mkdir -p $D/$A/{aptcache,build,result}

		OTHERMIRROR=""

		CheckOrLinkDebootstrapScript $D

		if [[ $UBUNTU_DISTROS == *$D* ]]; then
			MIRROR="${UBUNTU_MIRROR:-http://archive.ubuntu.com/ubuntu/}"
			COMPONENTS="main universe multiverse"
			KEYRING1="/usr/share/keyrings/ubuntu-archive-keyring.gpg"
			PROXY="$http_proxy"
			for S in backports updates; do
				addmirror "deb $MIRROR $D-$S $COMPONENTS"
			done
			LLSO="http://linux.lsdev.sil.org/ubuntu/"
			KEYRING2=$KEYRINGLLSO
			PSO="http://packages.sil.org/ubuntu/"
			KEYRING3=$KEYRINGPSO
			for S in "" "-proposed" "-updates" "-experimental"; do
				addmirror "deb $LLSO $D$S $COMPONENTS"
				addmirror "deb $PSO $D$S $COMPONENTS"
			done
		elif [[ $UBUNTU_OLDDISTROS == *$D* ]]; then
			MIRROR="${UBUNTU_OLDMIRROR:-http://old-releases.ubuntu.com/ubuntu/}"
			COMPONENTS="main universe multiverse"
			KEYRING1="/usr/share/keyrings/ubuntu-archive-keyring.gpg"
			PROXY="$http_proxy"
			for S in backports updates; do
				addmirror "deb $MIRROR $D-$S $COMPONENTS"
			done
			LLSO="http://linux.lsdev.sil.org/ubuntu/"
			KEYRING2=$KEYRINGLLSO
			PSO="http://packages.sil.org/ubuntu/"
			KEYRING3=$KEYRINGPSO
			for S in "" "-proposed" "-updates" "-experimental"; do
				addmirror "deb $LLSO $D$S $COMPONENTS"
				addmirror "deb $PSO $D$S $COMPONENTS"
			done
		else
			MIRROR="$local_debian_mirror"
			COMPONENTS="main"
			LLSO="http://linux.lsdev.sil.org/debian/"
			PSO="http://packages.sil.org/debian/"
			KEYRING1="/usr/share/keyrings/debian-archive-keyring.gpg"
			KEYRING2=$KEYRINGLLSO
			KEYRING3=$KEYRINGPSO
			addmirror "deb $LLSO $D $COMPONENTS"
			addmirror "deb $PSO $D $COMPONENTS"
			PROXY=""
		fi

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
