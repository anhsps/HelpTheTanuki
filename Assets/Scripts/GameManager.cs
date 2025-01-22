using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }
    public static int level = 1;

    [SerializeField] private GameObject winMenu, loseMenu;
    [SerializeField] private TextMeshProUGUI lvText;
    [SerializeField] private AudioSource win_audio, lose_audio;
    [SerializeField] private GameObject buttonNext, buttonRetry;

    private void Awake()
    {
        if (instance != null && instance != this)
            Destroy(this);
        else instance = this;

        lvText.text = "LEVEL " + (level < 10 ? "0" + level : level);

        bool isMaxLevel = (level == 20);
        buttonRetry.SetActive(isMaxLevel);
        buttonNext.SetActive(!isMaxLevel);
    }

    void Start()
    {

    }

    void Update()
    {
        
    }

    public void Retry() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

    public void NextLV()
    {
        level++;
        Invoke("Retry", 0.5f);
    }

    public void GameWin()
    {
        UnlockNextLevel(level);
        StartCoroutine(GameDeplay(winMenu, win_audio));
    }

    public void GameLose()
    {
        StartCoroutine(GameDeplay(loseMenu, lose_audio));
    }

    public IEnumerator GameDeplay(GameObject gameObject, AudioSource audio)
    {
        yield return new WaitForSeconds(0.5f);
        gameObject.SetActive(true);
        if (SoundManager.instance.soundEnabled)
            audio.Play();
    }

    public void UnlockNextLevel(int currentLevel)
    {
        int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);
        if (currentLevel >= unlockedLevel)
        {
            PlayerPrefs.SetInt("UnlockedLevel", currentLevel + 1);
            PlayerPrefs.Save();
        }
    }

    public void SetCurrentLV(int levelIndex)
    {
        level = levelIndex;
        Invoke("Retry", 0.25f);
    }

    public int GetCurrentLevel() => level;
}
