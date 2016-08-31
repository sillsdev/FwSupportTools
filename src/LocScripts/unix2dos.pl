#!/usr/bin/perl

# unix2dos.pl
# Change Unix line ends to DOS ones.

while (<>) {
	$_ =~ s/\n/\r\n/g;
	print $_;
	}
