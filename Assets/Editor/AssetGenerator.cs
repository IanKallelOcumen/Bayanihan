using UnityEngine;
using UnityEditor;
using System.IO;

public class AssetGenerator : EditorWindow
{
    const string ArtPath = "Assets/Art";

    [MenuItem("Pitstop Panic/Regenerate Placeholders")]
    public static void GenerateAll()
    {
        if (!Directory.Exists(ArtPath)) Directory.CreateDirectory(ArtPath);

        // Vehicles
        CreateTexture("CarBroken", 512, 256, new Color(0.8f, 0.2f, 0.2f)); // Red Car
        CreateTexture("CarFixed", 512, 256, new Color(0.2f, 0.8f, 0.2f)); // Green Car
        
        CreateTexture("ScooterBroken", 256, 256, new Color(0.8f, 0.4f, 0.1f)); // Orange Scooter
        CreateTexture("ScooterFixed", 256, 256, new Color(0.2f, 0.8f, 0.4f)); // Green Scooter

        // Tools (Different Colors for identification)
        CreateTexture("Tool_Wrench", 128, 128, new Color(0.6f, 0.6f, 0.7f)); // Grey
        CreateTexture("Tool_Jack", 128, 128, new Color(0.9f, 0.2f, 0.2f)); // Red
        CreateTexture("Tool_OilCan", 128, 128, new Color(0.9f, 0.8f, 0.1f)); // Yellow
        CreateTexture("Tool_Screwdriver", 128, 128, new Color(0.2f, 0.8f, 0.2f)); // Green
        CreateTexture("Tool_Funnel", 128, 128, Color.white);
        CreateTexture("Tool_Multimeter", 128, 128, new Color(1f, 0.5f, 0f)); // Orange

        // Backgrounds
        CreateTexture("GarageBg", 1080, 1920, new Color(0.15f, 0.15f, 0.18f)); // Dark Blue-Grey
        CreateTexture("VictoryBg", 1080, 1920, new Color(0.1f, 0.2f, 0.1f)); // Dark Green

        // Parts
        CreateTexture("Wheel", 128, 128, Color.black);

        AssetDatabase.Refresh();
        Debug.Log("Placeholder Assets Generated in Assets/Art!");
    }

    static void CreateTexture(string name, int width, int height, Color color)
    {
        Texture2D tex = new Texture2D(width, height);
        Color[] pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = color;
        
        // Add a simple border
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (x < 4 || x > width - 5 || y < 4 || y > height - 5)
                    tex.SetPixel(x, y, Color.black);
                else
                    tex.SetPixel(x, y, color);
            }
        }
        
        tex.Apply();
        byte[] bytes = tex.EncodeToPNG();
        File.WriteAllBytes(Path.Combine(ArtPath, name + ".png"), bytes);

        // Import settings
        string assetPath = Path.Combine(ArtPath, name + ".png");
        AssetDatabase.ImportAsset(assetPath);
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.SaveAndReimport();
        }
    }
}
