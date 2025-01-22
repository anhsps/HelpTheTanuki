using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetBG : MonoBehaviour
{
    Image image;
    [SerializeField] private Sprite[] bgs;

    // Start is called before the first frame update
    void Start()
    {
        image = GetComponent<Image>();
        image.sprite = GameManager.instance.GetCurrentLevel() > 10 ? bgs[1] : bgs[0];
    }

    // Update is called once per frame
    void Update()
    {

    }
}
