#!/usr/bin/perl

use utf8;

# rm-TE.pl < infile.po > outfile.po

# Remove strings that are only in TE.
# Leave strings that are in both FLEx and TE.

$tefile = "messages-TE.po";
open(TEFILE, ">$tefile");

while ($line = <>) {
	chomp $line;
	if ($line =~ /^$/) {
		# If this is the end of a clump; process the previous one.
		if (!$blank) {
			# See if it's a TE clump
			foreach $str (@cmt) {
				if ($str =~/\/Src\/TE\//) {
					$te = 1;
					}
				elsif ($str =~/Scr/) {
					$te = 1;
					}
				elsif ($str =~/Quotation/) {
					$te = 1;
					}
				elsif ($str =~/PunctuationDlg/) {
					$te = 1;
					}
				elsif ($str =~/\/TeDll/) {
					$te = 1;
					}
				elsif ($str =~/\/FwPrintLayoutComponents/) {
					$te = 1;
					}
				# Some specific strings from the strings file
				# kstidBibleNode, kstidNtNode, kstidOtNode
				elsif ($str =~/kstid[^ ]+Node/) {
					$te = 1;
					}
				elsif ($str =~/kstidDivName/) {
					$te = 1;
					}
				elsif ($str =~/NonUnicodeFileError/) {
					$te = 1;
					}
				elsif ($str =~/UnableToFindEditorialChecks/) {
					$te = 1;
					}
				elsif ($str =~/kstidIsolated/) {
					$te = 1;
					}
				elsif ($str =~/PuncPatternTipSpace/) {
					$te = 1;
					}
				elsif ($msgid[0] =~/Scripture/) {
					$te = 1;
					}
				elsif ($msgid[0] =~/scripture/) {
					$te = 1;
					}
				elsif ($msgid[0] =~/biblical/) {
					$te = 1;
					}
				elsif ($msgid[0] =~/Biblical/) {
					$te = 1;
					}
				elsif ($msgid[0] =~/Translation Editor/) {
					$te = 1;
					}
				elsif ($str =~ /^#\. \//) {
					$nonte = 1;
					}
				}
			if ($te and !$nonte) {
				foreach $cm (@cmt) {
					print TEFILE "$cm\n";
					#print STDERR "$cm\n";
					}
				foreach $mi (@msgid) {
					print TEFILE "$mi\n";
					}
				foreach $ms (@msgstr) {
					print TEFILE "$ms\n";
					}
				print TEFILE "\n";
				}
			else {
				foreach $cm (@cmt) {
					if ($cm !~ /[sS]cripture/ && $cm !~ /[Bb]iblical/ && $cm !~ /[Vv]ersification/ && $cm !~ /\/TE\// && $cm !~ /\/TeDll\//) {
						print STDOUT "$cm\n";
						}
					}
				foreach $mi (@msgid) {
					print STDOUT "$mi\n";
					}
				foreach $ms (@msgstr) {
					print STDOUT "$ms\n";
					}
				print STDOUT "\n";
				}

			# Reset the arrays and counters
			@cmt = ();
			$i = 0;
			@msgid = ();
			$j = 0;
			@msgstr = ();
			$k = 0;
			$te = 0;
			$nonte = 0;
			$blank = 1;
			}
		}
	elsif ($line =~ /^#/) {
		$blank = 0;
		$cmt[$i] = $line;
		$i++;
		}
	elsif ($line =~ /^msgid/) {
		$msgid[0] = $line;
		$j++;
		}
	elsif ($line =~ /^msgstr/) {
		$msgstr[0] = $line;
		$k++;
		}
	elsif ($line =~ /^\"/) {
		if ($msgstr[0]) {
			$msgstr[$k] = $line;
			$k++;
			}
		else {
			$msgid[$j] = $line;
			$j++;
			}
		}
	}
