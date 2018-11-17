#load "./build.config.cake"
#load "./build.tools.cake"
#load "./build.targets.cake"

var outputRootPath = Argument("outputRoot", "./../Publish");
var buildConfigurationFile = Argument("buildConfig", "./configurations/build.dev.json");
var projectsFile = Argument("projectsFile", "./configurations/projects.json");

Task(BuildTargets.Clean)
  .Does(() =>{
    CleanDirectory(buildScheme.GetRootOutputFolder());
});

Task(BuildTargets.LoadConfiguration)
  .Does(() =>{
    LoadBuildScheme(outputRootPath, projectsFile, buildConfigurationFile);
});

Task(BuildTargets.BuildAllProjects)
  .Does(()=>
  {
    RunInParallel(EnumerableConcat(
        buildScheme.WebSites.Select(x => (Action)(() => BuildAngularProject(buildScheme, x))),
        buildScheme.Databases.Select(x => (Action)(() => BuildDatabaseProject(buildScheme, x))),
        GetSingleActionAsEnumerable(
          buildScheme.WebApplications.Select(x => (Action)(() => BuildWebApplication(buildScheme, x))),
          buildScheme.WindowsApplications.Select(x => (Action)(() => BuildWindowsApplication(buildScheme, x)))
      )));
  });

Task(BuildTargets.Run)
  .IsDependentOn(BuildTargets.LoadConfiguration)
  .IsDependentOn(BuildTargets.Clean)
  .IsDependentOn(BuildTargets.BuildAllProjects)
  .Does(() =>{
    Information("Build completed succesfully");
  });