#!/usr/bin/perl

use utf8;

# RemoveFuzzy.pl < messages-LG-fuzzy.po > messages-LG.po

# Find translations marked as "fuzzy".
# Change the msgstr to "" for these, since these are often misleading translations.

binmode(STDIN, "utf8");
binmode(STDOUT, "utf8");
binmode(STDERR, "utf8");


$removed = 0;
$fuzzycount = 0;
# iterate through the original file, reading one line each time through the loop,
# until there are no more lines
while ($line = <>) {
	# remove the end-of-line character, whatever it is
	chomp $line;
	if ($line =~ /^msgstr/) {
		# These are the only ones we have to treat differently.
		# Remember that we're in a msgstr
		$in = 1;
		if ($fuzzy) {
			# Remove the translation for this one
			print "msgstr \"\"\n";
			#print STDERR "Removing [$line]\n";
			# Reset the arrays and counters
			$fuzzycount++;
			$fuzzy = 0;
			}
		else {
			# Otherwise keep the line as is
			print "$line\n";
			}
		}

	elsif ($line =~ /^\s*$/) {
		# reset at the beginning of a new clump
		$fuzzy = 0;
		$in = 0;
		print "$line\n";
		}
	elsif ($line =~ /^#, fuzzy/) {
		$fuzzy = 1;
		# Don't print this line either, because it won't be fuzzy after we remove them.
		$removed++;
		}
	elsif ($line =~ /^\"(.*)\"/) {
		if ($in && $fuzzy) {
			# print nothing
			#$removed++;
			}
		else {
			print "$line\n";
			}
		}
	else {
		print "$line\n";
		}
	}

print STDERR "Removed $fuzzycount fuzzy strings\n";
print STDERR "Removed $removed lines.\n";
