#!/usr/bin/perl

use utf8;

# merge-po4.pl workingfile.po catalog.po missing.po newfile.po

# Merge translations from a working .po file into a catalog file.
# Creates a new .po file consisting of all and only the strings in the catalog.
# If the .po file has translations for any of these strings, include those.
# If the catalog has "translations" (they would really be annotations), then
# put those in the new file only for strings without translatoins in working file.
# Use user comments from working file and auto comments from catalog.
# Use all the strings in the catalog.  Ignore strings that are only in working.

# To do: Adjust so we pull the initial msgstr (with header info for the catalog)
# from the catalog, not from workingfile.po.

# Created:
# Modified:
# Modified:	26 Jan 2010	bb	Keep the "fuzzy" annotation on fuzzy strings!!!
# Modified: 26 Apr 2011 bb	Keep *all* comments (#) not just the auto ones (#.)

if ($#ARGV != 2) {
	print STDERR "Usage: merge-po4.pl catalog.po workingfile.po newfile.po\n";
	print STDERR "Merges translations from workingfile.po into catalog.po,\n";
	print STDERR "retaining any \"translations\" in catalog.po for strings\n";
	print STDERR "that don't have a translation in workingfile.po.\n";
	print STDERR "Initial msgstr should be modified by hand to reflect\n";
	print STDERR "correct header info for the catalog.\n";
	print "[0] = $ARGV[0]\n";
	print "[1] = $ARGV[1]\n";
	print "[2] = $ARGV[2]\n";
	exit;
	}
else {
	$catfile = $ARGV[0];
	$wkgfile = $ARGV[1];
	$newfile = $ARGV[2];
	#$extrafile = $ARGV[3];
	open(WKGFILE, "<$wkgfile") or die;
	open(CATFILE, "<$catfile") or die;
	open(NEWFILE, ">$newfile") or die;
	#open(EXTRAFILE, ">$extrafile") or die;

	print STDERR "Opening $wkgfile, $catfile, $newfile.\n";
	}
binmode(WKGFILE, "utf8");
binmode(CATFILE, "utf8");
binmode(NEWFILE, "utf8");
#binmode(EXTRAFILE, "utf8");

$line = <WKGFILE>;
chomp $line;
#print stderr "First line: [$line]\n";
# Start by printing all the stuff before the first real record
while ($line !~ /^#/) {
	#print NEWFILE "$line\n";
	#print STDERR "$line\n";
	$line = <WKGFILE>;
	chomp $line;
	}
#print STDERR "First comment.\n";


# array for storing all the msgid's.  Index increments each time we see a new msgid.
# Each element of the array has to be the concatenation of all the lines in one msgid.
@ids = ();
$i = 0;
# Hash for remembering what to output for each
%strs = ();

