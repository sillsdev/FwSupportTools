#!/usr/bin/perl

# dos2unix.pl
# Change DOS line ends to Unix ones.

while (<>) {
	$_ =~ s/\r\n/\n/g;
	print $_;
	}
