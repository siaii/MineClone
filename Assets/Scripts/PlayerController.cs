using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private Camera mainCam;

    [SerializeField] private float verticalSens = 1f;

    [SerializeField] private float horizontalSens = 1f;

    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float playerTerminalVelocity = -25f;
    [SerializeField] private float gravityConst = 9.8f;
    
    [SerializeField] private float jumpHeight = 1.125f;

    [SerializeField] private InventoryView _inventoryView;

    private CharacterController _characterController;
    private Vector3 _playerVelocity;
    private Vector3 _downVelocity = new Vector3(0,0,0);

    private float rotationY = 0;

    private bool isInWater = false;

    public bool IsInWater
    {
        get => isInWater;
        private set => isInWater = value;
    }
    
    void Start()
    {
        _characterController = GetComponent<CharacterController>();

    }

    // Update is called once per frame
    void Update()
    {
        //Don't process player movement and camera if inventory is active
        if (_inventoryView.IsInventoryActive)
            return;
        ProcessCamera();
        ProcessMovement();
    }

    private void ProcessMovement()
    {
        Vector3 _playerVelocity =
            new Vector3(Input.GetAxis("Horizontal") * moveSpeed, 0, Input.GetAxis("Vertical") * moveSpeed);
        Vector3.ClampMagnitude(_playerVelocity, moveSpeed);

        //Transform local direction vector into world vector
        _playerVelocity = transform.TransformDirection(_playerVelocity);

        if (!_characterController.isGrounded)
        {
            _downVelocity.y += -gravityConst * Time.deltaTime;
        }

        if (Input.GetButtonDown("Jump") && _characterController.isGrounded)
        {
            _downVelocity.y = Mathf.Sqrt(Mathf.Abs(2 * jumpHeight * gravityConst));
        }

        //Clamp to terminal velocity
        _downVelocity.y = Mathf.Clamp(_downVelocity.y, playerTerminalVelocity, Mathf.Infinity);

        //Move takes in distance moved. Velocity times time equals distance
        _characterController.Move((_downVelocity + _playerVelocity) * Time.deltaTime);
    }

    private void ProcessCamera()
    {
        //Vertical rotation
        rotationY -= Input.GetAxis("Mouse Y") * verticalSens * Time.deltaTime;
        rotationY = Mathf.Clamp(rotationY, -90, 90);
        mainCam.transform.localRotation = Quaternion.Euler(new Vector3(rotationY, 0, 0));

        //Horizontal rotation
        transform.Rotate(Vector3.up, Input.GetAxis("Mouse X") * horizontalSens * Time.deltaTime);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("WaterChunk"))
        {
            IsInWater = true;
        } 
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("WaterChunk"))
        {
            IsInWater = false;
        } 
    }
}
