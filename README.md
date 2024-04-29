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
    bin\Release\net8.0\win-x64\publish
    ```

3. Copy `ArmaExtensionDotNet_x64.dll` into your base Arma 3 directory.

## License

[MIT](https://choosealicense.com/licenses/mit/)
