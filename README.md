# AluminumFoil
Standalone GUI USB installer for GoldLeaf

![Screenshot](https://raw.githubusercontent.com/nosmokingbandit/AluminumFoil/master/Screenshots/Capture.PNG)
![Screenshot](https://raw.githubusercontent.com/nosmokingbandit/AluminumFoil/master/Screenshots/MacCapture.png)

## Usage

#### First run setup

In order to communicate with your Switch you must first install the libusbk driver. This only needs to be done once and will not affect your ability to send RCM payloads.

* Download [Zadig](https://zadig.akeo.ie/)

* Plug your Switch into your pc and open Tinfoil. Navigate to `Title Management` -> `USB Install`.

* Open Zadig. In Options enable `List All Devices`. Select `libnx USB comms`. Select the driver `libusbk` and click `Replace Driver`

---

Your PC can now communicate with TinFoil. Leave your Switch on the `USB Install` screen.

#### Installing NSPs

* Connect your Switch to your pc and open GoldLeaf's `USB Install` screen.

* Open AluminumFoil.

* Select an NSP and click Install.
