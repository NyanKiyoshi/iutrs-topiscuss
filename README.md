# IUTRS-Topiscuss
University assignment, an advanced IRC-like UDP client-server.

## Requirements
You need to install the [dotnet-sdk](https://www.microsoft.com/net/download)
and add it to your `PATH` (if installed from Microsoft's website).

| Platform  |  Download Link                                                  |
|-----------|-----------------------------------------------------------------|
| ArchLinux | https://www.archlinux.org/packages/community/x86_64/dotnet-sdk/ |
| Windows, MacOS, Linux | https://www.microsoft.com/net/download              |

## Compiling the project
1. Ensure your current directory is the root project;
1. Run `dotnet build`.

## Running the tests
1. Change your current directory to the `Tests` folder;
1. Run the command `dotnet test`.

## Compiling the docs
The project uses `doxygen` to compile the internal C# XML documentation.

### Installing Doxygen
| Platform   | Version   | Reason                                           |
|------------|-----------|--------------------------------------------------|
| Windows(*) | == 1.8.0  | Other releases are bugged on Windows.            |
| Unix-like  | \>= 1.8.0 | All versions are working just fine on Unix-like. |

<p align='center'>
    <a href='https://is.gd/2FVTMA'>Windows Download Link</a>
</p>

Note: on Windows, add `doxygen` to your `PATH`.

### Usage
1. Run `doxygen` in the root project folder;
1. Open `docs-html/index.html` in your web-browser.
