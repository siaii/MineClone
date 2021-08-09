using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraEffectController : MonoBehaviour
{
    [SerializeField] private GameObject TintLayer;

    [SerializeField] private PlayerController _playerController;

    private bool isTinted = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void SetWaterTint(bool isTinted)
    {
        TintLayer.SetActive(isTinted);
    }
    private void OnTriggerEnter(Collider other)
    {
        print("a");
        if (other.CompareTag("WaterChunk"))
        {
            isTinted = !isTinted;
            SetWaterTint(isTinted);
            
        } 
    }
}
