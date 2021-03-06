#addin "Cake.FileHelpers"
#addin nuget:?package=Cake.Json
#addin nuget:?package=Newtonsoft.Json&version=9.0.1

using Newtonsoft.Json;
enum ProjectType {
    Solution,
    WebSite,
    WebApplication,
    WindowsApplication,
    Database
}

static class BuildTargets {
    public const string Clean = "Clean";
    public const string LoadConfiguration = "LoadBuildConfiguration";
    public const string BuildAllProjects = "BuildAllProjects";
    public const string Run = "Build";
}

class GlobalSettings {
    public string SolutionRoot { get; set; }
    public string GlobalOutputPath { get; set; }
}

class Projects {
    public Dictionary<string, SolutionMetadata> Solutions { get; set; }
    public Dictionary<string, ProjectMetadata> WebSites { get; set; }
    public Dictionary<string, ProjectMetadata> WebApplications { get; set; }
    public Dictionary<string, ProjectMetadata> WindowsApplications { get; set; }
    public Dictionary<string, ProjectMetadata> Databases { get; set; }
}
class BuildScheme {
    public GlobalSettings GlobalSettings { get; set; }
    public string EnvironmentName { get; set; }
    public string RootOutputFolder { get; set; }
    public string WebSiteOutputFolder { get; set; }
    public string WebApplicationOutputFolder { get; set; }
    public string WindowsApplicationOutputFolder { get; set; }
    public string DatabaseOutputFolder { get; set; }
    public string SolutionOutputFolder { get; set; }
    public List<SolutionBuildSettings> Solutions { get; set; }
    public List<AngularWebSiteBuildSettings> WebSites { get; set; }
    public List<WebApplicationBuildSettings> WebApplications { get; set; }
    public List<WindowsApplicationBuildSettings> WindowsApplications { get; set; }
    public List<DatabaseProjectBuildSettings> Databases { get; set; }

    public string GetProjectOutputRootFolder (ProjectType projectType) {
        switch (projectType) {
            case ProjectType.Solution:
                return SolutionOutputFolder;
            case ProjectType.WebSite:
                return WebSiteOutputFolder;
            case ProjectType.WebApplication:
                return WebApplicationOutputFolder;
            case ProjectType.WindowsApplication:
                return WindowsApplicationOutputFolder;
            case ProjectType.Database:
                return DatabaseOutputFolder;
            default:
                return "";
        }
    }

    public string GetRootOutputFolder () {
        return $"{GlobalSettings.GlobalOutputPath}/{RootOutputFolder}";
    }

    public string GetOutputFolder (ProjectBuildSettings project) {
        return GetOutputFolder(project.ProjectType, project.ProjectName);
    }

    public string GetSolutionOutputFolder (SolutionBuildSettings solution, bool projectFolders) {
        return GetOutputFolder(ProjectType.Solution, solution.SolutionName, projectFolders);
    }

    public string GetOutputFolder (ProjectType projectType, string projectName, bool projectFolder = true) {
        return $"{GetRootOutputFolder()}/{GetProjectOutputRootFolder(projectType)}/{(projectFolder ? projectName : ".")}";
    }

    public string GetAbsoluteProjectFolder (ProjectBuildSettings project) {
        return $"{GlobalSettings.SolutionRoot}/{project.ProjectMetadata.ProjectFolder}";
    }

    public string GetAbsoluteSolutionFolder (SolutionBuildSettings project) {
        return $"{GlobalSettings.SolutionRoot}/{project.SolutionMetadata.SolutionFolder}";
    }

    public string GetAbsoluteProjectFile (ProjectBuildSettings project) {
        return $"{GetAbsoluteProjectFolder(project)}/{project.ProjectMetadata.ProjectFile}";
    }

    public string GetAbsoluteSolutionFile (SolutionBuildSettings solution) {
        return $"{GetAbsoluteSolutionFolder(solution)}/{solution.SolutionMetadata.SolutionFile}";
    }

    public (string key, ProjectType projectType, ProjectMetadata metadata) GetProjectMetadataFromFolder(SolutionBuildSettings settings, string folderName)
    {
        return
        settings.SolutionMetadata.Databases.Where(x => folderName == System.IO.Path.GetFileNameWithoutExtension(x.Value.ProjectFile)).Select(x => (x.Key, ProjectType.Database, x.Value)).Concat(
        settings.SolutionMetadata.WebApplications.Where(x => folderName == System.IO.Path.GetFileNameWithoutExtension(x.Value.ProjectFile)).Select(x => (x.Key, ProjectType.WebApplication, x.Value)).Concat(
        settings.SolutionMetadata.WindowsApplications.Where(x => folderName == System.IO.Path.GetFileNameWithoutExtension(x.Value.ProjectFile)).Select(x => (x.Key, ProjectType.WindowsApplication, x.Value))))
        .FirstOrDefault();
    }

