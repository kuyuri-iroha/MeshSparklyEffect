# MeshSparklyEffect

[日本語版](./README_ja.md)

## Overview

![Dancing Pymon](./Documentation/Images/pymon_demo.gif)

An effect for Unity that emits particles with sparkly effect matches the texture color from the specified mesh vertex position.

Two types of particles can be used: procedural ones that extend in a cross pattern, and textures.

Internally, vertex information is burned into textures when a mesh is specified, and the burned textures can be saved as EXR files.

Skinned Mesh Renderer and Mesh Filter are supported for the mesh format.

This effect is designed to be used with accessories and other items, so it does not support deformable meshes.

## Installation

This asset can be installed using the Unity Package Manager (UPM).

The packages required for installation are as follows.

(Will be installed automatically by installing using UPM.)

- Universal RP
- Visual Effect Graph

### From git URL

Open `Window > Package Manager` and click the + sign in the upper left corner to display `Add package from git URL...`
You can install the latest version by typing `git+ssh://git@github.com/Kuyuri-Iroha/MeshSparklyEffect.git?path=/Assets/MeshSparklyEffect` in the input field that appears when you click to install the latest version.

### From local disk (Release)

Download MeshSparklyEffect.zip from Release on GitHub, unzip it, and then open `Window > Package Manager` and click the + sign in the upper left corner. Then, click `Add package from disk...` and select unziped folder to install.

## Usage

After making sure that the Universal Render Pipeline Asset is set correctly in the Scriptable Render Pipeline Settings in the Graphics section of Project Settings, add MeshSparklyEffect.cs to the GameObject, which will generate a GameObject with a Visual Effect added to its lower layer and work.

See the documentation for details.

[Documentation for MeshSparklyEffect](./Documentation/MeshSparklyEffect.md)

## Unity Version & Dependencies

Development version：2020.3.9

- Universal RP
- Visual Effect Graph
