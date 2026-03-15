using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootstepSwapper : MonoBehaviour
{
    
    
    private TerrainChecker checker;
    private FPSController playerController;
    private string currentLayer;
    public FootstepCollection[] terrainFootstepCollections;

    // Start is called before the first frame update
    void Start()
    {
        checker = new TerrainChecker();
        playerController = GetComponent<FPSController>();
    }

    public void CheckLayers()
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position, Vector3.down, out hit, 3))
        {
            if (hit.transform.GetComponent<Terrain>()!= null)
            {
                Terrain t = hit.transform.GetComponent<Terrain>();

                if (currentLayer != checker.GetLayerName(transform.position, t))
                {
                    currentLayer = checker.GetLayerName(transform.position, t);

                    foreach(FootstepCollection collection in terrainFootstepCollections)
                    {
                        if(currentLayer == collection.name)
                        {
                            playerController.SwapFootsteps(collection);
                        }
                    }
                }
            }
            if(hit.transform.GetComponent<SurfaceType>()!= null)
            {
                FootstepCollection collection = hit.transform.GetComponent<SurfaceType>().footstepCollection;
                currentLayer = collection.name;
                playerController.SwapFootsteps(collection);
            }
        }
    }
    
}
