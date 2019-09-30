# NI Dev Labs - Web Service Runner

Example application which uses GLL exports as Web Services

## Building

Update the InstallLocations.targets file to point to the installed LabVIEW NXG runtime directory. You may need to run Visual Studio as an administrator if the runtime directory is in a protected location.
Open and build the WebServiceRunner.sln solution. This will copy several files to the LabVIEW NXG runtime directory.

## Runnning

To run the server, open the WebServiceHost.exe application that was copied to the LabVIEW NXG runtime directory.

Once running the server will display a tray icon that you can double click on to bring up a configuration dialog that you can use to setup where the server looks for GLLs and what ports it listens on.

### Creating Web Services

Currently you can create GLLs with VI exports that work as HTTP get methods.
Use LabVIEW to create a GLL with VI Exports.

The exported HTTP Get VIs can have string inputs that will be mapped to the HTTP URL parameters. The runner will map, case insensitively, any URL parameters to string inputs on the VI. If parameters are not found on the VI and errror will be returned from the request.

The exported HTTP Get VIs can have an output named "Response". The value of the response will be converted to JSON and return as the response to the request.

The exported HTTP Get VI can have an Error Out output named "ErrorOut". If ErrorOut indicates an error, the HTTP status code will be set to the code of the error cluster. The HTTP status description will be set to the message of the error cluster.

Once the GLL is built you need to create a .config file with the same name as the GLL. The .config file is an XML file which lists the VIs you want to act as HTTP Get methods and what url path the VIs should respond to.

The .config file should look something like this:

``` xml
<?xml version="1.0" encoding="utf-8"?>
<WebServiceRegistrationInfo xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
  <RegisteredVIs>
    <VIRegistrationInfo>
      <Type>HttpGetMethod</Type>
      <VIComponentName>SimpleAPI::Method1.gvi</VIComponentName>
      <UrlPath>TestMethods/Method</UrlPath>
    </VIRegistrationInfo>
  </RegisteredVIs>
</WebServiceRegistrationInfo>
```

For every exported VI in the GLL that you want to be an HTTP Get method you need to have a corresponding VIRegistrationInfo entry.

Once you have created the GLL and matching .config file you should put them in a directory under the location you specified in the server configuration dialog. By default the server will look in a WebServiceLibraries directory in the LabVIEW NXG runtime directory.

Once you add new web service GLLs you need to restart the server so that it will pick up the new registered VIs.
