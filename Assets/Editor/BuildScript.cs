using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

// Builds the Basic Connection sample as an Android Gradle project (Unity-as-a-Library export).
// Invoked headless: Unity -batchmode -quit -projectPath sample/unity -executeMethod BuildScript.ExportAndroid
public static class BuildScript
{
    private const string ScenePath = "Assets/Scenes/BasicConnection.unity";

    public static void ExportAndroid()
    {
        PlayerSettings.companyName = "MoveLab";
        PlayerSettings.productName = "Connections Sample";
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "studio.movelab.connectionssample");
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        // The SDK requires minSdk 26.
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;

        EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
        EditorUserBuildSettings.exportAsGoogleAndroidProject = true;
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);

        string outPath = Path.GetFullPath("AndroidExport");
        if (Directory.Exists(outPath)) Directory.Delete(outPath, true);

        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = new[] { ScenePath },
            locationPathName = outPath,
            target = BuildTarget.Android,
            targetGroup = BuildTargetGroup.Android,
            options = BuildOptions.None
        });

        if (report.summary.result != BuildResult.Succeeded)
        {
            Debug.LogError("[BuildScript] Export FAILED: " + report.summary.result);
            if (Application.isBatchMode) EditorApplication.Exit(1);
            return;
        }

        Debug.Log("[BuildScript] Export succeeded -> " + outPath);
        if (Application.isBatchMode) EditorApplication.Exit(0);
    }
}
