# Remove SignalR MessagePack Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Remove the unused SignalR MessagePack protocol and retain the default JSON protocol.

**Architecture:** Remove the direct protocol package and MQService registration, then simplify notification payload handling for System.Text.Json. Verify with a source-level regression scan, restore/build, and NuGet vulnerability scan.

**Tech Stack:** .NET 10, ASP.NET Core SignalR, System.Text.Json, PowerShell

---

## File Structure

- Modify `Domain/NexusStack.Core/NexusStack.Core.csproj`: remove the SignalR MessagePack package.
- Modify `Domain/NexusStack.Core/ServiceCollectionExtensions.cs`: register SignalR with its default JSON protocol only.
- Modify `Domain/NexusStack.Core/EventHandler/NotificationEventHandler.cs`: remove MessagePack-specific payload conversion.

### Task 1: Remove MessagePack

**Files:**
- Modify: `Domain/NexusStack.Core/NexusStack.Core.csproj:24`
- Modify: `Domain/NexusStack.Core/ServiceCollectionExtensions.cs:193-197`
- Modify: `Domain/NexusStack.Core/EventHandler/NotificationEventHandler.cs:72-85`

- [x] **Step 1: Run the regression scan and verify it fails**

Run:

```powershell
$matches = rg -n "MessagePack|AddMessagePackProtocol" Domain -g "*.cs" -g "*.csproj" -g "!**/bin/**" -g "!**/obj/**"
if ($LASTEXITCODE -eq 0) { $matches; throw "MessagePack references remain" }
```

Expected: the command fails and reports the package reference, protocol registration, and compatibility comments.

- [x] **Step 2: Remove the package and protocol registration**

Delete this package reference from `NexusStack.Core.csproj`:

```xml
<PackageReference Include="Microsoft.AspNetCore.SignalR.Protocols.MessagePack" Version="10.0.7" />
```

Change MQService registration to:

```csharp
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
});
```

- [x] **Step 3: Use JSON-compatible notification payloads**

Replace the MessagePack-specific payload branch with:

```csharp
object payload = relayMessage.Payload.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null
    ? new
    {
        relayMessage.Event,
        relayMessage.Target,
        Payload = (string)null
    }
    : relayMessage.Payload;
```

- [x] **Step 4: Run the regression scan and verify it passes**

Run the command from Step 1.

Expected: exit code 0 with no MessagePack references under `Domain`.

- [x] **Step 5: Restore, build, and scan vulnerable dependencies**

Run:

```powershell
dotnet restore NexusStack.sln
dotnet build NexusStack.sln --no-restore
dotnet list NexusStack.sln package --vulnerable --include-transitive
```

Expected: restore and build succeed; the vulnerability report contains no `MessagePack` or `Microsoft.AspNetCore.SignalR.Protocols.MessagePack`.

- [x] **Step 6: Commit**

```powershell
git add Domain/NexusStack.Core/NexusStack.Core.csproj Domain/NexusStack.Core/ServiceCollectionExtensions.cs Domain/NexusStack.Core/EventHandler/NotificationEventHandler.cs Docs/superpowers/plans/2026-07-06-remove-messagepack.md
git commit -m "build: remove SignalR MessagePack protocol"
```
