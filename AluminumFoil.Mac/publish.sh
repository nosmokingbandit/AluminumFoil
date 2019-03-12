dotnet publish -c Release -f netcoreapp2.0 -r osx-x64 --self-contained

mkdir -p ./AluminumFoil.app/Contents/MacOS
cp ./bin/Release/netcoreapp2.0/osx-x64/publish/* ./AluminumFoil.app/Contents/MacOS/

mkdir -p ./AluminumFoil.app/Contents/Resources
cp ./Assets/AluminumFoil.icns ./AluminumFoil.app/Contents/Resources

cat <<EOF > ./AluminumFoil.app/Contents/Info.plist
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple Computer//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
	<key>CFBundleExecutable</key>
	<string>AluminumFoil.Mac</string>
	<key>CFBundleGetInfoString</key>
	<string>AluminumFoil</string>
	<key>CFBundleIconFile</key>
	<string>AluminumFoil</string>
	<key>CFBundleName</key>
	<string>AluminumFoil</string>
	<key>CFBundlePackageType</key>
	<string>APPL</string>
</dict>
</plist>
EOF