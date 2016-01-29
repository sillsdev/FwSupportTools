#!/usr/bin/perl

use utf8;

# TestVblFmts.pl < messages.LG.po

# Check to see whether all the variables have the correct format, in the target strings of a .po file.

$logfile = "PuncTest.log";
open(LOGFILE, ">$logfile") or die;

# Are we in a msgid?
$in = 0;

while ($line = <>) {
	chomp $line;
	if ($line =~ /^$/) {
		# If this is the first blank, then process the previous chunk
		# (Assumes blank lines between each bundle!!  Maybe wrong assumption.)
		if ($msgid =~ /[^\"]/ && $msgstr =~ /[^\"]/) {
			$left = grep(/\{/, $msgid);
			$right = grep(/\}/, $msgid);
			$idvbls = $msgid;
			$idvbls =~ s/[^\{\}0-9]//g;
			#if ($msgid =~ /Click OK/) {
			#	print STDERR "  $idvbls\n";
			#	}
#print STDERR "id has $left open braces and $right close braces.\n";
			$strleft = grep(/\{/, $msgstr);
			$strright = grep(/\}/, $msgstr);
#print STDERR " str has $strleft opening and $strright closing braces.\n";
			$strvbls = $msgstr;
			$strvbls =~ s/[^\{\}0-9]//g;
			#if ($msgid =~ /Click OK/) {
			#	print STDERR "  $strvbls\n";
			#	}
			if ($left && ($strleft != $left)) {
				#print STDERR "Mismatch of opening braces in $msgstr for $msgid\n";
				#print LOGFILE "Mismatch of opening braces in $msgstr for $msgid\n";
				}
			if ($right && ($strright != $right)) {
				#print STDERR "  Mismatch of closing braces in $msgstr for $msgid\n";
				#print LOGFILE "  Mismatch of closing braces in $msgstr for $msgid\n";
				}
			if ($idvbls ne $strvbls) {
				print STDERR "Different variables in msgstr $strvbls from msgid $idvbls\t\t($msgid)\n";
				print LOGFILE "Different variables in msgstr $strvbls from msgid $idvbls\t\t($msgid)\n";
				}

			# Now clear out what we've been remembering
			$msgid = "";
			$msgstr = "";
			$in = 0;
			}
		}
	elsif ($line =~ /^msgid (\".*\")/) {
		$msgid = $1;
		$in = 1;
		}
	elsif ($line =~ /^(\".*\")/ && $in) {
		$msgid .= $1;
		}
	elsif ($line =~ /^msgstr (\".*\")/) {
		$in = 0;
		$msgstr = $1;
		}
	elsif ($line =~ /^(\".*\")/ && !$in) {
		$msgstr .= $1;
		}

	}
