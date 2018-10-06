# mavshell.net

Mavlink shell tool, which use [MAVLink.net](https://github.com/asvol/mavlink.net) library.

Example:

### ### Listens to all MAVLink packets, groups it by message id and prints statistic: rate in seconds (Hz)
```
mavshell-net.exe mavlink
```
![listen](img/1.PNG)

### Prints vehicle info

```
mavshell-net.exe info
```

![info](img/2.PNG)

### Prints extended PX4 vehicle info
```
mavshell-net.exe px4info
```
![px4info](img/3.PNG)

### Real time vehicle params with text search by name and page navigation.
mavshell-net.exe params

![params](img/4.PNG)

## Versioning

Project is maintained under [the Semantic Versioning guidelines](http://semver.org/).





