# Remove SignalR MessagePack Design

> Status: approved
> Date: 2026-07-06
> Scope: NexusStackBackend

## Goal

Remove the unused SignalR MessagePack protocol and its vulnerable dependency while preserving SignalR communication through the default JSON protocol.

## Changes

- Remove `Microsoft.AspNetCore.SignalR.Protocols.MessagePack` from `NexusStack.Core`.
- Remove `.AddMessagePackProtocol()` from MQService SignalR registration.
- Remove MessagePack-specific payload conversion and comments from `NotificationEventHandler`.
- Send notification payloads as JSON-compatible objects through SignalR's default JSON protocol.

## Compatibility

The repository contains no SignalR client or `MessagePackHubProtocol` selection. Default JSON clients remain compatible. External clients that explicitly request MessagePack must switch to JSON.

## Verification

- Source scan contains no MessagePack package, registration, or compatibility references.
- `dotnet restore` and `dotnet build NexusStack.sln` succeed.
- `dotnet list NexusStack.sln package --vulnerable --include-transitive` no longer reports MessagePack packages.
