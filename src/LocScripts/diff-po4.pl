#!/usr/bin/perl

use utf8;

# diff-po.pl bigfile.po smallfile.po extrafile.po

if ($#ARGV != 3) {
	print STDERR "Usage: diff-po.pl bigfile.po smallfile.po extrafile.po samefile.po\n";
	print "[0] = $ARGV[0]\n";
	print "[1] = $ARGV[1]\n";
	print "[2] = $ARGV[2]\n";
	print "[3] = $ARGV[3]\n";
	exit;
	}
else {
	$bigfile = $ARGV[0];
	$smallfile = $ARGV[1];
	$extrafile = $ARGV[2];
	$samefile = $ARGV[3];
	open(BIGFILE, "<$bigfile") or die;
	open(SMALLFILE, "<$smallfile") or die;
	open(EXTRAFILE, ">$extrafile") or die;
	open(SAMEFILE, ">$samefile") or die;

	#print STDERR "Opening $bigfile, $smallfile, $extrafile.\n";
	}
binmode(BIGFILE, "utf8");
binmode(SMALLFILE, "utf8");
binmode(EXTRAFILE, "utf8");
binmode(SAMEFILE, "utf8");

$i = 0;
#iterate through SMALLFILE, collecting all the msgid strings
while ($line = <SMALLFILE>) {
	chomp $line;
	if ($line =~ /^msgid \"(.*)\"/) {
		$ids[$i] = $1;
		$in = 1;
		}
	elsif ($line =~ /^\"(.*)\"/ && $in) {
		$ids[$i] .= $1;
		}
	elsif ($line =~ /^msgstr/) {
		#$ids[$i] =~ s/([\(\)\[\]\$\{\}\*\?\"\\])/\\$1/g;
		$in = 0;
		$i++;
		}
	}


$line = <BIGFILE>;
# Start by printing all the stuff before the first real record
while ($line !~ /^#\./) {
	chomp $line;
	print EXTRAFILE "$line\n";
	$line = <BIGFILE>;
	chomp $line;
	}

# iterate through the big file, reading one line each time through the loop,
# until there are no more lines
while ($line = <BIGFILE>) {
	# remove the end-of-line character, whatever it is
	chomp $line;
	if ($line =~ /^$/) {
		# If this is the end of a clump; process the previous one.
		if (!$blank) {
			# See if it's in the small file
			$teststr =~ s/([\(\)\[\]\$\{\}\*\+\|\?\"\\])/\\$1/g;
			#print STDERR "[$teststr]\n";
			if (grep(/^$teststr$/, @ids)) {
				# found
				#print STDERR "Match: $teststr\n";
				# for verification, print this record to a logfile
				foreach $cm (@cmt) {
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
				#print STDERR " Doesn't match: $teststr\n";
				# not found, so print this record
				foreach $cm (@cmt) {
					print EXTRAFILE "$cm\n";
					}
				print EXTRAFILE "msgid \"$msgid[0]\"\n";
				if ($#msgid) {
					#print STDERR " mpl $#msgid: $msgid[1]\n";
					for $m (1..$#msgid) {
						#print STDERR "  mpl: $msgid[$m]\n";
						print EXTRAFILE "\"$msgid[$m]\"\n";
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
close SMALLfile;
close EXTRAFILE;
