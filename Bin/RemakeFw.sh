#!/bin/sh
# Completely rebuild FieldWorks (Bounds, Debug, or Release version) and test databases
# First parameter is build configuration: d (default), b, r
# Second parameter is build action: build (default), test, clean

CONFIG=
if   [ "$1" = "r" ]; then CONFIG=/property:config=release;
elif [ "$1" = "b" ]; then CONFIG=/property:config=bounds;
fi

ACTION=
if   [ "$2" = "test" ]; then ACTION=/property:action=test
elif [ "$2" = "clean" ]; then ACTION=/property:action=clean
fi

. $FWROOT/environ
cd $FWROOT/Build
echo '$1="'$1'"'
echo '$2="'$2'"'
echo CONFIG=$CONFIG
echo ACTION=$ACTION
xbuild /t:remakefw $CONFIG $ACTION
