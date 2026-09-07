# DEVELOPER NOTE 

Project SslStreamTests_WiFi is not building nor configured because it depends on System.Device.Wifi, which in turn, depends on System.Net.
In order to build and run the tests in this project, you need to add a reference to System.Device.Wifi to it.