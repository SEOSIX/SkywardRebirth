using UnityEngine;
using UnityEditor;
using System.IO;

public class TextToMat : EditorWindow
{
    [MenuItem("Tools/Créer Matériaux depuis Textures")]
    static void CreateMaterials()
    {
        Object[] selectedTextures = Selection.GetFiltered(
            typeof(Texture2D), SelectionMode.Assets
        );

        if (selectedTextures.Length == 0)
        {
            EditorUtility.DisplayDialog("Erreur", 
                "Textures a selectionner", "OK");
            return;
        }

        Shader hdrpShader = Shader.Find("HDRP/Lit");

        if (hdrpShader == null)
        {
            EditorUtility.DisplayDialog("Erreur", 
                "Shader HDRP/Lit introuvable !", "OK");
            return;
        }

        int count = 0;
        foreach (Object tex in selectedTextures)
        {
            string texPath = AssetDatabase.GetAssetPath(tex);
            string matPath = texPath.Replace(".png", ".mat")
                .Replace(".jpg", ".mat");

            if (File.Exists(Application.dataPath + 
                            matPath.Replace("Assets", ""))) continue;

            // 1. Crée et sauvegarde d'abord
            Material mat = new Material(hdrpShader);
            AssetDatabase.CreateAsset(mat, matPath);
            AssetDatabase.SaveAssets();

            // 2. Recharge le matériau depuis le disque
            Material savedMat = AssetDatabase.LoadAssetAtPath<Material>(matPath);

            // 3. Assigne la texture sur le matériau rechargé
            savedMat.SetTexture("_BaseColorMap", tex as Texture2D);

            // 4. Marque comme modifié
            EditorUtility.SetDirty(savedMat);
            count++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Succès !", 
            $"{count} matériaux créés", "OK");
    }
}