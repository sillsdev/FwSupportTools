#!/bin/bash

init()
{
	CONFIGFILE="$(dirname $0)/build-multi.config"
	[ -e $CONFIGFILE ] && . $CONFIGFILE # Settings for PBUILDERDIR, DISTRIBUTIONS, etc.

	# currently supported and future Ubuntu versions
	UBUNTU_DISTROS="devel trusty xenial bionic cosmic disco eoan focal groovy hirsute impish jammy kinetic"
	# no longer supported Ubuntu versions that live in old-releases.ubuntu.com
	UBUNTU_OLDDISTROS=""
	# We're no longer building packages for: precise quantal raring saucy utopic vivid wily yakkety zesty artful

	# Debian versions
	DEBIAN_DISTROS="wheezy jessie stretch buster bullseye bookworm trixie"

	# set default values in case we don't have a config file or the config file
	# doesn't set all variables
	PBUILDERDIR=${PBUILDERDIR:-$HOME/pbuilder}
	DISTRIBUTIONS=${DISTRIBUTIONS:-$UBUNTU_DISTROS $UBUNTU_OLDDISTROS $DEBIAN_DISTROS}
	ARCHES=${ARCHES:-amd64 i386}

	UBUNTU_MIRROR=${UBUNTU_MIRROR:-http://archive.ubuntu.com/ubuntu/}
	UBUNTU_OLDMIRROR=${UBUNTU_OLDMIRROR:-http://old-releases.ubuntu.com/ubuntu/}

	PBUILDERSUDO=${PBUILDERSUDO:-}

	DIST_ARCH_SEP=${DIST_ARCH_SEP:-/}

	RED='\033[0;31m'
	GREEN='\033[0;32m'
	NC='\033[0m' # No Color
}

err() {
	echo -e "${RED}$1${NC}"
}

log() {
	echo -e "${GREEN}$1${NC}"
}
