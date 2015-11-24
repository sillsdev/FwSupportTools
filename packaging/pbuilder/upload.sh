#!/bin/bash
. $(dirname $0)/common.sh
init

set -e

NOOP=
DISTROCOMPONENT=-experimental
REPO=llso

help()
{
	echo "Usage:"
	echo "$(basename $0) [-n|-nn] [--component <experimental|main>] [--repo <llso|pso>] <package.dsc>"
	echo ""
	echo "-n "
	echo "	Run script without doing the actual upload"
	echo "-nn "
	echo "	Run script without doing the actual upload and print commands"
	echo ""
	echo "--component <experimental|main|proposed|updates>"
	echo "	Where the package should go: experimental, proposed, updates, or main section."
	echo "	Default: experimental"
	echo ""
	echo "--repo <llso|pso>"
	echo "	Repo to upload to. Default: llso"
	exit 0
}

# Process arguments.
while (( $# )); do
	case $1 in
		# Process individual arguments here. Use shift and $1 to get an argument value.
		-n)  NOOP=: ;;
		-nn) NOOP=echo ;;
		--component)
			if [ "$2" == "experimental" ]; then
				DISTROCOMPONENT=-experimental
			elif [ "$2" == "proposed" ]; then
				DISTROCOMPONENT=-proposed
			elif [ "$2" == "updates" ]; then
				DISTROCOMPONENT=-updates
			elif [ "$2" == "main" ]; then
				DISTROCOMPONENT=
			else
				echo "Error: Unknown component \"$2\". Exiting"
				exit 2
			fi
			shift
			;;
		--repo) REPO=$2; shift;;
		--help) help;;
		*)
			if [ $# -eq 1 ]; then
				break
			else
				echo "Error: Unexpected argument \"$1\". Exiting."
				exit 1
			fi
			;;
	esac
	shift
done

if [ $# -eq 0 ]; then
	echo "Error: No .dsc file specified. Exiting."
	exit 3
fi

for SRC
do
	PACKAGE=$(basename "$SRC" .dsc)

	for D in $DISTRIBUTIONS
	do
		echo "D=$D"
		if [[ "$UBUNTU_DISTROS $UBUNTU_OLDDISTROS" == "*$D*" ]]; then
			DISTRO=ubuntu
			COMPONENT=$DISTROCOMPONENT
		elif [[ $DEBIAN_DISTROS == *$D* ]]; then
			DISTRO=debian
			COMPONENT=
		else
			echo "Unknown distribution $D. Please update the script $0"
			exit 1
		fi

		for A in $ARCHES
		do
			echo "Processing $D${DIST_ARCH_SEP}$A"
			RESULT="$PBUILDERDIR/$D${DIST_ARCH_SEP}$A/result"

			if [ ! -d "$RESULT" ]
			then
				continue
			fi

			F1="${PACKAGE}_${A}"
			F2="${PACKAGE}+${D}1_${A}"

			if [ -e "$RESULT/${F2}.changes" ]; then
				CHANGES="$RESULT/${F2}.changes"
				FULLPACKAGE=$F2
			else
				CHANGES="$RESULT/${F1}.changes"
				FULLPACAKGE=$F1
			fi

			if [ ! -f "$RESULT/$FULLPACKAGE.$REPO.upload" ]; then
				$NOOP debsign $DEBSIGNKEY $CHANGES
				$NOOP dput $REPO:$DISTRO/$D$COMPONENT $CHANGES
			else
				echo "Package $FULLPACKAGE already uploaded. Ignoring."
			fi
		done
	done
done
