# arma-extension-dotnet

A basic example of an Arma 3 extension written in fully modern .NET 8.0 with C#.

Unlike previous versions which required dependencies such as DllExport, this project does not have any dependencies at all.

## Installation

1. Run the following command:
    ```
    dotnet publish -f net8.0 -c Release -r win-x64 -p:PublishAot=true -p:NativeLib=Shared -p:SelfContained=true
    ```

2. From the project directory, navigate to the output directory:
    ```
    .\bin\Release\net8.0\win-x64\publish
    ```

3. Copy `ArmaExtensionDotNet_x64.dll` into your base Arma 3 directory.

## Usage

An example SQF can be found in `mission\init.sqf`.

## Useful Links & Tools

[Arma 3 Wiki - Extensions](https://community.bistudio.com/wiki/Extensions)

[Arma 3 Wiki - callExtension](https://community.bistudio.com/wiki/callExtension)

[DLL Export Viewer](https://www.nirsoft.net/utils/dll_export_viewer.html)

[Arma 3 Extension Tester](http://killzonekid.com/arma-3-extension-tester-callextension-exe-callextension_x64-exe/)

## License

[MIT](https://choosealicense.com/licenses/mit/)
