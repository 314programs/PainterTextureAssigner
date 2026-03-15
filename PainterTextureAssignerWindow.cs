using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

#if UNITY_EDITOR
public class PainterTextureAssignerWindow : EditorWindow
{   
    //Main Mesh
    private GameObject mainMesh;
    //Folders
    private DefaultAsset materialsFolder;
    private DefaultAsset texturesFolder;

    private int materialCount = 0;
    private List<Material> materials;
    private List<List<Texture2D>> texturesOnMaterials;

    //Material checklist
    private bool checkAlbedo = true;
    private bool checkMetallic = true;
    private bool checkNormal = true;
    private bool checkHeight = false;
    private bool checkAO = false;
    private bool checkEmissive = false;

    private bool allValid = false;
    private bool hasValidated = false;

    [MenuItem("Tools/Painter Texture Assigner")]
    public static void OpenWindow()
    {
        GetWindow<PainterTextureAssignerWindow>(
            "Painter Texture Assigner"
        );
    }

    private void OnGUI()
    {
        GUILayout.Label("Folders", EditorStyles.boldLabel);

        EditorGUILayout.Space();

        mainMesh = (GameObject)EditorGUILayout.ObjectField(
            "Main Mesh",
            mainMesh,
            typeof(GameObject),
            false
        );

        materialsFolder = (DefaultAsset)EditorGUILayout.ObjectField(
            "Materials Folder",
            materialsFolder,
            typeof(DefaultAsset),
            false
        );

        texturesFolder = (DefaultAsset)EditorGUILayout.ObjectField(
            "Textures Folder",
            texturesFolder,
            typeof(DefaultAsset),
            false
        );

        EditorGUILayout.Space(10);

        GUILayout.Label("Required Texture Maps", EditorStyles.boldLabel);

        checkAlbedo = EditorGUILayout.Toggle("Albedo", checkAlbedo);
        checkMetallic = EditorGUILayout.Toggle("Metallic", checkMetallic);
        checkNormal = EditorGUILayout.Toggle("Normal", checkNormal);
        checkHeight = EditorGUILayout.Toggle("Height", checkHeight);
        checkAO = EditorGUILayout.Toggle("Ambient Occlusion", checkAO);
        checkEmissive = EditorGUILayout.Toggle("Emissive", checkEmissive);

        EditorGUILayout.Space(10);

        //Disable the button if either folder is not assigned
        using (new EditorGUI.DisabledScope(materialsFolder == null || texturesFolder == null || mainMesh == null))
        {
            if (GUILayout.Button("Validate Selection"))
            {
                ValidateFolders();
            }
        }

        using (new EditorGUI.DisabledScope(!allValid))
        {
            if (GUILayout.Button("Assign Textures"))
            {
                assignTextures();
            }
        }

        EditorGUILayout.Space(10);

        if (hasValidated)
        {
            if (allValid)
            {
                EditorGUILayout.HelpBox("All required textures are present for " + materialCount + " materials.", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("Some required textures are missing. Please check the console for details.", MessageType.Error);
            }
        }
    }

    private void ValidateFolders()
    {
        hasValidated = true;
        allValid = true;
        string meshName = mainMesh.name;
        string matPath = AssetDatabase.GetAssetPath(materialsFolder);
        string texPath = AssetDatabase.GetAssetPath(texturesFolder);

        if(materials != null) materials.Clear();
        materials = AssetDatabase.FindAssets("t:Material", new[] { matPath })
        .Select(g => AssetDatabase.LoadAssetAtPath<Material>(
            AssetDatabase.GUIDToAssetPath(g)))
        .Where(m => m != null)
        .ToList();

        var textures = AssetDatabase.FindAssets("t:Texture2D", new[] { texPath })
        .Select(g => AssetDatabase.LoadAssetAtPath<Texture2D>(
            AssetDatabase.GUIDToAssetPath(g)))
        .Where(t => t != null)
        .ToList();

        if(materials.Count == 0)
        {
            Debug.LogError("No materials found in the selected folder.");
            return;
        }
        materialCount = materials.Count;
        texturesOnMaterials = new List<List<Texture2D>>();

        for (int i = 0; i < materials.Count; i++)
        {
            texturesOnMaterials.Add(new List<Texture2D>(6));
            for (int j = 0; j < 6; j++)
            {
                texturesOnMaterials[i].Add(null);
            }
        } 
        
        //Naming formats for textures
        for (int i = 0; i < materials.Count; i++){
            if(checkAlbedo){
                var albedoTex = textures.FirstOrDefault(t => t.name == meshName + "_" + materials[i].name + "_Albedo");
                if(albedoTex == null){
                    Debug.LogError("Albedo texture not found for material: " + materials[i].name + "\n Check if "
                    + meshName + "_" + materials[i].name + "_Albedo exists in the textures folder.");
                    allValid = false;
                }
                else{
                    texturesOnMaterials[i][0] = albedoTex;
                }
            }
            if(checkMetallic){
                var metallicTex = textures.FirstOrDefault(t => t.name == meshName + "_" + materials[i].name + "_Metallic");
                if(metallicTex == null){
                    Debug.LogError("Metallic texture not found for material: " + materials[i].name + "\n Check if "
                    + meshName + "_" + materials[i].name + "_Metallic exists in the textures folder.");
                    allValid = false;
                }
                else{
                    texturesOnMaterials[i][1] = metallicTex;
                }
            }
            if(checkNormal){
                var normalTex = textures.FirstOrDefault(t => t.name == meshName + "_" + materials[i].name + "_Normal");
                if(normalTex == null){
                    Debug.LogError("Normal texture not found for material: " + materials[i].name + "\n Check if "
                    + meshName + "_" + materials[i].name + "_Normal exists in the textures folder.");
                    allValid = false;   
                }
                else{
                    texturesOnMaterials[i][2] = normalTex;
                }
            }
            if(checkHeight){
                var heightTex = textures.FirstOrDefault(t => t.name == meshName + "_" + materials[i].name + "_Height");
                if(heightTex == null){
                    Debug.LogError("Height texture not found for material: " + materials[i].name + "\n Check if "
                    + meshName + "_" + materials[i].name + "_Height exists in the textures folder.");
                    allValid = false;   
                }
                else{
                    texturesOnMaterials[i][3] = heightTex;
                }
            }
            if(checkAO){
                var aoTex = textures.FirstOrDefault(t => t.name == meshName + "_" + materials[i].name + "_AO");
                if(aoTex == null){
                    Debug.LogError("AO texture not found for material: " + materials[i].name + "\n Check if "
                    + meshName + "_" + materials[i].name + "_AO exists in the textures folder.");
                    allValid = false;
                }
                else{
                    texturesOnMaterials[i][4] = aoTex;
                }
            }
            if(checkEmissive){
                var emissiveTex = textures.FirstOrDefault(t => t.name == meshName + "_" + materials[i].name + "_Emissive");
                if(emissiveTex == null){
                    Debug.LogError("Emissive texture not found for material: " + materials[i].name + "\n Check if "
                    + meshName + "_" + materials[i].name + "_Emissive exists in the textures folder.");
                    allValid = false;
                }
                else{   
                    texturesOnMaterials[i][5] = emissiveTex;
                }
            }
        }
    }

    private void assignTextures()
    {
        for(int i = 0; i < materialCount; i++){
            materials[i].SetFloat("_Surface", 0); 
            materials[i].SetFloat("_Blend", 0);
            materials[i].DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
            materials[i].EnableKeyword("_SURFACE_TYPE_OPAQUE");

            if(checkAlbedo){
                if (materials[i].HasProperty("_BaseMap")){
                    materials[i].SetTexture("_BaseMap", null);
                    materials[i].SetTexture("_BaseMap", texturesOnMaterials[i][0]);
                }

                if (materials[i].HasProperty("_MainTex")){
                    materials[i].SetTexture("_MainTex", null);
                    materials[i].SetTexture("_MainTex", texturesOnMaterials[i][0]);
                }
            }
            if(checkMetallic){
                materials[i].SetTexture("_MetallicGlossMap", null);
                materials[i].SetTexture("_MetallicGlossMap", texturesOnMaterials[i][1]);    
            }
            if(checkNormal){
                var normalTexture = texturesOnMaterials[i][2];
                string normalPath = AssetDatabase.GetAssetPath(normalTexture);
                TextureImporter normalImporter = AssetImporter.GetAtPath(normalPath) as TextureImporter;
                if (normalImporter != null && normalImporter.textureType != TextureImporterType.NormalMap)
                {
                    normalImporter.textureType = TextureImporterType.NormalMap;
                    AssetDatabase.ImportAsset(normalPath);
                }
                materials[i].SetTexture("_BumpMap", null);
                materials[i].SetTexture("_BumpMap", normalTexture);
            }
            if(checkHeight){
                materials[i].SetTexture("_ParallaxMap", null);
                materials[i].SetTexture("_ParallaxMap", texturesOnMaterials[i][3]);
            }
            if(checkAO){
                materials[i].SetTexture("_OcclusionMap", null);
                materials[i].SetTexture("_OcclusionMap", texturesOnMaterials[i][4]);    
            }
            if(checkEmissive){
                materials[i].SetTexture("_EmissionMap", null);
                materials[i].SetTexture("_EmissionMap", texturesOnMaterials[i][5]);
            }
        }
    }
}
#endif