#iterate through WKGFILE, collecting all the msgid strings and corresponding msgstr strings
# store a modified form of them in the hash %str
while ($line = <WKGFILE>) {
	chomp $line;
	if ($line =~ /^msgid (\".*\"$)/) {
		#$ids[$i] =~ s/([\(\)\[\]\$\{\}\*\+\|\?\"\\])/\\$1/g;
		#print STDERR "   $ids[$i]\n";
		$i++;
		# array for storing the msgstr entries (not sure what I'm using this for yet, other than to know we have a msgstr)
		@strs = ();
		$k = 0;
		#print STDERR "idstr was: [$idstr]\n";
		$ids[$i] = $1;
		# special string to store the concatenation of the lines
		$idstr = "$ids[$i]";
		# flag to say we're in msgid, not anywhere else
		$in = 1;
		}
	elsif ($line =~ /^msgstr/) {
		# Store the first line of msgstr
		$strs[0] = $line;
		# Fix the concatenation of the id lines, so it can be used with grep
		$idstr =~ s/([\(\)\[\]\$\{\}\*\+\|\?\"\\])/\\$1/g;
		# Make a hash, to remember which msgstr goes with which msgid
		# (We'll add to this if this msgstr has more lines.)
		# This is for printing, and so it needs to have newlines.
		# This includes all the quote marks, so when checking for translations, need to account for that.
		$str{$idstr} = "$line\n";
		#print STDERR " 1Storing str: [$str{$idstr}]\n";
		$k++;
		# We've left msgid
		$in = 0;
		# Now set the "fuzzy" hash, since now we have $idstr calculated for this record.
		if ($fuzzy) {
			$fuzzy{$idstr} = 1;
			print STDERR "Fuzzy line: $idstr \n";
			$fuzzy = 0;
			}
		# And set the comment hash as well
		if ($comt) {
				print stderr "Storing [$comt]\n";
				$comment{$idstr} = $comt;
				$comt = "";
			}
		}
	elsif ($line =~ /^(\".*\")$/) {
		$string = $1;
		# If we've started storing the lines of a msgstr, then this continues that.
		if ($strs[0]) {
			$strs[$k] = "$string\n";
			$k++;
			# Add to the special concatenated string that's in the hash
			$str{$idstr} .= "$line\n";
			#print STDERR " .Storing str: [$str{$idstr}]\n";
			}
		else {
			#print STDERR "continuation: $line\n";
			$ids[$i] .= $string;
			$idstr = $ids[$i];
			if ($idstr =~ /A programming/) {
				#print STDERR "\nStoring idstr: [$idstr]\n";
				}
			}
		#$ids[$i] .= $1;
		}
	# Store user comments
	elsif ($line =~ /^# /) {
		# Record that we've come to the end of the blank lines between records
		$blank = 0;
		if ($comt) {
			$comt .= "\n$line";
			}
		else {
			$comt = $line;
			}
		print "  comt: [$line]\n";
		}
	elsif ($line =~ /fuzzy/) {
		$fuzzy = 1;
		#print stderr "fuzzy: [$line]\n";
		}
	# No need to store anything for blank lines or comments
	elsif ($line =~ /^\s*$/) {
		}
	else {
		#print STDERR "None of the above: [$line]\n";
		}
	}


# We're going to reuse this index below.
$i=0;

# Now work on the catalog file, the authority file
$line = <CATFILE>;
# Start by just outputting all the stuff before the first real record
while ($line !~ /^#/) {
	chomp $line;
	print NEWFILE "$line\n";
	$line = <CATFILE>;
	chomp $line;
	}
# Still need to print this line.
print NEWFILE "$line\n";

# Flag to indicate the previous line was a blank line
$blank = 0;

# We will print something for each message:
#  - If translated in working file, keep that.
#  - Else, if there is a target string in the catalog, print that.
#  - Else, make an empty msgstr.
while ($line = <CATFILE>) {
	chomp $line;
	if ($line =~ /^$/) {
		# If this is the end of a clump; process the previous one.
		if (!$blank) {
			# See if it's also in the working file
			$teststr =~ s/([\(\)\[\]\$\{\}\*\+\|\?\"\\])/\\$1/g;
			#print STDERR "[$teststr]\n";
			if (grep(/^$teststr$/, @ids)) {
				# found
				#print STDERR "Match: $teststr\n";
				# print these to the output file, with "working"
				# string, or catalog string, if "working" string was empty
				# Use the user comments from the working file
				if ($comment{$teststr}) {
					print stderr "have cmts\n";
					print NEWFILE "$comment{$teststr}\n";
					}
				# Use the automatic comments from the catalog file
				foreach $cm (@cmt) {
					print NEWFILE "$cm\n";
					}
				if ($fuzzy{$teststr}) {
					print NEWFILE "#, fuzzy\n";
					}
				print NEWFILE "msgid $msgid[0]\n";
				if ($#msgid) {
					#print STDERR " mpl $#msgid: $msgid[1]\n";
					for $m (1..$#msgid) {
						#print STDERR "  mpl: $msgid[$m]\n";
						print NEWFILE "$msgid[$m]\n";
						}
					}

				#print STDERR " Multiline msgstr? $#msgstr\n";
				# If it was translated in the working file, print that
				if ($str{$teststr} !~ /msgstr \"\"\n$/) {
					#print STDERR "  Translated str: [$str{$teststr}]\n$teststr";
					print NEWFILE "$str{$teststr}\n";
					}
				# otherwise print multiline strings from catalog
				elsif ($#msgstr) {
					foreach $ms (@msgstr) {
						print NEWFILE "$ms\n";
						}
					print NEWFILE "\n";
					#@msgstr = ();
					}
				# as well as single line ones.
				elsif ($msgstr[0] ne "msgstr \"\"") {
					print NEWFILE "$msgstr[0]\n\n";
					}
				# otherwise make it blank
				else {
					print NEWFILE "msgstr \"\"\n\n";
					}
				#print NEWFILE "\n";

				}
			else {
				#print STDERR " Doesn't match: $teststr\n";
				# not found, so just print this record as is
				#print STDERR " Not found: [$teststr]\n";
				foreach $cm (@cmt) {
					print NEWFILE "$cm\n";
					}
				print NEWFILE "msgid $msgid[0]\n";
				if ($#msgid) {
					#print STDERR " mpl $#msgid: $msgid[1]\n";
					for $m (1..$#msgid) {
						#print STDERR "  mpl: $msgid[$m]\n";
						print NEWFILE "$msgid[$m]\n";
						}
					}
				foreach $ms (@msgstr) {
					print NEWFILE "$ms\n";
					}
				print NEWFILE "\n";
				}

			# Reset the arrays and counters
		#print STDERR "teststr was: [$teststr]\n";
			$teststr = "";
			undef @cmt;
			undef @comts;
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
	elsif ($line =~ /^#[\.]/) {
		# Record that we've come to the end of the blank lines between records
		$blank = 0;
		$cmt[$i] = $line;
		if ($line =~ /^#\s/) {print "  cmt: [$line]\n"}
		$i++;
		}
	# Keep the "fuzzy" annotation!!
	elsif ($line =~ /^#,/) {
		$fuzzy = 1;
		}
	# msgid line:  the string in English
	elsif ($line =~ /^msgid (\".*\")$/) {
		$msgid[0] = $1;
		$teststr = "$msgid[0]";
		$j++;
		}
	# msgstr line: the translated string
	elsif ($line =~ /^msgstr/) {
		$msgstr[0] = $line;
		$k++;
		}
	# continuation line
	elsif ($line =~ /^(\".*\")/) {
		if ($msgstr[0]) {
			$msgstr[$k] = "$1";
			$k++;
			}
		else {
			$msgid[$j] = $1;
			$teststr .= $msgid[$j];
			#print STDERR " $j\n";
			$j++;
			}
		}
	}

# Process the last record
# (I realize this is bad coding, to just copy this much code
# from inside the loop.  There must be a way to do this as part
# of the above loop.)
if (!$blank) {
	# See if it's also in the working file
	$teststr =~ s/([\(\)\[\]\$\{\}\*\+\|\?\"\\])/\\$1/g;
	#print STDERR "[$teststr]\n";
	if (grep(/^$teststr$/, @ids)) {
		# found
		#print STDERR "Match: $teststr\n";
		# print these to the output file, with "working"
		# string, or catalog string, if "working" string was empty
		# Use the user comments from the working file
		if ($comment{$teststr}) {
			print stderr "have cmts\n";
			print NEWFILE "$comment{$teststr}\n";
			}
		# Use the automatic comments from the catalog file
		foreach $cm (@cmt) {
			print NEWFILE "$cm\n";
			}
		if ($fuzzy{$teststr}) {
			print NEWFILE "#, fuzzy\n";
			}
		print NEWFILE "msgid $msgid[0]\n";
		if ($#msgid) {
			#print STDERR " mpl $#msgid: $msgid[1]\n";
			for $m (1..$#msgid) {
				#print STDERR "  mpl: $msgid[$m]\n";
				print NEWFILE "$msgid[$m]\n";
				}
			}

		#print STDERR " Multiline msgstr? $#msgstr\n";
		# If it was translated in the working file, print that
		if ($str{$teststr} !~ /msgstr \"\"\n$/) {
			#print STDERR "  Translated str: [$str{$teststr}]\n$teststr";
			print NEWFILE "$str{$teststr}\n";
			}
		# otherwise print multiline strings from catalog
		elsif ($#msgstr) {
			foreach $ms (@msgstr) {
				print NEWFILE "$ms\n";
				}
			print NEWFILE "\n";
			#@msgstr = ();
			}
		# as well as single line ones.
		elsif ($msgstr[0] ne "msgstr \"\"") {
			print NEWFILE "$msgstr[0]\n\n";
			}
		# otherwise make it blank
		else {
			print NEWFILE "msgstr \"\"\n\n";
			}
		#print NEWFILE "\n";

		}
	else {
		#print STDERR " Doesn't match: $teststr\n";
		# not found, so just print this record as is
		#print STDERR " Not found: [$teststr]\n";
		foreach $cm (@cmt) {
			print NEWFILE "$cm\n";
			}
		print NEWFILE "msgid $msgid[0]\n";
		if ($#msgid) {
			#print STDERR " mpl $#msgid: $msgid[1]\n";
			for $m (1..$#msgid) {
				#print STDERR "  mpl: $msgid[$m]\n";
				print NEWFILE "$msgid[$m]\n";
				}
			}
		foreach $ms (@msgstr) {
			print NEWFILE "$ms\n";
			}
		print NEWFILE "\n";
		}

	# Reset the arrays and counters
#print STDERR "teststr was: [$teststr]\n";
	$teststr = "";
	undef @cmt;
	undef @comts;
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


close WKGFILE;
close CATFILE;
#close EXTRAFILE;
close NEWFILE;