    public bool IsPublishedProjectFolder(SolutionBuildSettings settings, string folderName)
    {
        var project = GetProjectMetadataFromFolder(settings, folderName);

        return settings.PublishedProjects.Contains(project.key);
    }
}

abstract class ProjectBuildSettings {
    public ProjectMetadata ProjectMetadata { get; set; }
    public string ProjectName { get; set; }
    public abstract ProjectType ProjectType { get; }
}

class SolutionMetadata {
    public string SolutionFolder { get; set; }
    public string SolutionFile { get; set; }
    public Dictionary<string, ProjectMetadata> WebApplications { get; set; }
    public Dictionary<string, ProjectMetadata> WindowsApplications { get; set; }
    public Dictionary<string, ProjectMetadata> Databases { get; set; }
}

class ProjectMetadata {
    public string ProjectFolder { get; set; }
    public string ProjectFile { get; set; }
    public string PackageInstaller { get; set; }
    public string PackageFile { get; set; }
    public string BinaryFile { get; set; }
    public string Description { get; set; }
    public bool IsWindowsService { get; set; }
    public bool IsTopshelfService { get; set; }
}

class AngularWebSiteBuildSettings : ProjectBuildSettings {
    public override ProjectType ProjectType => ProjectType.WebSite;
    public bool NoOptimizer { get; set; }
    public bool IsProductionBuild { get; set; }
    public string Environment { get; set; }
    public string BaseHref { get; set; }
}

class SolutionBuildSettings {
    public string SolutionName { get; set; }
    public string BuildConfiguration { get; set; } = "Debug";
    public bool CreateSolutionFolder { get; set; } = true;
    public SolutionMetadata SolutionMetadata { get; set; }
    public List<string> PublishedProjects { get; set; } = new List<string>();
}

class WebApplicationBuildSettings : ProjectBuildSettings {
    public override ProjectType ProjectType => ProjectType.WebApplication;
    public string BuildConfiguration { get; set; }
}

class WindowsApplicationBuildSettings : ProjectBuildSettings {
    public override ProjectType ProjectType => ProjectType.WindowsApplication;
    public string BuildConfiguration { get; set; }
}

class DatabaseProjectBuildSettings : ProjectBuildSettings {
    public override ProjectType ProjectType => ProjectType.Database;
}

Projects projects;
BuildScheme buildScheme;
bool configurationLoaded = false;
void LoadBuildScheme (string outputRootPath, string projectsFile, string buildConfigurationFile) {
    if (configurationLoaded)
        return;

    var jsonSerializerSettings = new JsonSerializerSettings {
        ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver ()
    };

    projects = JsonConvert.DeserializeObject<Projects> (FileReadText (projectsFile), jsonSerializerSettings);
    buildScheme = JsonConvert.DeserializeObject<BuildScheme> (FileReadText (buildConfigurationFile), jsonSerializerSettings);

    //TODO: Assert configuration is valid

    buildScheme.GlobalSettings = new GlobalSettings {
        SolutionRoot = MakeAbsolute (Directory ("./..")).FullPath,
        GlobalOutputPath = MakeAbsolute (Directory (outputRootPath)).FullPath
    };

    buildScheme.Solutions.ForEach (x => {
        if (projects.Solutions.TryGetValue (x.SolutionName, out var metadata)) x.SolutionMetadata = metadata;
        else throw new KeyNotFoundException ($"Solution {x.SolutionName} not found in projects file");
    });

    buildScheme.WebSites.ForEach (x => {
        if (projects.WebSites.TryGetValue (x.ProjectName, out var metadata)) x.ProjectMetadata = metadata;
        else throw new KeyNotFoundException ($"WebSite {x.ProjectName} not found in projects file");
    });

    buildScheme.WebApplications.ForEach (x => {
        if (projects.WebApplications.TryGetValue (x.ProjectName, out var metadata)) x.ProjectMetadata = metadata;
        else throw new KeyNotFoundException ($"WebApplication {x.ProjectName} not found in projects file");
    });

    buildScheme.WindowsApplications.ForEach (x => {
        if (projects.WindowsApplications.TryGetValue (x.ProjectName, out var metadata)) x.ProjectMetadata = metadata;
        else throw new KeyNotFoundException ($"WindowsApplication {x.ProjectName} not found in projects file");
    });

    buildScheme.Databases.ForEach (x => {
        if (projects.Databases.TryGetValue (x.ProjectName, out var metadata)) x.ProjectMetadata = metadata;
        else throw new KeyNotFoundException ($"Database project {x.ProjectName} not found in projects file");
    });

    configurationLoaded = true;
}