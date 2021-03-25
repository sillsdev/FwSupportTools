# Instructions to create base box

## Install

* Make sure you have at least 50 GB of disk space free, as you will end up with a few
  copies of the machine you are working on, in different forms.
* Use virtualbox manager to create a new VM. Name it following the pattern `fwdev-u2004-base`.
* Give 4 GB RAM and 200 GB dynamic storage. Actual RAM available will depend on a
  Vagrantfile setting later on. Give 2 processors.
* Boot installation .iso. Install OS. Don't install unneeded "third-party software" in Ubuntu.
* Use a computer name such as following the pattern `fwdev-u2004`, 
  username `vagrant` and password `vagrant`. Choose Log in automatically.

## Setup machine

* Launch Software Updater and apply updates. If that gives any trouble, can fall back to:
  boot to recovery mode, enable networking, and run `apt update && apt upgrade -y`.

* Install guest additions.

  * On your host machine, run

    sudo apt install virtualbox-guest-additions-iso

  * In the guest machine, run

    sudo apt update && sudo apt install -y linux-headers-$(uname -r) build-essential \
      dkms virtualbox-guest-utils virtualbox-guest-dkms

    In the guest machine window, choose Devices > Insert Guest Additions CD image. In the
    guest, a VBOXADDITIONS window appears; click Run. A terminal window will appear,
    asking if you want to replace versions of additions. Enter yes. It will say something
    went wrong. Right-click the CD icon on the panel or desktop and Eject the VirtualBox
    Additions disc.

  * In the guest machine window, choose Devices > Shared clipboard > Bidirectional.

  * Reboot.

* Set resolution to a big enough but conservative initial value of 1600x900, that will fit
  inside someone's 1080p desktop.

* Taking a virtual machine snapshot here makes for a good place to come back to when making
  new versions of this virtual machine.

* Run provision script on guest. Possibly by copying and pasting its contents into a new
  file on guest and running it. For example,

  nano ~/provision && bash ~/provision

* Log out to use newly installed and activated Gnome extensions.

* Launch Settings. Don't use a bunch of RAM for error reports:
  Privacy - Diagnostics - Send error reports to Canonical - Never.

* Set desktop background.

  IMAGE_CODE="insert-id-here" &&
  mkdir -p ~/.local/share/backgrounds &&
  wget -O ~/.local/share/backgrounds/${IMAGE_CODE}-unsplash.jpg "https://unsplash.com/photos/${IMAGE_CODE}/download?force=true" &&
  gsettings set org.gnome.desktop.background picture-uri "file:///home/vagrant/.local/share/backgrounds/${IMAGE_CODE}-unsplash.jpg" &&
  gsettings set org.gnome.desktop.screensaver picture-uri "file:///home/vagrant/.local/share/backgrounds/${IMAGE_CODE}-unsplash.jpg"

* Arrange desktop icons, since may be in disarray from resizing the desktop or moving panel.
  Make a couple rows of icons, with the second row containing Git Gui, Terminator, Code, Gitk, Git Cola, Byobu.

* Clean up byobu status bar (F1).

* Optionally edit basebox version file at `~/machine-info.txt`. "Installed from" can state
  the installation media used to create the machine.

* On fwtest, add terminal and synaptic to dock/panel.

* Turn off automatic updates, so that a user won't turn off the machine during updates, which may make a mess. In Wasta 16.04 or 18.04, launch Software Settings (or "Software & Updates" in Ubuntu 18.04~20.04), go to the Updates tab. 'Automatically check for updates' to 'Every two weeks'. Close and re-open the Software Settings window if needed to make 'When there are security updates' work. Change 'When there are security updates' to 'Download automatically'. Change 'When there are other updates' to 'Display every two weeks'. Change 'Notify me of..' to 'Never'.

  For fwtest, set "When there are security updates" to "Display immediately" to prevent the fwtest machine from starting downloads immediately upon boot, which gets in the way of the machine being used to immediately install software to test.

## Finalize

* Do the following to free up disk space, delete the guest's ssh host keys so they will be
  re-generated uniquely by users, and zero-out deleted files so they don't take up space
  shipping with the product. A `cat: write error: No space left on device` is expected and
  not a problem.

  sudo apt-get update && sudo apt-get -y autoremove && sudo apt-get -y clean \
    && sudo rm -v /etc/ssh/ssh_host_* \
    && cat /dev/zero > ~/zeros; sync; ls -lh ~/zeros; rm -v ~/zeros

* For fwtest, after apt clean, pre-download FW and dependencies.

  sudo apt-get install -dy fieldworks

* Power off.

## Generate and publish product

* Export VM .box file and prepare to test the result. This may take 30 minutes.
  The `--base` argument is the name of the base machine in virtualbox manager. Replace
  the BOX and VERSION strings in the below before running.

export BOX="fwdev-u2004"
export VERSION="1.0.0"
date && vagrant package --base ${BOX}-base --output ${BOX}-${VERSION}.box &&
  date && ls -lh ${BOX}-${VERSION}.box &&
  sha256sum ${BOX}-${VERSION}.box | tee -a "${BOX}".json &&
  date && mkdir test && tee test/Vagrantfile << END &&
Vagrant.configure("2") do |config|
  config.vm.box = "../${BOX}-${VERSION}.box"
  config.vm.provider "virtualbox" do |vb|
    vb.gui = true
    vb.cpus = 2
    vb.memory = "4000"
  end
end
END
cd test && vagrant up; date

* Smoke test the result

  - fwdev

    - Build FB and FW.
    - Launch VSCode. Open FW workspace. Start debugging FW.
    - Verify that you can start flexbridge from fieldworks, such as by clicking "Get project from colleague".

  - fwtest

    - Launch synaptic. Enable pso:main, pso:experimental, llso:main, llso:experimental. 
    - Install and launch fieldworks.

* Clean up box test machine. In the `test` directory, run the following.
  The `vagrant box remove` command removes the internally stored copy of the base box
  (to free disk space); it's not removing the '../foo' _file_, but internally stored data
  with the designation of '../foo'.

  vagrant halt && vagrant destroy && vagrant box remove "../${BOX}-${VERSION}.box" && cd .. && rm test -rf

* Update the box .json file, and move the sha to the right place. Publish basebox.

## Notes

* Changes to guest settings can be seen by observing differences in output from
```
gsettings list-recursively
dconf dump /
```

* You can free up space by deleting old base boxes. `vagrant box prune`
