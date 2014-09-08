#!/bin/bash

init()
{
	CONFIGFILE="build-multi.config"
	[ -e $CONFIGFILE ] && . $CONFIGFILE # Settings for PBUILDERDIR, DISTRIBUTIONS, etc.

	# set default values in case we don't have a config file or the config file
	# doesn't set all variables
	PBUILDERDIR=${PBUILDERDIR:-$HOME/pbuilder}
	DISTRIBUTIONS=${DISTRIBUTIONS:-precise saucy trusty utopic wheezy jessie}
	ARCHES=${ARCHES:-amd64 i386}
	UBUNTU_MIRROR=${UBUNTU_MIRROR:-http://archive.ubuntu.com/ubuntu/}
}
