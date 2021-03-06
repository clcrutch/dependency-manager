# Dependency Manager
This project hopes to solve a common problem when managing projects.  This project allows users to specify a set of required development dependencies as part of a project.

## Install
Install the dotnet global tool. `dotnet tool install --global Clcrutch.DependencyManager --version 0.1.19-alpha`.

## Example
Create a file called `dependencies.yaml` with the below content

```yaml
all: # This names the block
  platform: all
  architecture: all
  vscode:
    eamodio.gitlens:
        dependencies:
          - vscode # Sets up a dependency on the named package
    VisualStudioExptTeam.vscodeintellicode:
        dependencies:
          - vscode

windows:
  platform: windows
  architecture: all
  chocolatey:
    visualstudio2019community:
    visualstudio2019-workload-netcoretools:
    vscode:
        name: vscode # Dependency of eamodio.gitlens and VisualStudioExptTeam.vscodeintellicode

windows10:
  platform: windows
  architecture: amd64
  version: 10.0.18362 # Sets a particular version.
  feature:
    Microsoft-Windows-Subsystem-Linux:
      name: wsl
    VirtualMachinePlatform:
      name: virtualmachine
  msi:
    https://wslstorestorage.blob.core.windows.net/wslblob/wsl_update_x64.msi: # Autodownloaded from here.
      name: kernel
      dependencies:
        - wsl
        - virtualmachine
  appx:
    https://aka.ms/wslubuntu2004: # Autodownloaded from here.
      dependencies:
        - kernel
```

Then run `depend install` as in an admin command prompt.  Based on your platform/architecture/version, the above file will be transformed to install the relevant packages.

## Install from URL
```PowerShell
$URL = "==URL=="; Invoke-Expression (Invoke-WebRequest https://raw.githubusercontent.com/clcrutch/dependency-manager/master/scripts/install.ps1).Content; Invoke-InstallScript -URL $URL
```

Replace ==URL== with your URL in the above expression.