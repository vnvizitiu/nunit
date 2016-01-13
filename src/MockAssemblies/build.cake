var target = Argument("target", "Default");
var configuration = Argument("configuration", "Debug");

Task("NuGetRestore")
    .Does(() =>
{
    NuGetRestore("./MockAssemblies.sln");
});

Task("Build")
    .IsDependentOn("NuGetRestore")
    .Does(() =>
{
    if(IsRunningOnUnix())
    {
        XBuild("./MockAssemblies.sln", new XBuildSettings()
            .SetConfiguration("Debug")
            .WithTarget("AnyCPU")
            .WithProperty("TreatWarningsAsErrors", "true")
            .SetVerbosity(Verbosity.Minimal)
        );
    }
    else
    {
        MSBuild("./MockAssemblies.sln", new MSBuildSettings()
            .SetConfiguration(configuration)
            .SetMSBuildPlatform(MSBuildPlatform.x86)
            .SetPlatformTarget(PlatformTarget.MSIL)
            .WithProperty("TreatWarningsAsErrors", "true")
            .SetVerbosity(Verbosity.Minimal)
            .SetNodeReuse(false)
        );
    }
});

Task("Default")
    .IsDependentOn("Build");

RunTarget(target);