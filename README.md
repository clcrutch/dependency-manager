# Dependency Manager
This project hopes to solve a common problem when managing projects.  This project allows users to specify a set of required development dependencies as part of a project.

## Install
Install the dotnet global tool. `dotnet tool install --global Clcrutch.DependencyManager --version 0.1.6-alpha`.

## Example
Create a file called `dependencies.yaml` with the below content

```yaml
windows:
  chocolatey:
    vscode:
```

Then run `depend install` as in an admin command prompt.  This will install Visual Studio Code from Chocolatey.