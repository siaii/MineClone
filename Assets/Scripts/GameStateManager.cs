using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    private const float maxTime = 24000;

    [SerializeField] private LightingManager _lightingManager;
    
    //Game data
    [SerializeField][Range(0,maxTime)]private float TimeOfDay = 10000;

    [SerializeField] private Gamemode gamemode = Gamemode.CREATIVE;

    public Gamemode GameMode
    {
        get => gamemode;
        private set => gamemode = value;
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (_lightingManager == null)
        {
            Debug.LogError("No Lighting Manager");
            return;
        }
        
        if (Application.isPlaying)
        {
            TimeOfDay += 20 * Time.deltaTime;
            TimeOfDay %= maxTime;
            _lightingManager.UpdateLighting(TimeOfDay/maxTime);
        }
        else
        {
            _lightingManager.UpdateLighting(TimeOfDay/maxTime);
        }
    }
}
