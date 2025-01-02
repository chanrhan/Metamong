using UnityEditor;
using System.IO;
using System.Diagnostics;

public class CustomCsprojPostprocessor : AssetPostprocessor
{
    private static void OnGeneratedCSProjectFiles()
    {
        string projectPath = Path.Combine(Directory.GetCurrentDirectory(), "Assembly-CSharp.csproj");
        if (File.Exists(projectPath))
        {
            string content = File.ReadAllText(projectPath);
            // 필요한 변경을 적용
            content = content.Replace("<TargetFramework>netstandard2.1</TargetFramework>", "<TargetFramework>net6.0</TargetFramework>");
            File.WriteAllText(projectPath, content);
            
        }
    }
}