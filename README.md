# AluminumFoil
Standalone GUI USB installer for GoldLeaf

![Screenshot](https://raw.githubusercontent.com/nosmokingbandit/AluminumFoil/master/Screenshots/Capture.PNG)
![Screenshot](https://raw.githubusercontent.com/nosmokingbandit/AluminumFoil/master/Screenshots/MacCapture.png)


## Preparation

#### Windows first run setup

In order to communicate with your Switch you must first install the libusbk driver. This only needs to be done once and will not affect your ability to send RCM payloads.

* Download [Zadig](https://zadig.akeo.ie/)

* Plug your Switch into your pc and open Tinfoil. Navigate to `Title Management` -> `USB Install`.

* Open Zadig. In Options enable `List All Devices`. Select `libnx USB comms`. Select the driver `libusbk` and click `Replace Driver`

Your PC can now communicate with TinFoil. Leave your Switch on the `USB Install` screen.

## Installing NSPs

* Connect your Switch to your pc and open GoldLeaf or TinFoil's `USB Install` screen.

* Open AluminumFoil.

* Select an NSP and click Install.

## For best results....

OSX 10.12 or greater is required.

When using TinFoil it is advised to use @satelliteseeker's build found [here](https://github.com/satelliteseeker/Tinfoil/releases/tag/v0.2.1-USB-fix). Other versions may work but I wouldn't expect it to.

As of 0.4, GoldLeaf's USB installs can be inconsistent. Any issues submitted regarding GoldLeaf installs will be ignored until GoldLeaf 0.5 is released.
