#addin "Cake.Npm"

#load "./build.config.cake"

void BuildSolution(BuildScheme buildScheme,SolutionBuildSettings settings)
{
  var outDir = $"{buildScheme.GetSolutionOutputFolder(settings, settings.CreateSolutionFolder)}";
  var tmpOurDir = $"{outDir}/tmp_{settings.SolutionName}";

  var config = new MSBuildSettings()
    .SetConfiguration(settings.BuildConfiguration)
    .SetMaxCpuCount(0)
    .UseToolVersion(MSBuildToolVersion.VS2017)
    .SetVerbosity(Verbosity.Normal)
    .WithProperty("SolutionDir", $"{buildScheme.GlobalSettings.SolutionRoot}")
    .WithProperty("OutDir", tmpOurDir)
    //.WithProperty("OutDirWasSpecified", "true")
    //.WithProperty("OutputPath", $"/bin/{settings.BuildConfiguration}")
    .WithProperty("GenerateProjectSpecificOutputFolder","true")
    .WithProperty("DeployOnBuild", "true")
    .WithProperty("DeployTarget", "Package")
    .WithProperty("MSDeployPublishMethod", "WMSVC")
    .WithProperty("AllowUntrustedCertificate", "true")
    .WithProperty("DebugSymbols", "false")
    .WithProperty("DebugType", "none")
    .WithProperty("GenerateDocumentation", "false")
    .WithProperty("AllowedReferenceRelatedFileExtensions", "none")
    .WithTarget("ReBuild")
    ;

  MSBuild(buildScheme.GetAbsoluteSolutionFile(settings), config);

  foreach(var projectDir in new System.IO.DirectoryInfo(tmpOurDir).GetDirectories())
  {
    var project = buildScheme.GetProjectMetadataFromFolder(settings, projectDir.Name);
    
    if (settings.PublishedProjects.Contains(project.key))
    {
      var targetDir = $"{outDir}/{buildScheme.GetProjectOutputRootFolder(project.projectType)}";
      System.IO.Directory.CreateDirectory(targetDir);
      projectDir.MoveTo($"{targetDir}/{project.key}");
    }
  }

  System.IO.Directory.Delete(tmpOurDir, true);
}

void BuildWindowsApplication(BuildScheme GetAbsoluteProjectFolder, WindowsApplicationBuildSettings settings)
{
var msBuildSettings = new MSBuildSettings()
        .SetConfiguration(settings.BuildConfiguration)
        .SetVerbosity(Verbosity.Minimal)
        .UseToolVersion(MSBuildToolVersion.VS2017)
        .WithProperty("SolutionDir", $"{buildScheme.GlobalSettings.SolutionRoot}")
        .WithProperty("OutDir", $"{buildScheme.GetOutputFolder(settings)}")
        .WithProperty("OutputPath", $"/bin/{settings.BuildConfiguration}")
        .WithProperty("DebugSymbols", "false")
        .WithProperty("DebugType", "none")
        .WithProperty("GenerateDocumentation", "false")
        .WithProperty("AllowedReferenceRelatedFileExtensions", "none")
        .WithProperty("BuildInParallel", "true")
        .SetMaxCpuCount(4)
        .WithTarget("ReBuild")
        ;
      
      MSBuild(buildScheme.GetAbsoluteProjectFile(settings), msBuildSettings);
}

void BuildWebApplication(BuildScheme buildScheme,WebApplicationBuildSettings settings)
{
  var config = new MSBuildSettings()
    .SetConfiguration(settings.BuildConfiguration)
    .SetMaxCpuCount(0)
    .UseToolVersion(MSBuildToolVersion.VS2017)
    .SetVerbosity(Verbosity.Normal)
    .WithProperty("SolutionDir", $"{buildScheme.GlobalSettings.SolutionRoot}")
    .WithProperty("OutDir", $"{buildScheme.GetOutputFolder(settings)}")
    .WithProperty("OutputPath", $"/bin/{settings.BuildConfiguration}")
    .WithProperty("DeployOnBuild", "true")
    .WithProperty("DeployTarget", "Package")
    .WithProperty("MSDeployPublishMethod", "WMSVC")
    .WithProperty("AllowUntrustedCertificate", "true")
    .WithProperty("DebugSymbols", "false")
    .WithProperty("DebugType", "none")
    .WithProperty("GenerateDocumentation", "false")
    .WithProperty("AllowedReferenceRelatedFileExtensions", "none")
    .WithTarget("ReBuild")
    ;

  MSBuild(buildScheme.GetAbsoluteProjectFile(settings), config);
}

void BuildDatabaseProject(BuildScheme buildScheme,DatabaseProjectBuildSettings settings)
{
  var config = new MSBuildSettings()
    .SetMaxCpuCount(0)
    .SetVerbosity(Verbosity.Normal)
    .UseToolVersion(MSBuildToolVersion.VS2017)
    .WithProperty("OutDir", $"{buildScheme.GetOutputFolder(settings)}")
    .WithProperty("DeployOnBuild", "true")
    .WithProperty("DeployTarget", "Package")
    .WithTarget("ReBuild")
    ;

  MSBuild(buildScheme.GetAbsoluteProjectFile(settings), config);
}

void BuildAngularProject(BuildScheme buildScheme,AngularWebSiteBuildSettings settings)
{
  var workingDirectory = buildScheme.GetAbsoluteProjectFile(settings);

  NpmInstall((config) => {
    config.WorkingDirectory = workingDirectory;
  });

  NpmRunScript($"build{settings.Environment}", config => {
    var arguments = new List<string>();
    if (settings.IsProductionBuild)
      arguments.Add("--prod");
    arguments.Add($"--output-path {buildScheme.GetOutputFolder(settings)}");

    config.WorkingDirectory = workingDirectory;
    config.WithArguments(string.Join(" ", arguments)); 
  });
}