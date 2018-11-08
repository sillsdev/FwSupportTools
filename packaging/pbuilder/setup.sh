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
		*) err "Error: Unexpected argument \"$1\". Exiting." ; exit 1 ;;
	esac
	shift
done

function CheckOrLinkDebootstrapScript()
{
	if [ ! -f /usr/share/debootstrap/scripts/$1 ]; then
		if [[ "$UBUNTU_DISTROS $UBUNTU_OLDDISTROS" == "*$1*" ]]; then
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


cd "${PBUILDERDIR:-$(dirname "$0")}"

KEYRINGLLSO="$PBUILDERDIR/sil-testing.gpg"
KEYRINGPSO="$PBUILDERDIR/sil.gpg"
KEYRINGNODE="$PBUILDERDIR/nodesource.gpg"

if [ ! -f $KEYRINGPSO ]; then
	wget --output-document=$KEYRINGPSO http://packages.sil.org/sil.gpg
fi

if [ ! -f $KEYRINGNODE ]; then
	wget --output-document=$KEYRINGNODE https://deb.nodesource.com/gpgkey/nodesource.gpg.key
fi

for D in ${DISTRIBUTIONS:-$UBUNTU_DISTROS $UBUNTU_OLDDISTROS $DEBIAN_DISTROS}
do
	for A in ${ARCHES-amd64 i386}
	do
		[ -e $D${DIST_ARCH_SEP}$A${DIST_ARCH_SEP}base.tgz -a -z "$update" ] && echo "$D${DIST_ARCH_SEP}$A already exists - skipping creation" && continue
		[ ! -e $D${DIST_ARCH_SEP}$A${DIST_ARCH_SEP}base.tgz -a -n "$update" ] && echo "$D${DIST_ARCH_SEP}$A doesn't exist - skipping update" && continue

		log "Processing $D${DIST_ARCH_SEP}$A"

		mkdir -p $D${DIST_ARCH_SEP}$A/{aptcache,build,result}

		OTHERMIRROR=""

		CheckOrLinkDebootstrapScript $D

		if [[ "$UBUNTU_DISTROS $UBUNTU_OLDDISTROS" == *$D* ]]; then
			if [[ $UBUNTU_DISTROS == *$D* ]]; then
				MIRROR="${UBUNTU_MIRROR:-http://archive.ubuntu.com/ubuntu/}"
			else
				MIRROR="${UBUNTU_OLDMIRROR:-http://old-releases.ubuntu.com/ubuntu/}"
			fi
			COMPONENTS="main universe multiverse"
			KEYRINGMAIN="/usr/share/keyrings/ubuntu-archive-keyring.gpg"
			PROXY="$http_proxy"
			for S in backports updates; do
				addmirror "deb $MIRROR $D-$S $COMPONENTS"
			done
			LLSO="http://linux.lsdev.sil.org/ubuntu/"
			PSO="http://packages.sil.org/ubuntu/"
			for S in "" "-proposed" "-updates" "-experimental"; do
				addmirror "deb $LLSO $D$S $COMPONENTS"
				addmirror "deb $PSO $D$S $COMPONENTS"
			done
			if [ $D != "precise" ]; then
				# allow to install current nodejs packages
				if [ -n "$update" ]; then
					# we can't use https when creating the chroot because apt-transport-https
					# isn't available yet
					addmirror "deb https://deb.nodesource.com/node_8.x $D main"
				fi
			fi
		elif [[ $DEBIAN_DISTROS == *$D* ]]; then
			MIRROR="${DEBIAN_MIRROR:-http://ftp.ca.debian.org/debian/}"
			COMPONENTS="main contrib non-free"
			KEYRINGMAIN="/usr/share/keyrings/debian-archive-keyring.gpg"
			PROXY="$http_proxy"
			LLSO="http://linux.lsdev.sil.org/debian/"
			PSO="http://packages.sil.org/debian/"
			addmirror "deb $LLSO $D $COMPONENTS"
			addmirror "deb $PSO $D $COMPONENTS"
			if [ $D != "wheezy" ]; then
				# allow to install current nodejs packages
				if [ -n "$update" ]; then
					# we can't use https when creating the chroot because apt-transport-https
					# isn't available yet
					addmirror "deb https://deb.nodesource.com/node_8.x $D main"
				fi
			fi
		else
			err "Unknown distribution $D. Please update the script $0."
			exit 1
		fi

		if [ -z "$update" ]; then
			options="--create --debootstrap debootstrap"
		else
			options="--update --override-config"
		fi

		if [[ $D == "bionic" && -z "$update" ]]; then
				# Hack to work around trouble with pbuilder --keyring giving error
				# "E: gnupg, gnupg2 and gnupg1 do not seem to be installed,
				# but one of them is required for this operation" on bionic. Build
				# a smaller base.tgz, and include gnupg. Manually add the
				# keyrings. Then let the base.tgz get updated with the othermirrors.

				sudo HOME=~ DIST=$D ARCH=$A pbuilder $options \
						${KEYRINGMAIN:+--debootstrapopts --keyring=}$KEYRINGMAIN \
						--extrapackages "apt-utils devscripts lsb-release apt-transport-https ca-certificates gnupg" \
						--mirror "$MIRROR" \
						--components "$COMPONENTS" \
						${PROXY:+--http-proxy }$PROXY

				echo >addkey '/usr/bin/apt-key add -'
				sudo HOME=~ DIST=$D ARCH=$A pbuilder --execute --save-after-exec -- addkey < $KEYRINGLLSO
				sudo HOME=~ DIST=$D ARCH=$A pbuilder --execute --save-after-exec -- addkey < $KEYRINGPSO
				sudo HOME=~ DIST=$D ARCH=$A pbuilder --execute --save-after-exec -- addkey < $KEYRINGNODE
				rm addkey

				options="--update --override-config"
		fi

		sudo HOME=~ DIST=$D ARCH=$A pbuilder $options \
			${KEYRINGMAIN:+--debootstrapopts --keyring=}$KEYRINGMAIN \
			${KEYRINGLLSO:+--keyring }$KEYRINGLLSO \
			${KEYRINGPSO:+--keyring }$KEYRINGPSO \
			${KEYRINGNODE:+--keyring }$KEYRINGNODE \
			--extrapackages "apt-utils devscripts lsb-release apt-transport-https ca-certificates gnupg tzdata" \
			--othermirror "$OTHERMIRROR" \
			--mirror "$MIRROR" \
			--components "$COMPONENTS" \
			${PROXY:+--http-proxy }$PROXY
	done
done
