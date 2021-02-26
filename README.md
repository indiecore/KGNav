# KGNav

Simple library for handling scene navigation and data transfer between managed scenes in Unity.

It also provides a simple abstract class for implementing dynamic loading screens between the scene loads.

## Requirements

While not specifically required it is suggested you also import the KGToolsGeneral repository and use the service locator to manage your game specific NavigationsSceneManager.

KGToolsGeneral can be downloaded [HERE](https://github.com/indiecore/KGToolsGeneral).

If this component is not downloaded the inherited NavigationSceneManager component will need to be setup according to your needs as a singleton or something.

## Usage

The component allows a stack based scene management system with navigation and data passed between scenes 

Each game will have to implement it's own NavigationSceneManager to provide access to the loading screens in whatever way is deemed best for your game.

This component assumes a basic non-managed initialization scene as the first scene in the stack.

From that scene you have to call PushScene for your first scene.

This component requires loading screens based on KGTools.General.AbstractLoadingScreen. A very simple implementation is included that simply enables/disable the component's GameObject which can be used for prototyping and testing.
