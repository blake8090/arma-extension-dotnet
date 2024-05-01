dotnet publish -f net8.0 -c Release -r win-x64 -p:PublishAot=true -p:NativeLib=Shared -p:SelfContained=true
copy "bin\Release\net8.0\win-x64\publish\ArmaExtensionDotNet_x64.dll" "C:\Program Files (x86)\Steam\steamapps\common\Arma 3\ArmaExtensionDotNet_x64.dll"
