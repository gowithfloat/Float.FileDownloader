# Float.FileDownloader [![Test](https://github.com/gowithfloat/Float.FileDownloader/actions/workflows/test.yml/badge.svg)](https://github.com/gowithfloat/Float.FileDownloader/actions/workflows/test.yml) [![NuGet](https://img.shields.io/nuget/v/Float.FileDownloader)](https://www.nuget.org/packages/Float.FileDownloader/)

This Xamarin library provides a convenient API for downloading HTTP responses to a file. Additionally, it provides a barebones concept of a "remote file" (e.g. a PDF or ZIP file on a server) and provides the logic necessary for downloading and caching the file locally.

## Building

This project can be built using [Visual Studio for Mac](https://visualstudio.microsoft.com/vs/mac/) or [Cake](https://cakebuild.net/). It is recommended that you build this project by invoking the bootstrap script:

    ./build.sh

There are a number of optional arguments that can be provided to the bootstrapper that will be parsed and passed on to Cake itself. See the [Cake build file](./build.cake) in order to identify all supported parameters.

    ./build.sh \
        --task=Build \
        --projectName=Float.FileDownloader \
        --configuration=Debug \
        --nugetUrl=https://nuget.org \
        --nugetToken=####

## Installing

### NuGet
This library is available as a NuGet via nuget.org.

## Testing

Note that xUnit reporting with Visual Studio can be a bit wonky, but it will run the tests. Whether or not they show in VS is seemingly random. For best results, run the tests from the command line as follows.

    dotnet test Float.FileDownloader.Tests/Float.FileDownloader.Tests.csproj

To get test reports in Nunit format:

    dotnet test Float.FileDownloader.Tests/Float.FileDownloader.Tests.csproj -l nunit

Test report should be published at `Float.FileDownloader.Tests/TestResults/TestResults.xml`.


## Usage

### Downloading HTTP response to file
At it's core, this library is simply for downloading an HTTP response to a file.

    using Float.FileDownloader;
    ...
    var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com/file.pdf");
    var response = await DownloadRequest.Download(request, "/absolute/path/to/file.pdf");
    // The file is download available at `/absolute/path/to/file.pdf`

### Downloading files
However, if you're downloading remote files (such as PDF or ZIP files), you may find it useful to use the `FileDownloadRequest` abstraction.

In your model layer, the model that represents a file should implement `IRemoteFile`. Additionally, your application must implement an `IRemoteFileProvider` to prepare an `HttpRequestMessage` (with a URL, authentication, caching headers, etc.).

Then, you can download the file like so:

    using Float.FileDownloader;
    ...
    var fileProvider = /* the `IFileProvider` */;
    var file = /* file model */;

    // Must be an absolute Uri to a local spot on disk
    var downloadDestination = new Uri("/path/to/file.pdf");

    // Optionally, you may receive updates about the status of the download
    var status = new DownloadStatus("My Download");

    var response = await FileDownloadRequest.DownloadFile(fileProvider, file, downloadDestination, status);

### Processing files
Additionally, you may want to process or manipulate the downloaded data before making it available to your application. This could include unzipping a zip file, resizing an image, etc.

Implement the `IRemoteFileProcessor` interface to manipulate the downloaded data before your application is notified of the finished download.

## License

All content in this repository is shared under an MIT license. See [license.md](./license.md) for details.
