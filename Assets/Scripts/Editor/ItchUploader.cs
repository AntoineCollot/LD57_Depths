using System.IO;
using UnityEditor;
using UnityEngine;

public class ItchUploaderWindow : EditorWindow
{
    string buildFolder = "Builds";
    string gameFolder = "GameName_web_version";//how to get this easely?

    //Data path is assets, get parent
    string ProjectPath => System.IO.Directory.GetParent(Application.dataPath).FullName;
    string FolderToZip => Path.Combine(ProjectPath, buildFolder, gameFolder);
    string ZipOutput => Path.Combine(ProjectPath, buildFolder, gameFolder) + ".zip";
    string WinrarCommandLine => $"winrar a -r -ep1 -afzip {ZipOutput} {FolderToZip}";

    //no caps, _ for spaces
    string username = "yorsh";
    //page name on itch (name in url)
    string pageNameOnItch = "my-game";
    string ButlerCommandLine => $"butler push \"{ZipOutput}\" {username}/{pageNameOnItch}:html";

    // Add menu item named "My Window" to the Window menu
    [MenuItem("Window/ItchUploader")]
    public static void ShowWindow()
    {
        //Show existing window instance. If one doesn't exist, make one.
        EditorWindow.GetWindow(typeof(ItchUploaderWindow));
    }

    void OnGUI()
    {
        GUILayout.Label("Build Location", EditorStyles.boldLabel);
        buildFolder = EditorGUILayout.TextField("Build Folder", buildFolder);
        gameFolder = EditorGUILayout.TextField("Game Folder", gameFolder);

        GUILayout.Label("Itch Settings", EditorStyles.boldLabel);
        username = EditorGUILayout.TextField("Itch Username", username).ToLower();
        pageNameOnItch = EditorGUILayout.TextField("Page Name (name in url)", pageNameOnItch);

        if (GUILayout.Button("Upload!"))
        {
            RunZip();
        }
    }

    bool RunZip()
    {
        if (!System.IO.Directory.Exists(FolderToZip))
        {
            Debug.LogError($"Folder {FolderToZip} not found...");
            return false;
        }

        Debug.Log($"<color=#42f57e>Zipping {FolderToZip} to {ZipOutput}</color>");
        Debug.Log($"<color=#32a56e>Executing: {WinrarCommandLine}</color>");
        System.Diagnostics.Process.Start("CMD.exe", "/K " + WinrarCommandLine);
        return true;
    }

    bool RunButler()
    {
        if (!System.IO.File.Exists(FolderToZip))
        {
            Debug.LogError($"Zip {ZipOutput} not found...");
            return false;
        }

        Debug.Log($"<color=#32a56e>Executing: {ButlerCommandLine}</color>");
        System.Diagnostics.Process.Start("CMD.exe", "/K " + ButlerCommandLine);

        return true;
    }
}