# Instructions to create base box

Such as for creating fwdev-w1604 and fwtest-w1604.

Created 2017-12-01.

## Creating the base box

This assumes using an Ubuntu 16.04 host machine. These instructions were used for Wasta 18.04 and Ubuntu 18.04 guests.

### Install machine

1. Make sure you have at least 50 GB of disk space free, as you will end up with a few copies of the machine you are working on, in different forms.
* Use virtualbox manager to create a new Ubuntu virtual machine. (named something like fwtest-w1604-base or fwdev-w1604-base). Give 2000 MB RAM to a dev machine and 1500 MB RAM to a test machine. Give 60 GB dynamic storage. Note that the actual RAM used after creating the box will depend on the Vagrantfile setting.
* Boot new virtual machine, specifying appropriate Ubuntu or Wasta iso file.
* At the boot menu, specify to start installing right away (eg "Install Ubuntu"). (Press shift right away in Ubuntu to see options.)
* Use default keyboard. Don't download updates while installing (it may use a slow mirror and take forever). Don't install flash, etc.
* Choose "Something else" and make one big `/` partition with no swap.
* Set location to Chicago (near Dallas, for timezone and locale).
* Name vagrant, computer name something like fwtest-w1604 or fwdev-u1604, user vagrant, pass vagrant. Choose Log in automatically.
* Reboot after installer finishes.
* Check that your host clipboard ring doesn't have anything sensitive.
* Log in to guest.
* Make guest machine window bigger to avoid guest windows staying maximized after opening and closing them. This resolution will be remembered in the product, so don't make it too big either. Maybe no bigger than 1860 x 960. How about 1600x900.
* Upgrade using Software Updater. Then upgrade from Terminal: sudo apt update && sudo apt dist-upgrade
* Reboot to run any new kernel.

### Configure

1. Turn off screen blanking, locking, suspend.

* Remove anything from the panel that will just get in the way of use as a testing or dev machine, like thunderbird, libreoffice, vlc. On fwdev, remove all the launcher icons. On fwtest, add terminal and synaptic to dock/panel.

* Change the desktop background to something specific to this machine.

  * fwtest-w1604 - green grass
  * fwdev-w1604  - park bench
  * fwtest-w1804 - green mountain
  * fwdev-w1804  - tan wall and door
  * fwtest-u1804 - looking out of orange cave
  * fwdev-u1804  - bag of orange food
  * fwtest-w1804 -
  * fwdev-w1804  -
  * fwtest-u1804 - (temp: boat on lake)
  * fwdev-u1804  - (temp: stone path thru forest)

* Install guest additions.

  * On your host machine, run

        sudo apt install virtualbox-guest-additions-iso

  * In the guest machine, run

        sudo apt install linux-headers-$(uname -r) build-essential dkms

   In the guest machine window, choose Devices > Insert Guest Additions CD image. In the guest, a VBOXADDITIONS window appears; click Run. Right-click the CD icon on the panel or desktop and Eject the VirtualBox Additions disc.

   In the guest machine window, choose Devices > Shared clipboard > Bidirectional.

* Reboot.

* Passwordless sudo

        sudo tee /etc/sudoers.d/passwordless >/dev/null <<< 'vagrant ALL=(ALL) NOPASSWD: ALL'

* Ubuntu 18.04 uses a swapfile (rather than a swap partition). Swap shouldn't be necessary and may cause unnecessary churn when backing up the guest image. Disable and delete the swapfile. (Skip this for Ubuntu 16.04 and earlier, which did not default to swapfiles.)

        sudo swapoff -a && sudo perl -ni -e 'print unless /swapfile/' /etc/fstab && sudo rm -v /swapfile

* Install initial login key.

        mkdir -p ~/.ssh && chmod 0700 ~/.ssh
        wget  https://raw.githubusercontent.com/hashicorp/vagrant/master/keys/vagrant.pub  -O ~/.ssh/authorized_keys
        chmod 0600 ~/.ssh/authorized_keys

* Don't let deja-dup hassle user.

        sudo apt remove deja-dup

* Install some packages.

        sudo apt install -y synaptic gdebi vim meld openssh-server

* Turn off automatic updates, so that a user won't turn off the machine during updates, which may make a mess. In Wasta 16.04 or 18.04, launch Software Settings (or "Software & Updates" in Ubuntu 18.04), go to the Updates tab. 'Automatically check for updates' to 'Every two weeks'. Close and re-open the Software Settings window if needed to make 'When there are security updates' work. Change 'When there are security updates' to 'Download automatically'. Change 'When there are other updates' to 'Display every two weeks'. Change 'Notify me of..' to 'Never'.

  For fwtest, set "When there are security updates" to "Display immediately" to prevent the fwtest machine from starting downloads immediately upon boot, which gets in the way of the machine being used to immediately install software to test.

