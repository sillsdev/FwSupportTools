#!/usr/bin/perl

use utf8;

# TestColorFmts.pl < messages.LG.po

# Check to see whether all the color labels have the correct format, in a .po file.

$logfile = "colortest.log";
open(LOGFILE, ">$logfile") or die;

# Is this string a color?
$color = 0;

while ($line = <>) {
	chomp $line;
	if ($line =~ /^$/) {
		# Blank line; clear out what we've been remembering
		# (Assumes blank lines between each bundle!!  Maybe wrong assumption.)
		$msgid = "";
		$msgstr = "";
		$fmt = 0;
		$color = 0;
		$thiscolor = "";
		}
	elsif ($line =~ /^#\./) {
		# Blank line; clear out what we've been remembering
		# If this is a color string, remember
		if ($line =~ /ColorStrings.resx/) {
			$color = 1;
			}
		}
	elsif ($line =~ /^msgid/) {
		if ($color) {
			if ($line =~ /msgid \"([^,]+)(,[0-9]+,[0-9]+,[0-9]+)\"$/) {
				$thiscolor = $1;
				$fmt = $2;
				}
			else {
				print STDERR "   Unexpected format for color in: $line\n";
				}
			}
		}
	elsif ($line =~ /^msgstr \"(.+)\"/) {
		if ($color) {
			if ($line =~ /msgstr \"([^,]+)$fmt\"$/) {
				print LOGFILE "Valid color: $1\t$thiscolor\n";
				}
			else {
				print STDERR "Invalid color: $line\t$thiscolor\t$fmt\n";
				print LOGFILE "Invalid color format: $line\t$thiscolor\t$fmt\n";
				}
			}
		}

	}
