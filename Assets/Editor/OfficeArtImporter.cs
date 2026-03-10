using UnityEditor;
using UnityEngine;
using System.IO;

public class OfficeArtImporter : EditorWindow
{
    [MenuItem("MoneySurvivor/Configure Office Sprites")]
    public static void ConfigureSprites()
    {
        string officePath = "Assets/Sprites/Office";
        if (!Directory.Exists(officePath)) return;

        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { officePath });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) continue;

            importer.textureType = TextureImporterType.Sprite;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.alphaIsTransparency = true;
            importer.alphaSource = TextureImporterAlphaSource.FromInput;

            // Set bottom pivot for office furniture to ground them correctly
            if (path.Contains("desk") || path.Contains("chair") || path.Contains("table") || path.Contains("coffee") || path.Contains("plant"))
            {
                TextureImporterSettings settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);
                settings.spriteAlignment = (int)SpriteAlignment.BottomCenter;
                settings.spritePivot = new Vector2(0.5f, 0f);
                importer.SetTextureSettings(settings);
            }
            importer.SaveAndReimport();
        }
        AssetDatabase.Refresh();
    }

    [MenuItem("MoneySurvivor/Process Office Sprites (Magenta to Transparent)")]
    public static void ProcessMagenta()
    {
        string[] folders = { "Assets/Sprites/Office", "Assets/Sprites" };
        foreach (string folder in folders)
        {
            if (!Directory.Exists(folder)) continue;
            string[] files = Directory.GetFiles(folder, "*.png");
            foreach (string file in files)
            {
                byte[] data = File.ReadAllBytes(file);
                Texture2D tex = new Texture2D(2, 2);
                if (!tex.LoadImage(data)) continue;

                Color32[] pixels = tex.GetPixels32();
                bool changed = false;
                
                // Capture any pixel that is "magenta-leaning" or near pure magenta
                for (int i = 0; i < pixels.Length; i++)
                {
                    Color32 c = pixels[i];
                    // Threshold to catch artifacts or slight color shifts near pure magenta
                    float dist = Mathf.Abs(c.r - 255) + Mathf.Abs(c.g - 0) + Mathf.Abs(c.b - 255);
                    bool isMagentaLeaning = (c.r > 150 && c.b > 150 && c.g < 100);

                    if (dist < 120 || isMagentaLeaning) 
                    {
                        pixels[i] = new Color32(0, 0, 0, 0);
                        changed = true;
                    }
                }

                if (changed)
                {
                    tex.SetPixels32(pixels);
                    tex.Apply();
                    File.WriteAllBytes(file, tex.EncodeToPNG());
                    Debug.Log($"[OfficeArtImporter] Processed pure magenta in: {file}");
                }
            }
        }
        AssetDatabase.Refresh();
    }
}
