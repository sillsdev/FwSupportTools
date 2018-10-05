# Instructions to create base box

For fwdev-w1604 and fwtest-w1604.

Created 2017-12-01.

## Creating the base box

This assumes using an Ubuntu 16.04 host machine. These instructions were used for Wasta 16.04 and Ubuntu 18.04 guests.

### Install machine

1. Make sure you have at least 40 GB of disk space free, as you will end up with a few copies of the machine you are working on, in different forms.
* Use virtualbox manager to create a new Ubuntu virtual machine. (named something like fwtest-w1604-base or fwdev-w1604-base). Give 60 GB dymanic storage (for fwdev, or 50 for fwtest). 0.75 GB ram is too small, give 2048 MB RAM for fwdev or 1536 for fwtest.
* Boot new virtual machine, specifying wasta 16.04 or 18.04 iso file.
* At wasta grub menu, specify to start installing right away.
* Use default keyboard. Don't download updates while installing (it may use a slow mirror and take forever). Don't install flash, etc.
* Choose "Something else" and make one big `/` partition with no swap.
* Set location to near Dallas (for timezone and locale).
* Name vagrant, computer name fwtest-w1604 or fwdev-w1604, user vagrant, pass vagrant. Choose Log in automatically.
* Reboot after installer finishes.
* Check that your host clipboard ring doesn't have anything sensitive.
* Log in to guest.

### Configure

1. Turn off screen blanking and locking.

* Remove anything from bottom panel that will just get in the way of use as a testing or dev machine, like thunderbird, libreoffice, vlc. On fwdev, remove all the launcher icons. On fwtest, add terminal and synaptic to dock/panel.

* Change the desktop background to something specific to this machine.

  * fwtest-w1604 - green grass
  * fwdev-w1604 - park bench
  * fwtest-w1804 - green island
  * fwdev-w1804 - lighthouse

* You may need to wait 20 minutes for the machine's automated package upgrading to finish before apt will work.

* Install guest additions.

		sudo apt install linux-headers-$(uname -r) build-essential dkms
		wget http://download.virtualbox.org/virtualbox/5.2.0/VBoxGuestAdditions_5.2.0.iso
		# Record for security
		sha256sum *iso>VBoxGuestAdditionsUsed.sha256
		sudo mount -o loop,ro VBoxGuestAdditions_5.2.0.iso /mnt
		sudo sh /mnt/VBoxLinuxAdditions.run
		sudo umount /mnt
		rm VBoxGuestAdditions_5.2.0.iso

* Reboot.

* In virtualbox, choose Devices > Shared clipboard > Bidirectional.

* Install initial login key.

		mkdir -p ~/.ssh
		chmod 0700 ~/.ssh
		wget  https://raw.githubusercontent.com/hashicorp/vagrant/master/keys/vagrant.pub  -O ~/.ssh/authorized_keys
		chmod 0600 ~/.ssh/authorized_keys

* Passwordless sudo

		sudo visudo
		append: vagrant ALL=(ALL) NOPASSWD: ALL

* Don't let deja-dup hassle user.

		sudo apt remove deja-dup

* Install some packages, and apply updates.

		sudo apt update
		sudo apt install -y synaptic gdebi vim meld openssh-server
		sudo apt dist-upgrade

* Turn off automatic updates, so that a user won't turn off the machine during updates, which may make a mess. In Wasta 16.04, launch Software Settings, go to the Updates tab. Change 'When there are security updates' to 'Download automatically'. Change 'When there are other updates' to 'Display every two weeks'. Change 'Notify me of..' to 'Never'.

  For fwtest, set "When there are security updates" to "Display immediately" to prevent the fwtest machine from starting downloads immediately upon boot, which gets in the way of the machine being used to immediately install software to test.

* Prepare for re-generating ssh host keys by appending this line to `/etc/rc.local`. If /etc/rc.local doesn't exist yet, then prepend `#!/bin/bash` and chmod +x it.

		test -f /etc/ssh/ssh_host_rsa_key || dpkg-reconfigure openssh-server

