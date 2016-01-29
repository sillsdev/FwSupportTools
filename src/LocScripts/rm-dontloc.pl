#!/usr/bin/perl

use utf8;

# rm-dontloc.pl ORIGFILE.pot messages-DONTLOC.po NEWFILE.pot

# Remove strings that don't need to be localized.
# The strings are stored in a separate file; that file needs to be updated as
# more are discovered or added.

if ($#ARGV != 1) {
	print STDERR "Usage: rm-dontloc.pl origfile.pot newfile.pot\n";
	print "ARG[0] = $ARGV[0]\n";
	print "ARG[1] = $ARGV[1]\n";
	exit;
	}
#if ($#ARGV != 2) {
#	print STDERR "Usage: rm-dontloc.pl origfile.pot messages-DONTLOC.po newfile.pot\n";
#	print "ARG[0] = $ARGV[0]\n";
#	print "ARG[1] = $ARGV[1]\n";
#	print "ARG[2] = $ARGV[2]\n";
#	exit;
#	}
else {
	$bigfile = $ARGV[0];
	$smallfile = "/Users/beth/Dev/LocScripts/messages-DONTLOC.po";
	$extrafile = $ARGV[1];
	$samefile = "samefile.po";
	open(BIGFILE, "<$bigfile") or die;
	open(SMALLFILE, "<$smallfile") or die;
	open(EXTRAFILE, ">$extrafile") or die;
	open(SAMEFILE, ">$samefile") or die;

	#print STDERR "Opening $bigfile, $smallfile, $extrafile.\n";
	}
print STDERR "Removing strings found in $smallfile\n";
binmode(BIGFILE, "utf8");
binmode(SMALLFILE, "utf8");
binmode(EXTRAFILE, "utf8");
binmode(SAMEFILE, "utf8");

# array for storing all the msgid's.  Index increments each time we see a new msgid.
# Each element of the array has to be the concatenation of all the lines in one msgid.
@ids = ();
$i = 0;

#iterate through SMALLFILE, collecting all the msgid strings
while ($line = <SMALLFILE>) {
	chomp $line;
	# msgid line
	if ($line =~ /^msgid (\".*\")$/) {
		$ids[$i] = $1;
		#print STDERR "Storing ids: [$ids[$i]]\n";
		#$i++;
		$in = 1;
		}
	# Continuation of msgid (not msgstr)
	elsif ($line =~ /^(\".*\")/ && $in) {
		$string = $1;
		$ids[$i] .= $string;
		}
	# msgstr line
	elsif ($line =~ /^msgstr/) {
		#$ids[$i] =~ s/([\(\)\[\]\$\{\}\*\?\"\\])/\\$1/g;
		#if ($idstr =~ /A programming/) {
			#print STDERR " Fixing ids: [$ids[$i]]\n";
		#	}
		$in = 0;
		$i++;
		}
	# No need to store anything for blank lines or comments
	elsif ($line =~ /^\s*$/ || $line =~ /^#\. /) {
		}
	else {
		print STDERR "None of the above: [$line]\n";
		}
	}

print STDERR "Stored $i strings\n";

# We're going to reuse this index below.
$i = 0;

$line = <BIGFILE>;
chomp $line;
# Start by printing all the stuff before the first real record
while ($line !~ /^#/) {
	print EXTRAFILE "$line\n";
	$line = <BIGFILE>;
	chomp $line;
	}
# Need to store this line as part of the first comment
$cmt[$i] = $line;
$i++;

# iterate through the big file, reading one line each time through the loop,
# until there are no more lines
## Preserves manual comments in BIGFILE, if they are on msgid's that are getting preserved.
## Any manual comment on a string that is removed, is also removed.
while ($line = <BIGFILE>) {
	# remove the end-of-line character, whatever it is
	chomp $line;
	if ($line =~ /^$/) {
		# If this is the end of a clump; process the previous one.
		if (!$blank) {
			# See if it's in the small file
			$teststr =~ s/([\(\)\[\]\$\{\}\*\+\|\?\"\\])/\\$1/g;
			#print STDERR "teststr = [$teststr]\n";
			if (grep(/^$teststr$/, @ids)) {
				# found
				#print STDERR "Match: $teststr\n";
				# for verification, print this record to a logfile
				foreach $cm (@cmt) {
					#print STDERR "$#cmt cmts; cmt $#cmt = [$cmt[$#cmt]]\n";
					print SAMEFILE "$cm\n";
					}
				print SAMEFILE "msgid \"$msgid[0]\"\n";
				if ($#msgid) {
					#print STDERR " mpl $#msgid: $msgid[1]\n";
					for $m (1..$#msgid) {
						#print STDERR "  mpl: $msgid[$m]\n";
						print SAMEFILE "\"$msgid[$m]\"\n";
						}
					}
				foreach $ms (@msgstr) {
					print SAMEFILE "$ms\n";
					}
				print SAMEFILE "\n";

				}
			else {
				#print STDERR "  Doesn't match: $teststr\n";
				#print STDERR " ids[0]: $ids[0]\n";
				# not found, so print this record
				foreach $cm (@cmt) {
					print EXTRAFILE "$cm\n";
					if ($cm =~ /^# /) {
						print STDERR "YYManual: [$cm]\n";
						#print STDERR "[[$cmt[$i]]]\n";
						}
					}
				print EXTRAFILE "msgid $msgid[0]\n";
				if ($#msgid) {
					#print STDERR " mpl $#msgid: $msgid[1]\n";
					for $m (1..$#msgid) {
						#print STDERR "  mpl: $msgid[$m]\n";
						print EXTRAFILE "$msgid[$m]\n";
						}
					}
				foreach $ms (@msgstr) {
					print EXTRAFILE "$ms\n";
					}
				print EXTRAFILE "\n";
				}

			# Reset the arrays and counters
			$teststr = "";
			undef @cmt;
			$i = 0;
			undef @msgid;
			$j = 0;
			undef @msgstr;
			$k = 0;
			#Next few lines might be blank
			$blank = 1;
			}
		else {
			# do just print extra blank lines
			#print "\n";
			}
		}
	# For actual records, collect the info
	# comment line; may signal the first part of a new record
	elsif ($line =~ /^#/) {
		# Record that we've come to the end of the blank lines between records
		$blank = 0;
		$cmt[$i] = $line;
		if ($line =~ /^# /) {
			print STDERR "Manual comment: [$line]\n";
			print STDERR "$i [[$cmt[$i]]]\n";
			}
		$i++;
		}
	# msgid line:  the string in English
	elsif ($line =~ /^msgid (\".*\")/) {
		$msgid[0] = $1;
		$teststr .= $msgid[0];
		$j++;
		}
	# msgstr line: the translated string
	elsif ($line =~ /^msgstr/) {
		$msgstr[0] = $line;
		$k++;
		}
	elsif ($line =~ /^(\".*\")/) {
		if ($msgstr[0]) {
			$msgstr[$k] = "$1";
			$k++;
			}
		else {
			$msgid[$j] = $1;
			$teststr .= $msgid[$j];
			$j++;
			}
		}
	}

close BIGFILE;
close SMALLfile;
close EXTRAFILE;
