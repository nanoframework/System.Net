# Running the Unit Tests

## Overview

The tests target nanoFramework MCU devices (or the Win32 nanoCLR emulator). Some tests require a host-side peer - the **Network Test Companion** - which acts as a TCP/UDP echo server and accepts connections from the MCU under test.

## Projects

| Project | Needs companion |
|---|---|
| `IPAddressTests` | No - self-contained data-structure tests |
| `NetworkHelperTests` | No - tests IP acquisition only |
| `SocketTests` | Loopback-only tests: No. Round-trip tests (`SocketRoundTripTests`): Yes (requires companion) |

---

## Network Test Companion

The companion is a .NET 10 console app in `Tests/NetworkTestCompanion/`. It runs on the developer's PC and exposes:

- A **control channel** (TCP, default port **11000**) that accepts newline-delimited JSON commands from MCU tests.
- Dynamic **TCP/UDP echo servers** spun up per test request.

### Build

```sh
dotnet build Tests/NetworkTestCompanion
```

### Run

```sh
dotnet run --project Tests/NetworkTestCompanion
```

The companion prints its bound IP and port on startup:

```
READY ip=192.168.1.10 port=11000
```

Use `--ip <addr>` to override the bind address or `--control-port <n>` to change the control port.

### Control channel commands

Send newline-terminated JSON; receive a JSON response on the same connection.

| Command | Description |
|---|---|
| `{"cmd":"ping"}` | Health check - returns `{"ok":true,"ip":"<companionIP>"}` |
| `{"cmd":"start_tcp_echo","port":N}` | Start a TCP echo server on port N |
| `{"cmd":"start_udp_echo","port":N}` | Start a UDP echo server on port N |
| `{"cmd":"stop","port":N}` | Stop the server on port N |
| `{"cmd":"stop_all"}` | Stop all active servers |
| `{"cmd":"connect_to","host":"...","port":N}` | Companion connects as TCP client (MCU acts as server) |

---

## Firewall configuration

The companion must be reachable from MCU devices on the local network. Run the following **once** to open the required ports.

### Windows (requires an elevated prompt)

```bat
dotnet run --project Tests/NetworkTestCompanion -- --setup-firewall
```

This adds inbound firewall rules named `nF-TestCompanion-TCP-*` / `nF-TestCompanion-UDP-*`. To remove them:

```bat
dotnet run --project Tests/NetworkTestCompanion -- --remove-firewall
```

### macOS

The macOS Application Firewall will prompt you to **Allow** the companion when it first starts listening. Click **Allow**. No further action is needed.

### Linux (ufw)

If `ufw` is active, open the ports manually:

```sh
sudo ufw allow 11000/tcp   # control channel
sudo ufw allow 7001/tcp    # TCP echo (RoundTrip_Tcp_SendReceive_Echo)
sudo ufw allow 7002/tcp    # TCP echo (RoundTrip_Tcp_LargeBuffer_Echo)
sudo ufw allow 7003/udp    # UDP echo (RoundTrip_Udp_SendReceive_Echo)
sudo ufw allow 7004/tcp    # MCU-as-server (RoundTrip_Tcp_McuAsServer_CompanionConnects)
```

### Linux (iptables / nftables)

For environments without ufw, add equivalent rules:

```sh
# iptables example
sudo iptables -A INPUT -p tcp --dport 11000 -j ACCEPT
sudo iptables -A INPUT -p tcp --dport 7001 -j ACCEPT
sudo iptables -A INPUT -p tcp --dport 7002 -j ACCEPT
sudo iptables -A INPUT -p udp --dport 7003 -j ACCEPT
sudo iptables -A INPUT -p tcp --dport 7004 -j ACCEPT

# nftables example
sudo nft add rule inet filter input tcp dport 11000 accept
sudo nft add rule inet filter input tcp dport 7001 accept
sudo nft add rule inet filter input tcp dport 7002 accept
sudo nft add rule inet filter input udp dport 7003 accept
sudo nft add rule inet filter input tcp dport 7004 accept
```

CI pipeline authors must open the required ports in their runner configuration.

---

## `.runsettings` parameters

The `.runsettings` file at the repo root contains two parameters for the companion:

| Parameter | Default | Description |
|---|---|---|
| `CompanionIP` | `127.0.0.1` | IP address of the PC running the companion |
| `CompanionControlPort` | `11000` | TCP port of the control channel |

For **virtual device** (emulator) runs the defaults work as-is.

For **real hardware** runs, set `CompanionIP` to the PC's LAN address - the address printed by the companion on startup:

```xml
<TestRunParameters>
    <Parameter name="CompanionIP" value="192.168.1.10" />
    <Parameter name="CompanionControlPort" value="11000" />
</TestRunParameters>
```
