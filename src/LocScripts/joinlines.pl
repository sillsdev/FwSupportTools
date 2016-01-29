#!/usr/bin/perl

# joinlines.pl
# Find any fields that span multiple lines and put them onto a single line.
# bb 8 Mar 2004

# Doesn't do other consistency checking that is needed:
# - Make sure there are no blank lines in the middle of records.
# - Make sure the file ends with a blank line.

# Count how many lines were joined, for verification.
$i = 0;

# Read in first line
$prevline = <>;

# Check beginning of each successive line, and join with previous if not a new SFM
while ($curline = <>) {
	if ($curline =~ /^$/) { # end of record
		print $prevline;
		$prevline = $curline;
		}
	elsif ($curline !~ /^\\/) { # continuation of an SFM; join it
		chop($prevline);
		$prevline = $prevline . " " . $curline;
		$i++;
		}
	else { # new SFM; print previous line
		print $prevline;
		$prevline = $curline;
		}
	}
# Print the final line.
print $prevline;
# For good measure, put a blank line at the end.
print "\n";

print STDERR "$i lines were combined with the previous one.\n";
