using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class VRMenuManager : MonoBehaviour
{
    [Header("Menu Settings")]
    public GameObject menuPanel;
    public Button closeMenuButton;
    public Button importMapsButton;

    [Header("360° Image Settings")]
    public string imagesFolderName = "VR360Images";

    [Header("UI References")]
    public Transform scrollContent;
    public GameObject thumbnailPrefab;
    
    private string imagesPath;
    public Renderer sphereRenderer;

    private List<Texture2D> loadedTextures = new List<Texture2D>();

    void Start()
    {
        imagesPath = Path.Combine(Application.persistentDataPath, imagesFolderName);
        CreateImagesFolder();
        
        // Setup standard buttons safely
        if(closeMenuButton != null) {
            closeMenuButton.onClick.AddListener(CloseMenu);
            SetupUIButtonForCardboard(closeMenuButton);
        }

        if(importMapsButton != null) {
            importMapsButton.onClick.AddListener(ImportMapsAction);
            SetupUIButtonForCardboard(importMapsButton);
        }

        ShowMenu();
    }

    void SetupUIButtonForCardboard(Button button)
    {
        // 1. Add BoxCollider for Gaze detection
        BoxCollider collider = button.gameObject.GetComponent<BoxCollider>();
        if (collider == null)
        {
            collider = button.gameObject.AddComponent<BoxCollider>();
        }

        // 2. Match Collider size to the Button's RectTransform
        RectTransform rectTransform = button.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            collider.size = new Vector3(rectTransform.rect.width, rectTransform.rect.height, 1f);
        }

        // 3. Add Controller script
        UIButtonCardboardController controller = button.gameObject.GetComponent<UIButtonCardboardController>();
        if (controller == null)
        {
            controller = button.gameObject.AddComponent<UIButtonCardboardController>();
        }
        controller.Initialize(button);
    }

    public void ShowMenu()
    {
        if (menuPanel != null) menuPanel.SetActive(true);
    }

    public void CloseMenu()
    {
        menuPanel.SetActive(false);
    }

    public void ImportMapsAction()
    {
        CreateImagesFolder();
        StartCoroutine(LoadAllImages());
    }

    private void CreateImagesFolder()
    {
        try
        {
            if (!Directory.Exists(imagesPath)) Directory.CreateDirectory(imagesPath);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error creating folder: {e.Message}");
        }
    }

    private IEnumerator LoadAllImages()
    {
        string[] imageFiles = Directory.GetFiles(imagesPath, "*.*", SearchOption.AllDirectories);
        loadedTextures.Clear();

        // Clear old thumbnails
        foreach (Transform child in scrollContent) 
            Destroy(child.gameObject);

        foreach (string filePath in imageFiles)
        {
            // Filter valid extensions
            if (!filePath.EndsWith(".jpg", System.StringComparison.OrdinalIgnoreCase) &&
                !filePath.EndsWith(".jpeg", System.StringComparison.OrdinalIgnoreCase) &&
                !filePath.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            byte[] imageBytes = null;
            try
            {
                imageBytes = File.ReadAllBytes(filePath);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error reading {filePath}: {e.Message}");
                continue;
            }

            Texture2D texture = new Texture2D(2, 2);
            if (texture.LoadImage(imageBytes))
            {
                // CRITICAL FIX 1: Apply texture to make it visible on GPU
                texture.Apply(); 
                
                loadedTextures.Add(texture);
                
                // Add thumbnail immediately inside the loop
                AddThumbnail(texture);
            }

            // Set first image as background
            if (loadedTextures.Count == 1 && sphereRenderer != null)
            {
                sphereRenderer.material.mainTexture = loadedTextures[0];
            }

            yield return null; 
        }
        Debug.Log("Finished loading. Total textures: " + loadedTextures.Count);
    }
    
    private void AddThumbnail(Texture2D texture)
    {
        GameObject thumbnailObj = Instantiate(thumbnailPrefab, scrollContent);
        
        RawImage img = thumbnailObj.GetComponentInChildren<RawImage>();
        if (img != null)
        {
            img.texture = texture;
            img.color = Color.white; 

            // --- CRITICAL FIX 2: FORCE MATERIAL VIA CODE ---
            // This bypasses the need to change it in the Editor.
            // It finds the "Sprites/Default" shader which renders properly in VR World Space.
            img.material = new Material(Shader.Find("Sprites/Default"));
        }
        else
        {
            Debug.LogError("Thumbnail Prefab is missing a RawImage component!");
        }

        Button btn = thumbnailObj.GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.AddListener(() =>
            {
                sphereRenderer.material.mainTexture = texture;
            });
            SetupUIButtonForCardboard(btn);
        }
    }
}