[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=nanoframework_lib-nanoFramework.System.Net&metric=alert_status)](https://sonarcloud.io/dashboard?id=nanoframework_lib-nanoFramework.System.Net) [![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=nanoframework_lib-nanoFramework.System.Net&metric=reliability_rating)](https://sonarcloud.io/dashboard?id=nanoframework_lib-nanoFramework.System.Net) [![License](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE) [![NuGet](https://img.shields.io/nuget/dt/nanoFramework.System.Net.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.System.Net/) [![#yourfirstpr](https://img.shields.io/badge/first--timers--only-friendly-blue.svg)](https://github.com/nanoframework/Home/blob/main/CONTRIBUTING.md) [![Discord](https://img.shields.io/discord/478725473862549535.svg?logo=discord&logoColor=white&label=Discord&color=7289DA)](https://discord.gg/gCyBu8T)

![nanoFramework logo](https://raw.githubusercontent.com/nanoframework/Home/main/resources/logo/nanoFramework-repo-logo.png)

-----

### Welcome to the .NET **nanoFramework** System.Net Library repository

## Build status

| Component | Build Status | NuGet Package |
|:-|---|---|
| System.Net | [![Build Status](https://dev.azure.com/nanoframework/System.Net/_apis/build/status/System.Net?repoName=nanoframework%2FSystem.Net&branchName=main)](https://dev.azure.com/nanoframework/System.Net/_build/latest?definitionId=20&repoName=nanoframework%2FSystem.Net&branchName=main) | [![NuGet](https://img.shields.io/nuget/v/nanoFramework.System.Net.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.System.Net/) |

## NetworkHelper usage

`NetworkHelper` provides two patterns for establishing a network connection: a blocking token-based approach for simple use-cases, and an event-based approach for background connection management.

### Token-based (retryable)

Call `SetupAndConnectNetwork` with a `CancellationToken` timeout. This method can be called repeatedly — if the first attempt times out, call it again:

```csharp
bool connected = false;
while (!connected)
{
    CancellationTokenSource cs = new(30000);
    connected = NetworkHelper.SetupAndConnectNetwork(requiresDateTime: true, token: cs.Token);
    if (!connected)
    {
        Debug.WriteLine($"Network not ready, status: {NetworkHelper.Status}");
        // wait before retrying
        Thread.Sleep(5000);
    }
}
```

### Event-based

Call `SetupNetworkHelper` once at startup. The helper connects in the background. Wait on `NetworkReady`:

```csharp
NetworkHelper.SetupNetworkHelper(requiresDateTime: true);

if (!NetworkHelper.NetworkReady.WaitOne(30000, true))
{
    Debug.WriteLine($"Failed to connect: {NetworkHelper.Status}");
}
```

> **Note:** `NetworkReady` is reset when the connection is lost and re-signaled when it is restored, accurately reflecting live network state. Code that previously assumed `NetworkReady` would remain set after first connect should be updated to handle transient disconnects.

### Reset and reconfigure

Call `Reset()` to fully reset the helper so it can be called again with different settings, or to restart after an error:

```csharp
NetworkHelper.Reset();

// Now call SetupNetworkHelper or SetupAndConnectNetwork again
NetworkHelper.SetupNetworkHelper(requiresDateTime: true);
```

`SetupNetworkHelper` throws `InvalidOperationException` if called a second time without a prior `Reset()`. Token-based methods (`SetupAndConnectNetwork`) do not have this restriction and are always retryable.

## 

For documentation, providing feedback, issues and finding out how to contribute please refer to the [Home repo](https://github.com/nanoframework/Home).

Join our Discord community [here](https://discord.gg/gCyBu8T).

## Credits

The list of contributors to this project can be found at [CONTRIBUTORS](https://github.com/nanoframework/Home/blob/main/CONTRIBUTORS.md).

## License

The **nanoFramework** Class Libraries are licensed under the [MIT license](LICENSE.md).

## Code of Conduct

This project has adopted the code of conduct defined by the Contributor Covenant to clarify expected behaviour in our community.
For more information see the [.NET Foundation Code of Conduct](https://dotnetfoundation.org/code-of-conduct).

### .NET Foundation

This project is supported by the [.NET Foundation](https://dotnetfoundation.org).
