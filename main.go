package main

import (
	"bufio"
	"encoding/binary"
	"fmt"
	"io"
	"io/ioutil"
	"os"
	"path/filepath"
	"strings"
	"unicode/utf8"

	"github.com/AlecAivazis/survey"
	"github.com/google/gousb"
)

var cmdTypeResponse = byte(1)
var cmdIDExit uint64 // = 0
var cmdIDRequestNSP uint32 = 1

// listNSPs returns list of absolute paths to NSP files
func listNSPs(dir string) ([]string, error) {
	nspList := []string{}

	files, err := ioutil.ReadDir(dir)
	if err != nil {
		return nil, err
	}

	for _, file := range files {
		if !file.IsDir() && filepath.Ext(file.Name()) == ".nsp" {
			nspList = append(nspList, filepath.Join(dir, file.Name()))
		}
	}
	return nspList, nil
}

// sendNSPList sends NSPs in nspDir to Switch
func sendNSPList(nspList []string, epOut *gousb.OutEndpoint) error {
	for i, s := range nspList {
		nspList[i] = s + "\n"
	}

	// Tinfoil USB List 0
	epOut.Write([]byte("TUL0"))
	// Send len of nsplist as a 4-byte array.
	nspLen := uint32(utf8.RuneCountInString(strings.Join(nspList, "")))
	b := make([]byte, 4)
	binary.LittleEndian.PutUint32(b, nspLen)
	epOut.Write(b)
	// 0x00 * 8 Padding
	epOut.Write(make([]byte, 8))

	fmt.Println("Sending NSP list")
	for _, nsp := range nspList {
		epOut.Write([]byte(nsp))
	}
	return nil
}

// pollCommands waits for command from Switch and calls appropriate method
// Currently the only supported method is fileRangeCmd
func pollCommands(epOut *gousb.OutEndpoint, epIn *gousb.InEndpoint) {
	fmt.Println("Waiting for Switch...")
	for true {
		// Read 32 bytes from switch
		inputBuf := make([]byte, 32)
		_, err := epIn.Read(inputBuf)

		if err != nil {
			printErr(err)
			continue
		}

		// Oh-oh its magic!
		if string(inputBuf[:4]) != "TUC0" {
			continue
		}

		cmdID, _ := binary.Uvarint(inputBuf[8:12])
		if cmdID == cmdIDExit {
			fmt.Println("Exiting...")
			return
		} else if cmdID == 1 {
			cmdRequestNSP(epOut, epIn)
		}
	}
}

// sendRespHeader sends response header to Switch to prepare it for data payload
// This should preface all data transmissions to Switch
// Header is 32 bytes as described in each step below
func sendRespHeader(epOut *gousb.OutEndpoint, cmdID uint32, dataSize uint64) {
	// Tinfoil USB Command 0
	epOut.Write([]byte("TUC0"))
	// Send cmdTypeResponse (1) as 4-byte array
	// Tinfoil sends CMD_TYPE_RESPONSE first, then 3 bytes of padding, this does it in one step
	epOut.Write([]byte{cmdTypeResponse, 0, 0, 0})

	// Send cmd id as 4-byte array
	b := make([]byte, 4)
	binary.LittleEndian.PutUint32(b, cmdID)
	epOut.Write(b)

	// Send payload size as 8-byte array
	b = make([]byte, 8)
	binary.LittleEndian.PutUint64(b, dataSize)
	epOut.Write(b)

	// 0x00 * 12 Padding
	epOut.Write(make([]byte, 12))
}

// cmdRequestNSP handles request from Switch for NSP payload
// This is called many times as the Switch requests different pieces of the NSP
// The Switch will request a payload of requestedPieceSize bytes starting at requestedPieceOffset in the NSP file
// This requested piece is broken up into 1mb chunks and sent to tinfoil
func cmdRequestNSP(epOut *gousb.OutEndpoint, epIn *gousb.InEndpoint) {
	// Read 32 bytes from switch
	inputBuf := make([]byte, 32)
	_, err := epIn.Read(inputBuf)
	if err != nil {
		fmt.Println("Unable to read command from Switch")
		return
	}

	requestedPieceSize := binary.LittleEndian.Uint64(inputBuf[:8])
	requestedPieceOffset := int64(binary.LittleEndian.Uint64(inputBuf[8:16]))
	nspNameLen := binary.LittleEndian.Uint64(inputBuf[16:24])

	nspNameBytes := make([]byte, nspNameLen)
	_, err = epIn.Read(nspNameBytes)
	if err != nil {
		fmt.Println("Unable to read requested NSP name from Switch")
	}

	fmt.Printf("Piece Size: %d, Piece Offset: %d, Name Len: %d, Name: %s \n", requestedPieceSize, requestedPieceOffset, nspNameLen, string(nspNameBytes))
	sendRespHeader(epOut, cmdIDRequestNSP, requestedPieceSize)

	fileHandle, err := os.Open(string(nspNameBytes))
	if err != nil {
		fmt.Printf("%#v", err)
		return
	}
	defer fileHandle.Close()

	fileHandle.Seek(requestedPieceOffset, 0)

	fileReader := bufio.NewReader(fileHandle)

	var currentOffset uint64
	var readChunkSize uint64 = 1048576 // 1mb

	for currentOffset < requestedPieceSize {
		if currentOffset+readChunkSize >= requestedPieceSize {
			readChunkSize = requestedPieceSize - currentOffset
		}

		readBuffer := make([]byte, readChunkSize)

		bytesRead, err := fileReader.Read(readBuffer)
		if err != nil {
			if err != io.EOF {
				fmt.Printf("%#v", err)
			}
			break
		}

		_, err = epOut.Write(readBuffer[:bytesRead])
		if err != nil {
			fmt.Printf("%#v", err)
			break
		}
		currentOffset += readChunkSize
	}
	return
}

// exit simply prevents the console window from closing immediately
func exit() {
	fmt.Println("Press Enter to exit")
	var input string
	fmt.Scanf("%s", &input)
	os.Exit(0)
}

func printErr(err error) {
	fmt.Println(fmt.Errorf("%#v", err))
}

func main() {
	vid, pid := gousb.ID(0x057E), gousb.ID(0x3000)

	usbCTX := gousb.NewContext()
	defer usbCTX.Close()

	nxDevice, err := usbCTX.OpenDeviceWithVIDPID(vid, pid)

	if err != nil || nxDevice == nil {
		fmt.Println("Unable to open USB connection to Switch")
		exit()
	}
	fmt.Println("Connected to Switch")

	intf, done, err := nxDevice.DefaultInterface()
	defer done()
	if err != nil {
		fmt.Printf("%s.DefaultInterface(): %v", nxDevice, err)
		exit()
	}

	epOut, err := intf.OutEndpoint(1)
	if err != nil {
		fmt.Println("Unable to open USB OUT endpoint [1]")
		exit()
	}

	epIn, err := intf.InEndpoint(1)
	if err != nil {
		fmt.Println("Unable to open USB IN endpoint [1]")
		exit()
	}

	nspDir := ""
	input := &survey.Input{
		Message: "NSP Directory:",
	}
	survey.AskOne(input, &nspDir, nil)

	nspList, err := listNSPs(nspDir)
	if err != nil {
		printErr(err)
		exit()
	}

	selectedNSPs := []string{}
	multiselect := &survey.MultiSelect{
		Message:  "NSPs to send to TinFoil",
		Options:  nspList,
		PageSize: 15,
	}
	survey.AskOne(multiselect, &selectedNSPs, nil)

	if sendNSPList(selectedNSPs, epOut) == nil {
		pollCommands(epOut, epIn)
	}

	exit()
}