* Prepare for ssh host keys to be re-generated uniquely by users. (The following script is not indented in markdown to fix problems when pasting from this .md file.)

```
sudo tee /root/regenerate-ssh-host-keys >/dev/null << END
#!/bin/bash
# Regenerate ssh host keys if not present
test -f /etc/ssh/ssh_host_rsa_key || dpkg-reconfigure openssh-server
END
sudo chmod +x /root/regenerate-ssh-host-keys

sudo tee /etc/systemd/system/regenerate-ssh-host-keys.service >/dev/null << END
[Unit]
Description=regenerate-ssh-host-keys

[Service]
ExecStart=/root/regenerate-ssh-host-keys

[Install]
WantedBy=multi-user.target
END
sudo systemctl enable regenerate-ssh-host-keys
```

* For fwdev, create `~/Desktop/dev-machine-instructions.txt` saying:

```
Linux FieldWorks Development Machine

INTRODUCTION

This machine has FieldWorks and related repositories already cloned, in ~/fwrepo and ~/projects . Launchers for IDEs and other tools are available in the development-tools folder on the desktop. FieldWorks is pre-compiled and should run if you launch Visual Studio Code, click File - Open Workspace, open ~/fwrepo/fw/fw.code-workspace, wait a bit for extensions and possibly click Reload in the lower right of the Code window, and then click Debug - Start Debugging.

Your ssh private key, to commit to gerrit, should have been copied to ~/.ssh/id_rsa .

NEXT STEPS

Set your git author name, git email address, and gerrit username by doing the following:

In the following four lines, right here in this file replace YOUR_GERRIT_USERNAME, YOUR_GIT_AUTHOR_NAME, and YOUR_GIT_EMAIL_ADDRESS with your gerrit username, git author name, and git email address. Then paste the lines into a terminal. Leave GERRIT_USER_PLACEHOLDER alone; don't replace it with anything.

	git config --global fwinit.gerrituser YOUR_GERRIT_USERNAME
	git config --global user.name YOUR_GIT_AUTHOR_NAME
	git config --global user.email YOUR_GIT_EMAIL_ADDRESS
	cd ~/fwrepo && find -path '.*\.git/config' | xargs perl -pi -e "s/GERRIT_USER_PLACEHOLDER/$(git config --get fwinit.gerrituser)/"

An old NuGet.exe might be in the way. You can remove it before building FieldWorks if you are aware that it will cause a problem:

	rm ~/fwrepo/fw/Build/NuGet.exe

All git repositories were cloned with shallow history, to not take up as much space. If you want to search back in history in one of your git repositories, you can first deepen the history by running:

	git fetch --unshallow

MONODEVELOP

MonoDevelop 5 would debug FieldWorks 9, but that's no longer easily installable.
MonoDevelop 7 is installed in this vagrant, and can be jostled into running FW, but it is not a smooth experience.
For now you can use Visual Studio Code until we adjust FW for MonoDevelop 7.

To set up MonoDevelop: Launch monodevelop. Choose File - Open, fwrepo/fw/RunFieldWorks.csproj. Choose Edit - Preferences. Click .NET Runtimes. Click Add. Click /opt/mono4-sil and Set as Default. Click OK.

If MonoDevelop later forgets about the runtime setting, then in Preferences set the .NET Runtimes default runtime to the latest mono in /opt .

To debug FW, launch Monodevelop, click File - Open, open ~/fwrepo/fw/RunFieldWorks.csproj, and click Run - Start Debugging. On the Outdated Build dialog, click Execute. The next time you load RunFieldWorks in Monodevelop, it may complain about MSBuild, but you may be able to just add RunFieldworks.csproj to the solution again, and then run.

To compile FieldWorks, in MonoDevelop click the Tools menu and then click one of "fw build remakefw" or "fw build recent". fw build recent will build projects whose files have changed within the last 30 minutes, allowing you to make a change to a code file and press ALT-T-T to quickly build your change.

Note that if you switch between FW 9 and FW 8, you will need to switch the default .NET runtime between mono 4 and mono 3 in MonoDevelop, Edit, Preferences, Projects, .NET Runtimes.

PACKAGING

(Note: This feature is only partially tested.)

To use this machine to build packages, first run

	cd ~/pbuilder && DISTRIBUTIONS="bionic xenial" ./setup.sh

You can then build a package managed by build-packages by running a command such as

	~/fwrepo/FwSupportTools/packaging/build-packages --main-package-name flexbridge --dists "xenial" --arches "amd64"  --repository-committishes flexbridge=origin/master --simulate-dput |& tee /var/tmp/log

Or you can build a source and binary package by running commands such as

	cd someproject && debuild -uc -us -S -nc
	cd someproject/.. && sudo DISTRIBUTIONS=xenial ARCHES=amd64 ~/pbuilder/build-multi.sh source-package-name.dsc |& tee /var/tmp/log
```

