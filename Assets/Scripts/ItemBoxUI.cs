using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ItemBoxUI : MonoBehaviour
{
    [SerializeField] private Sprite inactiveItemBox;
    [SerializeField] private Sprite activeItemBox;
    [SerializeField] private Image containedItemImage;

    public Image ContainedItemImage => containedItemImage;

    private Image _image;

    // Start is called before the first frame update
    void Start()
    {
        _image = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetSelected(bool isSelected)
    {
        _image.sprite = isSelected ? activeItemBox : inactiveItemBox;
    }
}
