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
    public bool autoLayoutHud = true;
    public float digitScale = 2f;

    [Header("Sprites de numeros")]
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
        if (isScoreCounter)
        {
            score = GameSession.CurrentScore;
        }

        ApplyHudLayout();
        UpdateScoreUI();
    }

    public void AddScore(int amount)
    {
        if (isScoreCounter)
        {
            GameSession.Ensure().AddScore(amount);
            score = GameSession.CurrentScore;
        }
        else
        {
            score += amount;
        }

        UpdateScoreUI();
    }

    public void UpdateScoreUI()
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
            img.preserveAspect = true;
            img.raycastTarget = false;

            RectTransform digitRect = digit.GetComponent<RectTransform>();
            Vector2 digitSize = numberSprites[number].rect.size * digitScale;
            digitRect.sizeDelta = digitSize;

            LayoutElement layout = digit.GetComponent<LayoutElement>();
            if (layout == null)
            {
                layout = digit.AddComponent<LayoutElement>();
            }

            layout.preferredWidth = digitSize.x;
            layout.preferredHeight = digitSize.y;
        }
    }

    private void ApplyHudLayout()
    {
        if (!autoLayoutHud || numbersContainer == null) return;

        RectTransform rect = numbersContainer as RectTransform;
        if (rect == null) return;

        rect.localScale = Vector3.one;
        rect.sizeDelta = new Vector2(180f, 28f);

        HorizontalLayoutGroup layout = numbersContainer.GetComponent<HorizontalLayoutGroup>();
        if (layout != null)
        {
            layout.spacing = 2f;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            layout.childScaleWidth = false;
            layout.childScaleHeight = false;
        }

        string containerName = numbersContainer.name;

        if (containerName.Contains("Score"))
        {
            SetRect(rect, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -18f), new Vector2(0f, 1f));
        }
        else if (containerName.Contains("Level"))
        {
            SetRect(rect, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-88f, -18f), new Vector2(1f, 1f));
        }
        else if (containerName.Contains("Lives"))
        {
            SetRect(rect, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(24f, 18f), new Vector2(0f, 0f));
        }

        CanvasScaler scaler = numbersContainer.GetComponentInParent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(800f, 600f);
            scaler.matchWidthOrHeight = 0.5f;
        }
    }

    private void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 pivot)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.anchoredPosition = anchoredPosition;
        rect.pivot = pivot;
    }
}
