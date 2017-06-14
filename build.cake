#addin nuget:?package=Cake.Git
#tool "nuget:?package=OctopusTools"
#addin nuget:https://ci.appveyor.com/nuget/cake-utility-4ufl9hamniq3/?package=Cake.Utility

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////
const string CopyrightText = "Copyright (c) {0}";

var target = Argument("target", "Default");
var configuration = EnvironmentVariable("CONFIGURATION") ?? Argument("configuration", "Release");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

var buildHelper = GetVersionHelper();
if (!buildHelper.IsAppVeyor){
	buildHelper.Branch = Argument("branch", GitBranchCurrent(".").FriendlyName);
	buildHelper.CommitMessageShort = GitLogTip(".").MessageShort;  //https://github.com/WCOMAB/Cake_Git
}

var versionInfo = buildHelper.GetNextVersion("1.0.0");
buildHelper.SetNextVersion(versionInfo);

Information("Build Target:" + target);

var solutionInfo = buildHelper.GetSolutionToBuild();

Information("{4} build of version {0} of {1} from {2} ({3}).", versionInfo.FullVersion, solutionInfo.SolutionFilename, buildHelper.Branch, buildHelper.CommitMessageShort, buildHelper.BuildEnvironmentName);


//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
	.WithCriteria(() => buildHelper.IsInteractiveBuild)
    .Does(() =>
{
	CleanDirectories(GetDirectories("./**/bin/" + configuration));
});

Task("Pack")
    .IsDependentOn("Nunit2")
    .Does(() =>
{
    var nuspecFiles = GetFiles("./**/*.nuspec");
    var output = Directory("./Artifacts"); 
    CreateDirectory(output); 
    foreach (var nuspec in nuspecFiles){
		Information(nuspec.FullPath);
		string fileToPack =  nuspec.ChangeExtension(".csproj").FullPath;
		if (nuspec.GetFilename().ToString()=="WayneBrantley.Extensions.Std.nuspec")
			fileToPack = nuspec.FullPath;
		NuGetPack(fileToPack,  new NuGetPackSettings { 
			Version = versionInfo.FullVersion,
            OutputDirectory = output.Path.FullPath,
            IncludeReferencedProjects = true,
			Properties = new Dictionary<string, string>() { { "Configuration", configuration } }
		});
	}

    //will not use nuspec right now
  	//var settings = new DotNetCorePackSettings
    //{
    //    Configuration = configuration,
    //    OutputDirectory = "./artifacts/",
	//	NoBuild = true,
	//	Verbose = true
    //};

    //DotNetCorePack("./*", settings);

});


Task("Nunit2")
	.IsDependentOn("Build")
    .Does(() =>
{
	//var testAssemblies = GetFiles("./**/bin/"+configuration+"/*.Tests.dll");
	//NUnit(testAssemblies, new NUnitSettings {  //http://cakebuild.net/api/Cake.Common.Tools.NUnit/NUnitSettings/
		//NoLogo = true,
		//NoResults = true,
    //});
});


Task("Restore-NuGet-Packages")
    .IsDependentOn("Clean")
    .Does(() =>
{
    DotNetCoreRestore(solutionInfo.SolutionFileAndPath);
    NuGetRestore(solutionInfo.SolutionFileAndPath);
});

Task("Build")
    .IsDependentOn("Patch-Assembly-Info")
    .Does(() =>
{
	var dotNetCoreBuildSettings = new DotNetCoreBuildSettings  //http://cakebuild.net/api/Cake.Common.Tools.DotNetCore.Build/DotNetCoreBuildSettings/
	{
		//Framework = "netcoreapp1.0",
		Verbose = false,
		Configuration = configuration,
		ArgumentCustomization = args=>args.Append("/property:Version="+versionInfo.FullVersion+";FileVersion="+versionInfo.RootVersion)
	};
	DotNetCoreBuild(solutionInfo.SolutionFileAndPath, dotNetCoreBuildSettings);
	// Use MSBuild
	var msBuildSettings = new MSBuildSettings {
		Verbosity = Verbosity.Minimal, //http://cakebuild.net/api/Cake.Core.Diagnostics/Verbosity/
		Configuration = configuration,
		//ToolVersion = MSBuildToolVersion.VS2015,
		//PlatformTarget = PlatformTarget.MSIL
	};
});

Task("Patch-Assembly-Info")
    .IsDependentOn("Restore-Nuget-Packages")
	.WithCriteria(() => buildHelper.IsCiBuildEnvironment)
    .Does(() =>
{
	buildHelper.PatchAllAssemblyInfo(versionInfo, CopyrightText);
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Pack");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);