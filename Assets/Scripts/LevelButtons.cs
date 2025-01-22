using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelButtons : MonoBehaviour
{
    [SerializeField] private Button[] levelButtons;
    [SerializeField] private Sprite lockedSprite, unlockedSprite;

    void Start()
    {
        int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);

        for (int i = 0; i < levelButtons.Length; i++)
        {
            int levelIndex = i + 1;
            Button button = levelButtons[i];
            Image buttonImage = button.GetComponent<Image>();
            TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();

            if (levelIndex <= unlockedLevel)
            {
                // Level đã mở khóa
                button.interactable = true;
                buttonImage.sprite = unlockedSprite;

                button.onClick.AddListener(() => LoadLevel(levelIndex));
            }
            else
            {
                // Level chưa mở khóa
                button.interactable = false;
                buttonImage.sprite = lockedSprite;
            }

            if (buttonText)
                buttonText.text = levelIndex < 10 ? "0" + levelIndex : levelIndex.ToString();
        }
    }

    void Update()
    {

    }

    private void LoadLevel(int levelIndex) => GameManager.instance.SetCurrentLV(levelIndex);
}
