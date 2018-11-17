#load "./build.tasks.cake"

Task("DebugBuild").IsDependentOn(BuildTargets.Run);

RunTarget(BuildTargets.Run);