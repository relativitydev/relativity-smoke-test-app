# relativity-smoke-test-app 

> **Warning**
>
> This repository has been archived. It has not been updated for several years and uses a now-defunct API (Relativity Services API).

This is a test suite &amp; RAP used to check the correctness of the Relativity DevVM.

There is no formal process for running these tests or building the associated RAP (not sure if any such public instructions exist), but this suite may be useful so long as we still provide DevVM releases.

## Building

The projects are built in Visual Studio. They have been most recently tested &amp; run using Visual Studio 2022.

## Running Tests

All the appropriate NuGet packages should be added for running tests directly from Visual Studio 2022, so you should not need to install any extensions.

1. Create a local DevVM &amp; ensure it is licensed. You may want to create a checkpoint.
1. Upload the latest copy of the test RAP in the [Application](./Application) folder to the DevVM's Application Library.
1. Modify the following constants in [TestConstants.cs](./SourceCode/SmokeTest.Tests/TestConstants.cs) as appropriate:
    1. `ServerName`: IP address of your DevVM.
    1. `SqlInstance`: Full SQL Server instance name on your DevVM. The default should be correct for any DevVMs created since March 2022, but for earlier ones use the appropriate instance name rather than `EDDSINSTANCE001`.
    1. `WorkspaceArtifactId`: Artifact ID of the workspace used to create objects. Must already exist on the DevVM.
1. Run the tests from the Test Explorer in Visual Studio, or use `dotnet`:

       > dotnet test .\SourceCode\SmokeTest.sln

## Maintainers

This repository is maintained by the Developer Environments team, which also owns the Relativity DevVM.

You may create a GitHub Issue in this repository to file a defect or request a feature, or open a PR.