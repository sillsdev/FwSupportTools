#!/usr/bin/perl

use utf8;

# TestSpaces.pl < messages.LG.po

# Check to see whether target strings have the same number
# of trailing and leading spaces as the source.

$logfile = "SpaceTest.log";
open(LOGFILE, ">$logfile") or die;

# Are we in a msgid?
$in = 0;

while ($line = <>) {
	chomp $line;
	# If this is the first blank, then process the previous chunk
	# (Assumes blank lines between each bundle!!  Maybe wrong assumption.)
	if ($line =~ /^$/) {
		# If there is anything in both msgid and other than quote marks, run the tests
		# ($msgid includes the quote marks)
		if ($msgid =~ /[^\"]/ && $msgstr =~ /[^\"]/) {
			# Check for leading spaces
			if ($msgid =~ /^"( +)/) {
				$spaces = $1;
				$left = length($spaces);
				$spaces = "";
				}
			if ($msgstr =~ /^"( +)/) {
				$spaces = $1;
				$strleft = length($spaces);
				$spaces = "";
				}
			# Check for trailing spaces
			if ($msgid =~ /( +)"$/) {
				$spaces = $1;
				$right = length($spaces);
				$spaces = "";
				}
			if ($msgstr =~ /( +)"$/) {
				$spaces = $1;
				$strright = length($spaces);
				$spaces = "";
				}
#print STDERR "  id has $left leading and $right trailing spaces.\n";
#print STDERR "   str has $strleft leading and $strright trailing spaces.\n";
			if ($strleft != $left) {
			#if ($left && ($strleft != $left)) {
				print STDERR "Mismatch of leading spaces in $msgstr for $msgid\n";
				print LOGFILE "Mismatch of leading spaces in $msgstr for $msgid\n";
				}
			if ($strright != $right) {
			#if ($right && ($strright != $right)) {
				print STDERR "  Mismatch of trailing spaces in $msgstr for $msgid\n";
				print LOGFILE "  Mismatch of trailing spaces in $msgstr for $msgid\n";
				}

			# Now clear out what we've been remembering
			$msgid = "";
			$msgstr = "";
			$left = $right = $strleft = $strright = 0;
			$spaces = "";
			$in = 0;
			}
		# If this hasn't been translated, do nothing.
		else {
			#print STDERR "  Empty msgid or msgstr: [$msgid] [$msgstr]\n";
			}
		}
	elsif ($line =~ /^msgid (\".*\")/) {
		$msgid = $1;
		$in = 1;
#print STDERR "msgid = [$msgid]\n";
		}
	elsif ($line =~ /^(\".*\")/ && $in) {
		$msgid .= $1;
		}
	elsif ($line =~ /^msgstr (\".*\")/) {
		$in = 0;
		$msgstr = $1;
		}
	elsif ($line =~ /^(\".*\")/ && !$in) {
		$msgstr .= $1;
		}

	}
