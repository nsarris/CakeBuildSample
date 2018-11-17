#addin "Cake.Npm"

#load "./build.config.cake"

void BuildWindowsApplication(BuildScheme GetAbsoluteProjectFolder, WindowsApplicationBuildSettings settings)
{
var msBuildSettings = new MSBuildSettings()
        .SetConfiguration(settings.BuildConfiguration)
        .SetMaxCpuCount(0)
        .SetVerbosity(Verbosity.Minimal)
        .UseToolVersion(MSBuildToolVersion.VS2017)
        .WithProperty("SolutionDir", $"{buildScheme.GlobalSettings.SolutionRoot}")
        .WithProperty("OutDir", $"{buildScheme.GetOutputFolder(settings)}")
        .WithProperty("OutputPath", $"/bin/{settings.BuildConfiguration}")
        .WithProperty("DebugSymbols", "false")
        .WithProperty("DebugType", "none")
        .WithProperty("GenerateDocumentation", "false")
        .WithTarget("ReBuild")
        ;
      
      MSBuild(buildScheme.GetAbsoluteProjectFile(settings), msBuildSettings);

      //var configDir = Directory($"{buildScheme.GetOutputFolder(settings)}/Configuration");
      //var configFiles = GetFiles($"{configDir}/*.json");
      //CopyFile($"{buildScheme.GlobalSettings.SolutionRoot}/{buildScheme.CommonConfigFolder}/{buildScheme.CommonConfigFile}", $"{configDir}/CommonConfig.json");
      //CopyFile($"{configDir}/{buildScheme.AppConfigFile}", $"{configDir}/Config.json");
      //DeleteFiles(configFiles);
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
    .WithProperty("CommonConfigFileName", buildScheme.CommonConfigFile)
    .WithProperty("ConfigFileName",buildScheme.AppConfigFile)
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
    //if (!string.IsNullOrEmpty(settings.BaseHref))
      //arguments.Add($"--base-href {settings.BaseHref}");
    //if (!string.IsNullOrEmpty(settings.Environment))
      //arguments.Add($"--environment={settings.Environment}");

    config.WorkingDirectory = workingDirectory;
    config.WithArguments(string.Join(" ", arguments)); 
  });
}