### Provision

* This is a good time to take a snapshot of the guest, in case anything fails in the provision.

* For a test machine:

		cd && wget https://raw.githubusercontent.com/sillsdev/FwSupportTools/develop/vagrant/testing/provision-fw-test-machine
		bash provision-fw-test-machine

* For a dev machine:

		cd && wget https://raw.githubusercontent.com/sillsdev/FwSupportTools/develop/vagrant/development/provision-fw-dev-machine
		bash provision-fw-dev-machine

	After the script, reboot a couple times to finish all initial automated configuration.

* Make basebox version file, where 'name' is something like 'fwtest-u2004', 'creation date' is something like '2019-12-31', and 'installed from' mentions the installation media (or other base box) used to create the machine.

```
tee ~/machine-info.txt >/dev/null << END
Vagrant base box information
Name:
Creation date:
Installed from:
Notes:
END
```

### Finish configuration

1. Free up disk space

		sudo apt-get update && sudo apt-get autoremove && sudo apt-get clean

* For fwtest, after apt clean, pre-download FW and dependencies.

		sudo apt-get install -dy fieldworks

* Clear any clipboard rings in guest and host.

### Finalize

1. Delete the guest's ssh host keys, so they will be re-generated uniquely by users.

		sudo rm -v /etc/ssh/ssh_host_*

* Zero-out deleted files so they don't take up space shipping with the product. Note: This does not appear to make the .vdi file on the host grow in size, so it should be safe to do for guests that need more space than the host has available. A `cat: write error: No space left on device` is expected and not a problem.

		cat /dev/zero > ~/zeros; sync; ls -lh ~/zeros; rm -v ~/zeros

* Shutdown guest.

### Generate and publish product

1. Export VM .box file. This may take 5-15 minutes. The `--base` argument is the name of the base machine in virtualbox manager.

		date; time vagrant package --base fwdev-w1604-base --output fwdev-w1604-0.0.0.box

* Test that your new box is what you expect.

		mkdir test && cd test
		vagrant init ../fwdev-w1604-0.0.0.box
		vim Vagrantfile # Uncomment provider virtualbox section that enables `vb.gui`. Increase RAM.
		vagrant up

* Perform smoke tests as described below.

* Clean up box test machine. In the `test` directory, run the following. Then delete the `test` directory. The `vagrant box remove` command removes the internally stored copy of the base box (to free disk space); it's not removing the '../foo' *file*, but internally stored data with the designation of '../foo'.

		vagrant destroy
		vagrant box remove '../fwdev-w1604-0.0.0.box'

* Create a `.json` file to describe and version the box. (Use another box's .json file as a template.)

* Upload the `.json` and `.box` files to the server.

* Adjust permissions on the files on the server so it's easier for others to maintain them.

		chmod g+w *box *json

	If you created a new directory, also chmod g+x that new directory.

* If this is a new box, then you may want to create a new file somewhere like `FwSupportTools/vagrant/testing/fwtest-w1804/Vagrantfile`.

## Modifying the base box

1. Check that your host clipboard ring doesn't have anything sensitive.

* Using virtualbox manager, start the base virtual machine (eg fwdev-w1604-base or fwtest-w1604-base).

* Optionally apply updates.

		sudo apt update
		sudo apt dist-upgrade

* Make any modifications, such as to settings or adding new VCS repositories. Make equivalent modifications to the `provision-fw-dev-machine` script, as appropriate, so the changes will happen automatically next time.

* Decide whether or not to fetch and rebuild code from various VCS repositories. Fetching may add hundreds of megs to the box image (because they won't be entirely shallow checkouts).

* Follow the "Finish creation" and "Finalize" instructions above.

* Follow the "Generate and publish product" instructions above, but:

	1. Increase the version number of the box in the `--output` argument to `vagrant package`.

	* Edit the `.json` file on the server by appending a new element in the `versions` array.

	* Upload the new `.box` file, and delete the oldest `.box` file from the server (for disk space).

## Smoke tests

### fwdev

* Launch VSCode. Open FW workspace. Start debugging FW.
* Verify that you can start flexbridge from fieldworks, such as by clicking "Get project from colleague".
* Successfully rebuild FW by clicking Terminal - Run Build Task, with minimal unit test errors.

### fwtest

* Launch synaptic. Enable pso:main, pso:experimental, llso:main, llso:experimental. Install and launch fieldworks.

## Notes

* The base box can't seem to be compressed any better as a vagrant box image, such as by using xz instead of gzip.

* You can free up space by deleting old base boxes. `vagrant box prune`

* You can make snapshots along the way if you suspect the next operation might fail, or if you want to test something and rollback so your test isn't in the product.
