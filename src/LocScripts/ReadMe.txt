LocScripts

Scripts for working with po files as part of the localization of the FieldWorks UI.
Beth Bryson
1/29/16

The scripts in this folder are ones I have used as I prepare files for localizers
to translate and then prepare those files to be checked in to source.  This file
lists the scripts and gives a brief description of when I use them and for what
purpose.  It also includes the steps I do when managing localization files.

Usage:  Most of the scripts have a Usage statement in the source file, but it
doesn't print out at the command line.  Please look for this statement before
using a script.  Many have input and output file paths hardcoded in the script,
and those need to be adjusted for your setup.

------------
Localization process:

1. To generate a POT file:
cd fwrepo\fw\Bin
LocaleStrings.exe -r C:\fwrepo\fw -x
	(Creates a file in Bin called FieldWorks.POT.  This is the catalog that
	contains all the localizable strings and that can be used to update po files.)

2. What I did to adjust a POT file before giving it to a localizer:
a. Convert it to Unix line endings if it is not already.   dos2unix.pl
b. Move all the TE strings to the end by using rm-TE.pl and then adding them back
in at the end of the resulting file.
c. Remove the unlocalizable strings.   rm-dontloc.pl
d. Merge with the prioritized po file, messages.en-ca.po_, in order to get
priorities marked.  merge-po4.pl

3. To start a po file for a language:
(Download PoEdit.  Be sure to use version 1.4.6, not any later version.  Later
versions wrap strings and make diffs nearly impossible._
Run PoEdit
File/New Catalog from POT file
Select FieldWorks.pot  (or something created from it by means of the steps in
#2 and the scripts listed below)
Save it as messages.lg.po, where "lg" is the two-letter code for your language.
Has to be a code that Microsoft recognizes as a UI language.

4. For a localizer to edit an existing po file:
Open messages.lg.po in PoEdit.
Look for the strings you want to translated (e.g., all untranslated ones, or
all that are prioritized as ^1^ (will show up in the "translated" section).
Type translations into the translation box.
Look for fuzzy strings; try to resolve all of those before sending for the
localization manager to check in to source.
Save the file.  Email it to the localization manager  (FLEx_Localization@sil.org)

5. When a .po file comes back from a localizer:
a. Check for fuzzy strings.  Decide if I can accept or modify any of the translations;
remove the rest.   rm-fuzzy.pl
b. Remove any untranslated prioritized strings.  rm-priority.pl
c. Test for strings that could cause a crash:  TestColorFmts.pl, TestSpaces.pl, TestVblFmts.pl
d. Once all of that is good, check it in to fwrepo\fw\Localizations
e. Build the localizations:
	 cd fwrepo\fw\Build
	 build /t:localize /p:config=release

------------
Files are listed in roughly the order I might use them, more or less.

dos2unix.pl: Converts from DOS line endings to Unix line endings.
Perl script run much better on files that have only a single character marking
the end of a line (Unix, Mac) instead of two (DOS).

diff-po4.pl:  For finding out what strings have been added.
Compares two po files and outputs the result of "subtracting" all
the msgid strings in one from the other.  Output is two files: one with all the
strings in the first file that are not in the second, and another is the intersection
of the strings.  I usually run it twice, swapping the order of the files.

rm-TE.pl:  Removes strings related to TE, for producing a PO file that can be
localized by someone who would need the SE version.
This is probably more aggressive in deciding that something is a TE
string than the option in localestrings.exe is.
I also used this to move all the TE strings to the end of the file.  I would pull
them out, and then add them back in at the bottom of the file, as an aid in
prioritizing.
This script will become obsolete with FW 9.0.

messages-DONTLOC.po:  Strings that are not localizable.
In theory, strings that are not localizable shouldn't make it into the POT files,
but they do.  This is a list of the ones I think should not be localized.  They
are not in a specific order.  [They should probably be alphabetized.]

rm-dontloc.pl: Remove strings that are not localizable.
Removes all the strings in messages-DONTLOC.po from a po file.
**The path for that file is hardcoded in the script; needs to be adjusted.
The Chinese and French .po files include all of these; the other po files don't.

countmsgs.sh: Shell script for counting how many msgid lines in a file.
Useful for doing a sanity check--is it roughly the right number?  Or if I have
done a subtraction or a merge, do the numbers of the resulting files add up as
I would expect?

messages.en-ca.po_:  Po file with strings marked for priority for localizing.
[This file is currently checked in to the Localizations folder/project.]
This is a po file in which the most important strings have been copied into the
msgstr field with a priority coded prepended.  ^1^ marks the highest priority of
strings to translate, ^2^ second priority, and so on.  Some codes mark a group of
strings rather than priority (e.g., Notebook strings).  Strings with lowest
priority have not been copied; their msgstr is blank.  This file needs to be
manually updated periodically.  Near the top it tells when it was last edited,
and the date of the POT file it was based on.

FW8.NotesOnPrior.txt: Notes about which functions are in which priority group.
I'm attempting to put all commands related to a specific task or function together.
This overlaps some with a goal of having a given view either all translated or
not translated, but not completely.

merge-po4.pl: Merge a prioritized .po file with a partially translated pofile, to
produce a file for a localizer.  It leaves alone the strings they have already
translated, and adds the priority code to strings they have not translated.
There is a Usage statement near the top of the source file.

split-prior-es.pl:  Split one prioritized .po file into several, based on priority.
This is useful for localizers who are less comfortable picking and choosing strings
from a file, or who are sharing the work among several.  If I can give them a
single file with all the "priority 1" strings, they can work to complete that
whole file.  Someone else could work on the "priority 2" strings, etc.
**This file is definitely out of date.  When I wrote it, I was marking the priorities
differently than I am now.  The regular expressions need to be updated to match
the new codes (^1^, ^2^, ^3^, etc., instead of  1, z, 3, ...)  Also, it is
hardcoded for Spanish (es).

rm-priority.pl: Blanks out strings with priority codes, in a file that has been
partially translated.
If a localizer works on a prioritized file and translates some of the strings
but not all, I have to remove the "fake translations" (the one with the priority
codes in them) before I can check it in to source control.

RemoveFuzzy.pl:  Removes the translations of strings marked fuzzy, and the #,fuzzy
comment, to produce a file that can be checked in to source control.
Since "fuzzy" strings are usually guesses from the program and are just "the
translation of a string that is similar to this one", they are almost never
accurate, and should not be checked in as is.  It is better if a localizer can
look at them and put a correct translation, but in the absence of that, it is
better to just delete the guesses.

TestColorFmts.pl: Looks for badly formatted color strings that could cause a crash.

TestSpaces.pl: Looks for mismatches in leading/trailing spaces between msgid and msgstr.

TestVblFmts.pl: Looks for mismatches in the number of parameters between msgid and
msgstr.  Also looks for a different order.  These need to be inspected by hand,
since it is normal to change the order in a translation.
One kind of false positive:  There are some strings with a parameter for the
linebreak, but the (French) translation doesn't have it.  It seems to work okay
in FLEx.  [Ideally we should either remove that parameter from the strings in FLEx
source code, or add them to the msgstr strings.]

FindDups.pl:  Finds duplicates in a .po file.
Other tools (PoEdit, LocaleStrings) can report that a file has a duplicate (and
then they refuse to do anything else with the file), but they don't report
what the duplicate is.  This one reports what they are, so they can be fixed.




joinlines.pl:  Used with SFM files.  Finds fields that span more than one line
and realigns them so each field occurs on only one line.  Makes further processing
easier.  Used primarily in import of SFM files, not with localization.
