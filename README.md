# NoirCvmApi.NET
NoirVisor Customizable VM API Library on .NET Framework

## Introduction
[NoirVisor](https://github.com/Zero-Tang/NoirVisor) is a hardware-accelerated hypervisor solution. This repository is a Class Library project that abstracts the Customizable VM feature of NoirVisor so the functionalities may be exposed to .NET Framework applications.

## Supported Platforms
Currently, only 64-bit Windows Operating Systems running on processors that support AMD-V are supported. \
You should install .NET Framework 4.0 or higher in order to run an application that depends on this library. \
You must load [NoirVisor](https://github.com/Zero-Tang/NoirVisor) driver and subvert the system before using any Customizable VM features.

## Build
In order to build this project, you must install Visual Studio 2010 or higher.

## Importing NoirCvmApi.NET to Your Project
Edit the `Reference` section of your .NET project. \
Future versions of NoirCvmApi.NET will be published through `nuget`.

## Sample Code
There is a long-mode sample project included in this repository.

## Documentation
Documentation of this library is planned to be released on [GitHub Wiki of this repository](https://github.com/Zero-Tang/NoirCvmApi.NET/wiki).

## License
This repository is under the MIT license.