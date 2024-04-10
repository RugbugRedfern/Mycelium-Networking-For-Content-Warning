# Changelog

All notable changes to this project will be documented in this file.
This project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## 1.0.10
- Added DeregisterNetworkObject, for deregistering destroyed objects.

## 1.0.9
- Updated README

## 1.0.8
- Registering LobbyData and PlayerData keys before accessing them is now optional. This makes dynamic keys easier to use, but you should always register your keys when you know them at compile time, or the LobbyDataUpdated and PlayerDataUpdated callbacks will not fire for them.

## 1.0.7
- Added LobbyData and PlayerData functionality, which allows you to define synced variables associated with the lobby (perfect for config syncing) or individual players
- The LobbyLeft callback should now properly fire
- General code cleanup

## 1.0.6
- Automated Thunderstore release

## 1.0.5
- Automated Nuget release

## 1.0.4
- Preparing GitHub actions for automatic update deployment

## 1.0.3
- Exceptions thrown inside RPCs will now be logged
- Improved log style consistency

## 1.0.2
- Increased maximum payload size to 524288 bytes

## 1.0.1
- Updated README

## 1.0.0
- Initial Release