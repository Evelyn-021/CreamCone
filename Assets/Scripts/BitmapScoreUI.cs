using UnityEngine;
using UnityEngine.UI;

public class BitmapScoreUI : MonoBehaviour
{
    public int score = 0;
    public static BitmapScoreUI Instance;
    public bool isScoreCounter = false;
    [Header("UI")]
    public Transform numbersContainer;
    public GameObject digitPrefab;

    [Header("Sprites de números")]
    public Sprite[] numberSprites; // 0,1,2,3,4,5,6,7,8,9

    public int scoreDigits = 6;


    private void Awake()
{
    if (isScoreCounter)
    {
        Instance = this;
    }
}
    private void Start()
    {
        UpdateScoreUI();
    }

    public void AddScore(int amount)
    {
        score += amount;
        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        foreach (Transform child in numbersContainer)
        {
            Destroy(child.gameObject);
        }

        string scoreText = score.ToString().PadLeft(scoreDigits, '0');

        foreach (char c in scoreText)
        {
            int number = c - '0';

            GameObject digit = Instantiate(digitPrefab, numbersContainer);
            Image img = digit.GetComponent<Image>();
            img.sprite = numberSprites[number];
            img.SetNativeSize();
        }
    }
}