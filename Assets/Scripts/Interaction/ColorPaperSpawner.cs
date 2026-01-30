using UnityEngine;
using System.Collections.Generic;

public class ColorPaperSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject paperPrefab;  // Paper with PaperGrabbable component
    
    [Header("Spawn Settings")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float spawnHeight = 1.2f;
    
    [Header("Materials")]
    [SerializeField] private Material greenMaterial;
    [SerializeField] private Material blueMaterial;
    [SerializeField] private Material redMaterial;
    [SerializeField] private Material yellowMaterial;
    
    private Dictionary<string, Material> colorMaterials;

    void Awake()
    {
        // Map color names to materials
        colorMaterials = new Dictionary<string, Material>
        {
            { "Green", greenMaterial },
            { "Blue", blueMaterial },
            { "Red", redMaterial },
            { "Yellow", yellowMaterial }
        };
    }

    public GameObject SpawnPaper(string color)
    {
        // Instantiate paper at spawn point
        Vector3 spawnPos = spawnPoint.position + Vector3.up * spawnHeight;
        GameObject paper = Instantiate(paperPrefab, spawnPos, Quaternion.identity);

        // Set material
        Transform a4Paper = paper.transform.Find("A4Paper");
        if (a4Paper != null)
        {
            Renderer renderer = a4Paper.GetComponent<Renderer>();
            if (renderer != null && colorMaterials.ContainsKey(color))
            {
                renderer.material = colorMaterials[color];
            }
        }

        paper.transform.rotation = Quaternion.Euler(0f, -90f, 0f);

        // Set metadata
        PaperGrabbable grabbable = paper.GetComponent<PaperGrabbable>();
        if (grabbable != null)
        {
            grabbable.PaperColor = color;
        }

        paper.name = $"Paper_{color}";

        return paper;
    }
}