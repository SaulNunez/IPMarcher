# IP Marcher
Small multiplatform IP scanner for the terminal.

## How to use
* Set the range of IP addresses to check. By default 192.168.1.0 up to 192.168.1.254 are scanned. You can set a custom range by using `-s` for the start of the IP range and `-e` for the end. Currently only IPV4 is supported.
* Set ports to scan. By default 80 and 443 are scanned. You can use either `-p` or `--port` to set custom ports to check.

```bash
IPMarcher -s 192.168.1.0 -e 192.168.1.254 --port 80 443 8443
```

``` bash
IPMarcher -s 192.168.1.0 -e 192.168.1.254 --port 80 --port 443 --port 8443
```

### Supported platforms
* Windows
* MacOS 
* Linux

This tool has been only been tested on MacOS, but should work in any of the OSes above.