# AluminumFoil
Standalone USB installer for TinFoil

Based on @Adubbz's [tinfoil_usb_pc.py](https://github.com/XorTroll/Tinfoil/blob/master/tools/usb_install_pc.py)

Intended to operate as a standalone alternative to those unable or unwilling to install python and required packages.

## Usage

#### First run setup

##### Windows
In order to communicate with your Switch you must first install the libusbk driver. This only needs to be done once and will not affect your ability to send RCM payloads.

* Download [Zadig](https://zadig.akeo.ie/)

* Plug your Switch into your pc and open Tinfoil. Navigate to `Title Management` -> `USB Install`.

* Open Zadig. In Options enable `List All Devices`. Select `libnx USB comms`. Select the driver `libusbk` and click `Replace Driver`

##### Mac OSX

OSX requires libusb due to the way it handles (or rather *doesn't* handle) static libraries.

Currently the easiest way to accomplish this is with [Homebrew](https://brew.sh/)

    $ brew install libusb

---

Your PC can now communicate with TinFoil. Leave your Switch on the `USB Install` screen.

#### Installing NSPs

* Connect your Switch to your pc and open Tinfoil's `USB Install` screen.

* Open AluminumFoil.

* Enter the directory containing your NSPs.

* Select the NSPs you wish to send to TinFoil using arrow keys to select and space to toggle. Press Enter when finished.

* On your Switch select the titles you want to install and press `A`.


### Running/Building from source

Building AluminumFoil requires [libusb](https://libusb.info/).

Run with `go run main.go` or build with `./build.sh` and execute the resulting binary. `build.sh` includes the neccesary compiler flags to include libsub in the binary.

AluminumFoil has been tested on Windows 10 x64. AluminumFoil is in Alpha and should not be expected to work under any circumstances. I am not responsible for lost data.