* For fwdev, create `~/Desktop/dev-machine-instructions.txt` saying:

	Linux FieldWorks Development Machine

	This machine has FieldWorks and related repositories already cloned, in ~/fwrepo and ~/projects . Launchers for IDEs and other tools are available in the development-tools folder on the desktop. FieldWorks is pre-compiled and should run if you launch Monodevelop, open RunFieldWorks, and run. On the Outdated Build dialog, click Execute.

	Your ssh private key, to commit to gerrit, should have been copied to ~/.ssh/id_rsa .

	Next steps:

	Set your git author name, git email address, and gerrit username by doing the following:

	In the following four lines, replacing YOUR_GERRIT_USERNAME, YOUR_GIT_AUTHOR_NAME, and YOUR_GIT_EMAIL_ADDRESS with your gerrit username, git author name, and git email address. (Leave GERRIT_USER_PLACEHOLDER alone.) Then paste the lines into a terminal.

	    git config --global fwinit.gerrituser YOUR_GERRIT_USERNAME
	    git config --global user.name YOUR_GIT_AUTHOR_NAME
	    git config --global user.email YOUR_GIT_EMAIL_ADDRESS
	    cd ~/fwrepo && find -path '.*\.git/config' | xargs perl -pi -e "s/GERRIT_USER_PLACEHOLDER/$(git config --get fwinit.gerrituser)/"

	Note that to compile FieldWorks, in MonoDevelop click the Tools menu and then click one of "fw build remakefw" or "fw build recent". fw build recent will build projects whose files have changed within the last 30 minutes, allowing you to make a change to a code file and press ALT-T-T to quickly build your change.

	All git repositories were cloned with shallow history, to not take up as much space. If you want to search back in history in one of your git repositories, you can first deepen the history by running:

		git fetch --unshallow

	Note that if you switch between FW 9 and FW 8, you will need to switch the default .NET runtime between mono 4 and mono 3 in MonoDevelop, Edit, Preferences, Projects, .NET Runtimes.

	Packaging: (Not yet fully working.)

	To use this machine to build packages, first run

	    cd ~/pbuilder && DISTRIBUTIONS="bionic xenial trusty" ./setup.sh

	You can then build a package managed by build-packages by running a command such as

	    ~/fwrepo/FwSupportTools/packaging/build-packages --main-package-name flexbridge --dists "xenial" --arches "amd64"  --repository-committishes flexbridge=origin/master --simulate-dput |& tee /var/tmp/log

	Or you can build a source and binary package by running commands such as

	    cd someproject && debuild -uc -us -S -nc
	    cd someproject/.. && sudo DISTRIBUTIONS=xenial ARCHES=amd64 ~/pbuilder/build-multi.sh source-package-name.dsc |& tee /var/tmp/log

### Wastaize

If you there is no Wasta .iso available and you are creating a Wasta machine from an Ubuntu 18.04 machine, then do the following. When installing wasta-cinnamon-bionic, it may ask which display manager to use. Make sure to use lightdm for Wasta.

        sudo add-apt-repository -y ppa:wasta-linux/wasta
        sudo add-apt-repository -y ppa:wasta-linux/wasta-apps
        sudo add-apt-repository -y ppa:wasta-linux/wasta-testing
        sudo add-apt-repository -y ppa:wasta-linux/cinnamon-3-6
        sudo apt-get update
        sudo apt-get install -y wasta-core-bionic
        sudo apt-get install -y wasta-cinnamon-bionic
        sudo wasta-initial-setup auto

Reboot.

If necessary, again turn off screen blanking and locking, set wallpaper, and adjust panel icons.

### Provision

* For a test machine:

		wget https://raw.githubusercontent.com/sillsdev/FwSupportTools/develop/vagrant/testing/provision-fw-test-machine
		bash provision-fw-test-machine

* For a dev machine:

		wget https://raw.githubusercontent.com/sillsdev/FwSupportTools/develop/vagrant/development/provision-fw-dev-machine
		bash provision-fw-dev-machine

	After the script, reboot a couple times. Launch monodevelop. Choose File - Open, fwrepo/fw/RunFieldWorks.csproj. Choose Edit - Preferences. Click .NET Runtimes. Click Add. Navigate to /opt/mono4-sil and click Open. Back in Preferences, click the mono4-sil option and click Set as Default. Click OK.

### Finish configuration

1. Free up disk space

		sudo apt-get update && sudo apt-get autoremove && sudo apt-get clean

* For fwtest, after apt clean, pre-download FW and dependencies.

		sudo apt-get install -dy fieldworks

* Clear any clipboard rings in guest.
* Shut down machine.

### Finalize

1. Boot to single user mode: Turn on the machine using virtualbox manager. When the virtualbox window says to press F12 to select boot device, hold left shift until Grub appears. For Wasta 16.04 and 18.04, choose Advanced options, and then the recovery option. Choose root from the recovery menu.

* Remount filesystem for writing

		mount -o remount,rw /

* Delete the guest's host keys, so they will be re-generated uniquely by users.

		rm -v /etc/ssh/ssh_host_*

* Zero-out deleted files so they don't take up space shipping with the product. Note: This does not appear to make the .vdi file on the host grow in size, so it should be safe to do for guests that need more space than the host has available. The `cat: write error: No space left on device` is expected and not a problem.

		df -h ; date ; time cat /dev/zero > /zeros ; sync ; sleep 1s ; sync ; ls -lh /zeros ; rm -v /zeros

* Shutdown guest.

		poweroff

### Generate and publish product

1. Export VM .box file. This may take 5-15 minutes. The `--base` argument is the name of the base machine in virtualbox manager.

		date ; time vagrant package --base fwdev-w1604-base --output fwdev-w1604-0.0.0.box

* Test that your new box is what you expect.

		mkdir test
		cd test
		vagrant init ../fwdev-w1604-0.0.0.box
		vim Vagrantfile # Uncomment provider virtualbox section that enables `vb.gui`.
		vagrant up

* See Smoke tests section below.

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

* Launch monodevelop. Start debugging FW. Remakefw from Tools menu. Start debugging FW.
* Verify that you can start flexbridge from fieldworks, such as by clicking "Get project from colleague".

### fwtest

* Launch synaptic. Enable llso:experimental. Install fieldworks.

## Notes

* The base box can't seem to be compressed any better as a vagrant box image, such as by using xz instead of gzip.

* You can free up space by deleting old base boxes. `vagrant box prune`

* You can make snapshots along the way if you suspect the next operation might fail, or if you want to test something and rollback so your test isn't in the product.