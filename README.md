# ginstall

A CLI tool to quickly download different versions of software from GitHub

# Usage

ginstall is relatively user-friendly, and is very simple and easy to use.

## Syntax

```
ginstall [author]/[repo]/[branch]
```

> [!NOTE]
> When no branch is provided, it will default to main.

## Example

```
ginstall Dismalitie/ginstall/main
```

## Selecting and downloading a package

After selecting the repository, a package list will appear:

![the ginstall package list screen](https://github.com/user-attachments/assets/d8552e84-c483-468e-a11b-7771f0b81914)

Simply type the package number to download. After doing so, you will be prompted to specify the path to install the package to:

![path prompt](https://github.com/user-attachments/assets/96f40755-1897-4310-a0af-008cbbc9d75c)

Before the download begins, a confirmation prompt will be shown, listing the package name and the location to install to:

![confirmation prompt](https://github.com/user-attachments/assets/68b4fbfb-a7e8-46d9-a7ad-00d03e2c2827)

After confirming, ginstall will download all the required files to the specified location:

![download process](https://github.com/user-attachments/assets/bc7487ae-df63-4b09-9403-d022a895ab81)

> [!IMPORTANT]
> Tada! You successfully installed a package with ginstall!

# How to make your repository support ginstall

ginstall uses a manifest in the root of the branch or repo called `.ginstall`. It follows the INI syntax.

## Headers

ginstall currently only has one header, the `minVer` header. This tells ginstall the minimum version that the client must be to be able to download packages from the repository.

```ini
minVer=1.0
```

> [!IMPORTANT]
> Do not place this under any sections; place it at the top of the file.

## Listing a downloadable package

To create a downloadable package, add this to you `.ginstall` file.

```ini
[package:package_name]
name=Cool Package
description=A very cool package
files=README.md|SomeFile.txt|CoolGame.exe
```

> [!NOTE]
> The `package_name` element is not visible to the client, and is used internally.

`name` is displayed to the client along with `description`. Keep the description short and simple.

They will be displayed like:

```
[0] Name - Description
```

> [!NOTE]
> The package number cannot be custom set, but rather it is decided in order of the packages.

> [!TIP]
> Put your most useful or reccomended package at the top.

`files` is the most important setting, listing all the paths to files that ginstall will download. The files are separated by `|`s.

Downloadeable files don't have to be in the same directory as the manifest. The file will be downloaded from the path, relevant to the repositorie's current branch's root. Example:

```ini
files=ginstall/Program.cs|ginstall/ginstallPackage.cs
```

This will go into the `ginstall` folder and download `Program.cs` and `ginstallPackage.cs`.

> [!NOTE]
> The filename upon downloading will be preserved, and not include the path.

> [!IMPORTANT]
> File paths do not transfer upon download. For example, when downloading `Program.cs`, because it is in the `ginstall` folder, the file will not be downloaded to `<download path specified by user>/ginstall/Program.cs`
