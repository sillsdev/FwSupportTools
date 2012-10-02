Note for compiling WriteKey

After building a release version, if you need to use the WriteKey.exe.manifest file generated in
C:\\WW\Bin\src\MonoRegistryTools\WriteKey\bin\Release, it must be edited to remove the first requestedPrivileges node. See previous sample in WW\Bin. (The build process inserts info from C:\\WW\Bin\src\MonoRegistryTools\WriteKey\WriteKey.exe.manifest into the default manifest it builds, but doesn't do a good enough job in the merge process.)

Also, to make the program appear safer to users, have Alistair sign the compiled WriteKey.exe file to indicate that it is a FieldWorks Registry Updater program signed by SIL International.

Then check the modified WriteKey.exe and WriteKey.exe.manifest into C:\WW\Bin.
