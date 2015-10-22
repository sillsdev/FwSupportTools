#!/bin/bash

init()
{
	CONFIGFILE="build-multi.config"
	[ -e $CONFIGFILE ] && . $CONFIGFILE # Settings for PBUILDERDIR, DISTRIBUTIONS, etc.

	# currently supported and future Ubuntu versions
	UBUNTU_DISTROS="precise trusty vivid wily xenial"
	# no longer supported Ubuntu versions that live in old-releases.ubuntu.com
	UBUNTU_OLDDISTROS="quantal raring saucy utopic"
	# Debian versions
	DEBIAN_DISTROS="wheezy jessie"

	# set default values in case we don't have a config file or the config file
	# doesn't set all variables
	PBUILDERDIR=${PBUILDERDIR:-$HOME/pbuilder}
	DISTRIBUTIONS=${DISTRIBUTIONS:-$UBUNTU_DISTROS $UBUNTU_OLDDISTROS $DEBIAN_DISTROS}
	ARCHES=${ARCHES:-amd64 i386}

	UBUNTU_MIRROR=${UBUNTU_MIRROR:-http://archive.ubuntu.com/ubuntu/}
	UBUNTU_OLDMIRROR=${UBUNTU_OLDMIRROR:-http://old-releases.ubuntu.com/ubuntu/}

	PBUILDERSUDO=${PBUILDERSUDO:-}
}
