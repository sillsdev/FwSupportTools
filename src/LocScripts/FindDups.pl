#!/usr/bin/perl #-CS

use utf8;

# FindDups.pl workingfile.po

# Check a .po file for duplicate keys
# (Still has a lot of stuff that was needed for merge-po4.pl that probably isn't
# really needed here.)

# Created:	 2 Nov 2010
# Modified:

if ($#ARGV != 0) {
	print STDERR "Usage: FindDups.pl workingfile.po\n";
	print "[0] = $ARGV[0]\n";
	print "[1] = $ARGV[1]\n";
	print "[2] = $ARGV[2]\n";
	exit;
	}
else {
	$wkgfile = $ARGV[0];
	open(WKGFILE, "<$wkgfile") or die;

	print STDERR "Opening $wkgfile.\n";
	}
binmode(WKGFILE, "utf8");
binmode(STDOUT, "utf8");

$line = <WKGFILE>;
chomp $line;
#print stderr "First line: [$line]\n";
# Start by consuming all the stuff before the first real record
while ($line !~ /^#\./) {
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
# Hash for remembering whether we've seen this key or not
%str = ();

#iterate through WKGFILE, collecting all the msgid strings and corresponding msgstr strings
# store a modified form of them in the hash %str
while ($line = <WKGFILE>) {
	chomp $line;
	if ($line =~ /^msgid (\".*\")$/) {
		#$ids[$i] =~ s/([\(\)\[\]\$\{\}\*\+\|\?\"\\])/\\$1/g;
		#print STDERR "   $ids[$i]\n";
		$i++;
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

		# Make a hash, to remember whether we've seen this msgid yet
		# (We'll add to this if this msgstr has more lines.)
		# This is for printing, and so it needs to have newlines.
		# This includes all the quote marks, so when checking for translations, need to account for that.

		if ($str{$idstr}) {
			print STDERR "Duplicate: $str{$idstr}\n";
			}
		else {
			$str{$idstr} = 1;
			#print STDERR " Storing str: [$str{$idstr}]\n";
			}

		$k++;
		# We've left msgid
		$in = 0;
		}
	elsif ($line =~ /^(\".*\")$/) {
		$string = $1;
		# If we've started storing the lines of a msgid, then this continues that.
		if ($in) {
			#print STDERR "continuation: $line\n";
			$ids[$i] .= $string;
			$idstr = $ids[$i];
			#if ($idstr =~ /A programming/) {
			#	print STDERR "\nStoring idstr: [$idstr]\n";
			#	}
			}
		}
	# No need to store anything for blank lines or comments or memory lines
	elsif ($line =~ /^\s*$/ || $line =~ /^#\. / || $line =~ /^#~/) {
		}
	elsif ($line =~ /^#,/) {
		$fuzzy = 1;
		}
	else {
		print STDERR "None of the above: [$line]\n";
		}
	}
