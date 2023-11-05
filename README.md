<!--
   Copyright 2023 Alexander Stärk

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
-->
# Basilisque - Dependency Injection

## Overview
This project provides functionality to easily register all services of the target project at the IServiceCollection.

[![NuGet Basilisque.DependencyInjection](https://img.shields.io/badge/NuGet_Basilisque.DependencyInjection-latest-blue.svg)](https://www.nuget.org/packages/Basilisque.DependencyInjection)
[![License](https://img.shields.io/badge/License-Apache%20License%202.0-red.svg)](LICENSE.txt)

## Description
This project contains functionality for registering services at the IServiceCollection.  
It also contains a source generator, that generates code to registers all services of the target project.  
The services simply have to be marked with an attribute.
The attribute can also be attached to base classes or interfaces. This means that all implementations of the base class or interface will be automatically registered.

The generated code is marked as partial. This means it can be easily extended with custom logic.

## Getting Started
Install the NuGet package [Basilisque.DependencyInjection](https://www.nuget.org/packages/Basilisque.DependencyInjection).  
Installing the package will also install the package Basilisque.DependencyInjection.CodeAnalysis as a child dependency. This contains the source generator.

Now you're ready to [register your first service](https://github.com/basilisque-framework/DependencyInjection/wiki/Getting-Started).


## Features
- Generates code that registers all marked services of a project.  
  This means anyone who uses your project can call a single method to register all of your provided services.

- The generated code automatically calls the registration methods of all dependencies that also use Basilisque.DependencyInjection.  
  This means if you use Basilisque.DependencyInjection in multiple related projects, you have to call the registration method only once and all dependencies will be registered, too.

- Configuration of the registration
  * Lifetime (Transient, Scoped, Singleton)
  * Type as that the service will be registered can be specified
  * ImplementsITypeName  
  When the service implements an interface with the same name but starting with an I, it will be registered as this interface (e.g. 'PersonService' as 'IPersonService').

- Marker attributes on interfaces and base classes.  
  This means for example, that you can register all implementations of an interface with the same configuration and you don't have to worry about registration whenever you create a new implementation of it.

- Custom marker attributes  
  You can create custom attributes with predefined configuration by inheriting from the provided attributes. Whenever you use your attribute you can be sure, that the same configuraiton will be used.

- Multiple registrations of the same service  
  A service can have multiple registrations with different configurations.

## Examples
All implementations of this interface will be registered as singleton:
```csharp
[RegisterServiceSingleton()]
public interface IMyInterface
{
}
```
You can get the same result writing it like this:
```csharp
[RegisterService(RegistrationScope.Singleton)]
public interface IMyInterface
{
}
```

For details see the __[wiki](https://github.com/basilisque-framework/DependencyInjection/wiki)__.


## License
The Basilisque framework (including this repository) is licensed under the [Apache License, Version 2.0](LICENSE.txt).