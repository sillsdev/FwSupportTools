#!/usr/bin/perl

use utf8;

# rm-prior.pl < infile.po > outfile.po

# For strings where the msgstr has priority codes in it,
# make the msgstr blank.


$diff = 0;
# iterate through the file, reading one line each time through the loop,
# until there are no more lines
while ($line = <>) {
	# remove the end-of-line character, whatever it is
	chomp $line;
	if ($line =~ /^$/) {
		# If this is the end of a clump; process the previous one.
		if (!$blank) {
			# See if it's a prioritized clump
			if (!$diff) {
				foreach $cm (@cmt) {
					print STDOUT "$cm\n";
					}
				print STDOUT "msgid ";
				foreach $mi (@msgid) {
					print STDOUT "$mi\n";
					}
				print STDOUT "msgstr \"\"\n";
				print STDOUT "\n";
				}
			else {
				foreach $cm (@cmt) {
					print STDOUT "$cm\n";
					}
				print STDOUT "msgid ";
				foreach $mi (@msgid) {
					print STDOUT "$mi\n";
					}
				print STDOUT "msgstr ";
				foreach $ms (@msgstr) {
					print STDOUT "$ms\n";
					}
				print STDOUT "\n";
				}

			# Reset the arrays and counters
			@cmt = ();
			$i = 0;
			@msgid = ();
			$j = 0;
			@msgstr = ();
			$k = 0;
			$eng = 0;
			$nonte = 0;
			$blank = 1;
			$eng = 0;
			$in = 0;
			$diff = 0;
			}
		}
	elsif ($line =~ /^#/) {
		$blank = 0;
		$cmt[$i] = $line;
		$i++;
		# We are in a comment
		$in = "c";
		}
	elsif ($line =~ /^msgid (\".*\")$/) {
		#$msgid[0] = $line;
		$msgid[0] = $1;
		$j++;
		# We are in a msgid
		$in = "m";
		}
	elsif ($line =~ /^msgstr (\".*\")/) {
		$msgstr[0] = $1;
		$k++;
		# We are in a msgstr
		$in = "s";
		if ($msgstr[0] =~ /^\"[13z]/) {
			$diff = 1;
			print STDERR "Stripping $msgstr[0]\n";
			}
		}
	elsif ($line =~ /^\"/) {
		#print STDERR "Continuation of $in: [$line]\n";
		if ($in eq "s") {
			# We're in a msgstr; remember this portion of the string.
			$msgstr[$k] = $line;
			if ($msgstr[$k] ne $msgid[$k]) {
				# Strings are different; remember this.
				$diff = 1;
				}
			$k++;
			}
		elsif ($in eq "m") {
			# We are in a msgid; store this line
			$msgid[$j] = $line;
			$j++;
			}
		else {
			print STDERR "String not in a msgid or msgstr: [$line]\n";
			}
		}
	}
