# AluminumFoil
Standalone USB installer for TinFoil

Based on XorTroll's [tinfoil_usb_pc.py](https://github.com/XorTroll/Tinfoil/blob/master/tools/usb_install_pc.py)

Intended to operate as a standalone alternative to those unable or unwilling to install python and required packages.

### Usage

AluminumFoil takes no arguments -- all required information will be requested as needed. Simply run `go run main.go` or build with `go build` and execute the resulting binary. Running or building from source will require [libusb](https://libusb.info/).

AluminumFoil has been tested on Windows 10 x64. AluminumFoil is in Alpha and should not be expected to work under any circumstances. I am not responsible for lost data.

