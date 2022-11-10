#!/bin/bash

# setup.sh
# Setup or update mirrors

set -e -o pipefail

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

KEYRINGLLSO="$PBUILDERDIR/llso-keyring-2013.gpg"
KEYRINGPSO="$PBUILDERDIR/pso-keyring-2016.gpg"
KEYRINGMICROSOFT="$PBUILDERDIR/microsoft.asc.gpg"
KEYRING_MONO="$PBUILDERDIR/mono-project.asc.gpg"

if [ ! -f ${KEYRINGPSO} ]; then
	wget --output-document=${KEYRINGPSO} https://packages.sil.org/keys/pso-keyring-2016.gpg
fi

if [ ! -f ${KEYRINGLLSO} ]; then
	wget --output-document=${KEYRINGLLSO} http://linux.lsdev.sil.org/keys/llso-keyring-2013.gpg
fi

if [ ! -f ${KEYRINGMICROSOFT} ]; then
	wget -qO- https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor -o ${KEYRINGMICROSOFT}
fi

if [ ! -f "${KEYRING_MONO}" ]; then
	TMP_KEYRING="$(mktemp)"
	XAMARIN_KEY_FINGERPRINT="3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF"
	gpg --trust-model always --no-default-keyring --keyring "${TMP_KEYRING}" \
		--keyserver keyserver.ubuntu.com --recv-keys "${XAMARIN_KEY_FINGERPRINT}"
	gpg --no-default-keyring --keyring "${TMP_KEYRING}" --export > "${KEYRING_MONO}"
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

		# packages.microsoft is a 64-bit only repo. 32-bit can be downloaded as a tar.
		MICROSOFT_APT="deb [arch=amd64] http://packages.microsoft.com/repos/microsoft-ubuntu-${D}-prod ${D} main"
		MONO_APT="deb http://download.mono-project.com/repo/ubuntu vs-${D} main"

		if [[ "$UBUNTU_DISTROS $UBUNTU_OLDDISTROS" == *$D* ]]; then
			if [[ $UBUNTU_DISTROS == *$D* ]]; then
				MIRROR="${UBUNTU_MIRROR:-http://archive.ubuntu.com/ubuntu/}"
			else
				MIRROR="${UBUNTU_OLDMIRROR:-http://old-releases.ubuntu.com/ubuntu/}"
			fi
			COMPONENTS="main universe multiverse"
			KEYRINGMAIN="/usr/share/keyrings/ubuntu-archive-keyring.gpg"
			PROXY="$http_proxy"
			for S in "" "-backports" "-updates"; do
				addmirror "deb $MIRROR $D$S $COMPONENTS"
			done
			LLSO="http://linux.lsdev.sil.org/ubuntu/"
			PSO="http://packages.sil.org/ubuntu/"
			for S in "" "-proposed" "-updates" "-experimental"; do
				addmirror "deb $LLSO $D$S $COMPONENTS"
				addmirror "deb $PSO $D$S $COMPONENTS"
			done
			addmirror "${MICROSOFT_APT}"
			addmirror "${MONO_APT}"
		elif [[ $DEBIAN_DISTROS == *$D* ]]; then
			MIRROR="${DEBIAN_MIRROR:-http://ftp.ca.debian.org/debian/}"
			COMPONENTS="main contrib non-free"
			KEYRINGMAIN="/usr/share/keyrings/debian-archive-keyring.gpg"
			PROXY="$http_proxy"
			LLSO="http://linux.lsdev.sil.org/debian/"
			PSO="http://packages.sil.org/debian/"
			addmirror "deb $LLSO $D $COMPONENTS"
			addmirror "deb $PSO $D $COMPONENTS"
			addmirror "${MICROSOFT_APT}"
			addmirror "${MONO_APT}"
		else
			err "Unknown distribution $D. Please update the script $0."
			exit 1
		fi

		if [ -z "$update" ]; then
			options="--create --debootstrap debootstrap"
		else
			options="--update --override-config"
		fi

		sudo HOME=~ DIST=$D ARCH=$A pbuilder $options \
			--debootstrapopts --include="perl gnupg ca-certificates" \
			${KEYRINGMAIN:+--debootstrapopts --keyring=}$KEYRINGMAIN \
			${KEYRINGLLSO:+--keyring }$KEYRINGLLSO \
			${KEYRINGPSO:+--keyring }$KEYRINGPSO \
			${KEYRINGMICROSOFT:+--keyring }$KEYRINGMICROSOFT \
			${KEYRING_MONO:+--keyring }$KEYRING_MONO \
			--extrapackages "apt-utils devscripts lsb-release apt-transport-https ca-certificates tzdata libdistro-info-perl" \
			--othermirror "$OTHERMIRROR" \
			--mirror "$MIRROR" \
			--components "$COMPONENTS" \
			${PROXY:+--http-proxy }$PROXY
	done
done
