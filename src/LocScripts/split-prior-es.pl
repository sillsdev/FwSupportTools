#!/usr/bin/perl

use utf8;

# split-prior.pl ORIGFILE.pot

# Split one prioritized POT file into multiple subfiles.
# The criteria is hard-coded:
# Look for msgstr fields that begin with 1,z,3 and
# put into files with the same name as the original, but with
# -1, -2, -3, or -0 appended to the filename.
# Leaves the English in the msgstr
# (for use with Lingobit).

# Customized for the Spanish .po files:
# - Put English into the msgstr of any string that has not been translated.
# - Check for Spanish strings that have 1 or @ in front, but have already been translated.

if ($#ARGV != 0) {
	print STDERR "Usage: split-prior.pl origfile.pot\n";
	print "ARG[0] = $ARGV[0]\n";
	#print "ARG[1] = $ARGV[1]\n";
	#print "ARG[2] = $ARGV[2]\n";
	exit;
	}
else {
	$bigfile = $ARGV[0];
	$onefile = "$bigfile" . "-1";
	$twofile = "$bigfile" . "-2";
	$threefile = "$bigfile" . "-3";
	$fivefile = "$bigfile" . "-5";
	$checkfile = "$bigfile" . "-CHECK";
	$donefile = "$bigfile" . "-DONE";
	open(BIGFILE, "<$bigfile") or die;
	open(ONEFILE, ">$onefile") or die;
	open(TWOFILE, ">$twofile") or die;
	open(THREEFILE, ">$threefile") or die;
	open(FIVEFILE, ">$fivefile") or die;
	open(CHECKFILE, ">$checkfile") or die;
	open(DONEFILE, ">$donefile") or die;

	#print STDERR "Opening $bigfile, $onefile, $twofile, $threefile, $fivefile.\n";
	}
binmode(BIGFILE, "utf8");
binmode(ONEFILE, "utf8");
binmode(TWOFILE, "utf8");
binmode(THREEFILE, "utf8");
binmode(FIVEFILE, "utf8");
binmode(CHECKFILE, "utf8");
binmode(DONEFILE, "utf8");


# Start by printing all the stuff before the first real record
# (Do I need this in all the files?)
$line = <BIGFILE>;
while ($line !~ /^#\./) {
	print FIVEFILE "$line\n";
	$line = <BIGFILE>;
	chomp $line;
	}

$outstring = "";

# Now process the file, looking at msgstr and deciding based on that,
# which file to write the whole clump to.
# iterate through the big file, reading one line each time through the loop,
# until there are no more lines
while ($line = <BIGFILE>) {
	# remove the end-of-line character, whatever it is
	chomp $line;
	if ($line =~ /^$/) {
		# If this is the end of a clump; process the previous one.
		if (!$blank) {
			foreach $cm (@cmt) {
				#print STDERR "$#cmt cmts; cmt $#cmt = [$cmt[$#cmt]]\n";
				$outstring = sprintf "$cm\n";
				}
			$outstring .= sprintf "msgid \"$msgid[0]\"\n";
			if ($#msgid) {
				#print STDERR " mpl $#msgid: $msgid[1]\n";
				for $m (1..$#msgid) {
					#print STDERR "  mpl: $msgid[$m]\n";
					$outstring .= sprintf "\"$msgid[$m]\"\n";
					}
				}
			#print STDERR "msgstr = [$msgstr[0]]\n";
			# TODO:  If msgid also starts with 1, don't treat this as a 1.
			if ($msgstr[0] =~ /^msgstr \"1(.*)\"/) {
				$msgstr[0] = "msgstr \"$1\"";
				if ($msgid[0] =~ /^msgid \"1/) {
					$which = 5;
					print STDERR "msgid begins with 1: $msgid[0]\n";
					}
				else {
					$which = "1";
					}
				}
			# The @ and 0 ones were done but need more checking.
			elsif ($msgstr[0] =~ /^msgstr \"@(.*)\"/) {
				$msgstr[0] = "msgstr \"@" . "$1\"";
				$which = "CHECK";
				}
			elsif ($msgstr[0] =~ /^msgstr \"0(.*)\"/) {
				$msgstr[0] = "msgstr \"0" . "$1\"";
				$which = "CHECK";
				}
			elsif ($msgstr[0] =~ /^msgstr \"z(.*)\"/) {
				$msgstr[0] = "msgstr \"$1\"";
				$which = "2";
				}
			elsif ($msgstr[0] =~ /^msgstr \"3(.*)\"/) {
				$msgstr[0] = "msgstr \"$1\"";
				$which = "3";
				}
			else {
				# For the ones without a mark, see if it's been translated
				if ($msgstr[0] =~ /^msgstr \".+\"$/) {
					$which = "DONE";
					}
				else {
					$which = "5";
					}
				}

			foreach $ms (@msgstr) {
				$outstring .= sprintf "$ms\n";
				}
			$outstring .= sprintf "\n";
			#print STDERR "  outstring = $outstring";

			#print STDERR "  which = $which\n";
			if ($which eq "1") {
				print ONEFILE $outstring;
				}
			elsif ($which eq "2") {
				print TWOFILE $outstring;
				}
			elsif ($which eq "3") {
				print THREEFILE $outstring;
				}
			elsif ($which eq "5") {
				print FIVEFILE $outstring;
				}
			elsif ($which eq "CHECK") {
				print CHECKFILE $outstring;
				}
			elsif ($which eq "DONE") {
				print DONEFILE $outstring;
				}


			# Reset the arrays and counters
			$outstring = "";
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
	elsif ($line =~ /^#\./) {
		# Record that we've come to the end of the blank lines between records
		$blank = 0;
		$cmt[$i] = $line;
		$i++;
		}
	# msgid line:  the string in English
	elsif ($line =~ /^msgid \"(.*)\"/) {
		$msgid[0] = $1;
		$teststr .= $msgid[0];
		$j++;
		}
	# msgstr line: the translated string
	elsif ($line =~ /^msgstr/) {
		$msgstr[0] = $line;
		$k++;
		}
	elsif ($line =~ /^\"(.*)\"/) {
		if ($msgstr[0]) {
			$msgstr[$k] = "\"$1\"";
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
close ONEFILE;
close TWOFILE;
close THREEFILE;
close FIVEFILE;
close CHECKFILE;
close DONEFILE;
