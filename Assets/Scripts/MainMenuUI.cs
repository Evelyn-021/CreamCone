using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class MainMenuUI : MonoBehaviour
{
    [Header("Escena")]
    public string firstSceneName = "SampleScene";

    [Header("Bitmaps")]
    public Sprite titleSprite;

    [Header("Letras bitmap")]
    public Sprite digit1Sprite;
    public Sprite letterASprite;
    public Sprite letterGSprite;
    public Sprite letterISprite;
    public Sprite letterLSprite;
    public Sprite letterPSprite;
    public Sprite letterRSprite;
    public Sprite letterSSprite;
    public Sprite letterTSprite;

    [Header("Colores")]
    public Color backgroundColor = new Color(0.05f, 0.02f, 0.08f, 1f);
    public Color startGlowColor = new Color(1f, 0.85f, 0.18f, 0.9f);

    private readonly Dictionary<char, Sprite> fontMap = new Dictionary<char, Sprite>();
    private RectTransform titleRect;
    private GameObject menuOptions;
    private CanvasGroup startPromptGroup;
    private Controles menuControls;
    private bool isLoadingScene;

    private void Start()
    {
        Build();
    }

    private void OnEnable()
    {
        if (menuControls == null)
        {
            menuControls = new Controles();
        }

        menuControls.Enable();
        menuControls.Player.Jump.performed += OnStartInput;
        menuControls.Player.Attack.performed += OnStartInput;
    }

    private void OnDisable()
    {
        if (menuControls == null) return;

        menuControls.Player.Jump.performed -= OnStartInput;
        menuControls.Player.Attack.performed -= OnStartInput;
        menuControls.Disable();
    }

    public void Build()
    {
        BuildFontMap();
        ClearChildren();

        RectTransform root = GetComponent<RectTransform>();
        root.anchorMin = Vector2.zero;
        root.anchorMax = Vector2.one;
        root.offsetMin = Vector2.zero;
        root.offsetMax = Vector2.zero;

        CreatePanel("Background", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, backgroundColor);

        if (titleSprite != null)
        {
            Image title = CreateImage("TitleBitmap", titleSprite, new Vector2(0.5f, 0.5f), new Vector2(520f, 360f));
            title.preserveAspect = true;
            titleRect = title.GetComponent<RectTransform>();
            titleRect.anchoredPosition = new Vector2(0f, 420f);
        }

        menuOptions = new GameObject("MenuOptions", typeof(RectTransform));
        menuOptions.transform.SetParent(transform, false);

        RectTransform optionsRect = menuOptions.GetComponent<RectTransform>();
        optionsRect.anchorMin = Vector2.zero;
        optionsRect.anchorMax = Vector2.one;
        optionsRect.offsetMin = Vector2.zero;
        optionsRect.offsetMax = Vector2.zero;
        menuOptions.SetActive(false);

        startPromptGroup = CreateBitmapButton("StartButton", "1P START", new Vector2(0.5f, 0.25f), LoadFirstScene);

        if (titleRect != null)
        {
            StartCoroutine(ShowMenuRoutine());
        }
        else
        {
            menuOptions.SetActive(true);
            StartCoroutine(BlinkStartPrompt());
        }
    }

    private void ClearChildren()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }

    private Image CreatePanel(
        string objectName,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 offsetMin,
        Vector2 offsetMax,
        Color color)
    {
        GameObject panelObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panelObject.transform.SetParent(transform, false);

        RectTransform rect = panelObject.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;

        Image image = panelObject.GetComponent<Image>();
        image.color = color;

        return image;
    }

    private Image CreateImage(string objectName, Sprite sprite, Vector2 anchor, Vector2 size)
    {
        GameObject imageObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        imageObject.transform.SetParent(transform, false);

        RectTransform rect = imageObject.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = size;

        Image image = imageObject.GetComponent<Image>();
        image.sprite = sprite;
        image.raycastTarget = false;

        return image;
    }

    private CanvasGroup CreateBitmapButton(
        string objectName,
        string label,
        Vector2 anchor,
        UnityEngine.Events.UnityAction action)
    {
        GameObject buttonObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(CanvasGroup));
        buttonObject.transform.SetParent(menuOptions.transform, false);

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(300f, 52f);

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0f);

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(action);

        CreateBitmapText(label, buttonObject.transform, new Vector2(0.5f, 0.5f), 4.5f);

        return buttonObject.GetComponent<CanvasGroup>();
    }

    private void OnStartInput(InputAction.CallbackContext context)
    {
        LoadFirstScene();
    }

    private void LoadFirstScene()
    {
        if (isLoadingScene) return;

        isLoadingScene = true;
        GameSession.Ensure().ResetRun(3);
        SceneManager.LoadScene(firstSceneName);
    }

    private void BuildFontMap()
    {
        fontMap.Clear();
        fontMap['1'] = digit1Sprite;
        fontMap['A'] = letterASprite;
        fontMap['G'] = letterGSprite;
        fontMap['I'] = letterISprite;
        fontMap['L'] = letterLSprite;
        fontMap['P'] = letterPSprite;
        fontMap['R'] = letterRSprite;
        fontMap['S'] = letterSSprite;
        fontMap['T'] = letterTSprite;
    }

    private void CreateBitmapText(string text, Transform parent, Vector2 anchor, float scale)
    {
        GameObject container = new GameObject("BitmapText", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        container.transform.SetParent(parent, false);

        RectTransform rect = container.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(360f, 56f);

        HorizontalLayoutGroup layout = container.GetComponent<HorizontalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        layout.childScaleWidth = false;
        layout.childScaleHeight = false;
        layout.spacing = 4f;

        foreach (char character in text)
        {
            if (character == ' ')
            {
                CreateSpacer(container.transform, 20f);
                continue;
            }

            if (!fontMap.TryGetValue(character, out Sprite sprite) || sprite == null)
            {
                continue;
            }

            GameObject letterObject = new GameObject(character.ToString(), typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            letterObject.transform.SetParent(container.transform, false);

            Image image = letterObject.GetComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;
            image.raycastTarget = false;

            Outline outline = letterObject.AddComponent<Outline>();
            outline.effectColor = startGlowColor;
            outline.effectDistance = new Vector2(2f, -2f);
            outline.useGraphicAlpha = true;

            Shadow softGlow = letterObject.AddComponent<Shadow>();
            softGlow.effectColor = new Color(startGlowColor.r, startGlowColor.g, startGlowColor.b, 0.45f);
            softGlow.effectDistance = new Vector2(3.5f, -3.5f);
            softGlow.useGraphicAlpha = true;

            RectTransform letterRect = letterObject.GetComponent<RectTransform>();
            Vector2 letterSize = sprite.rect.size * scale;
            letterRect.sizeDelta = letterSize;

            LayoutElement layoutElement = letterObject.AddComponent<LayoutElement>();
            layoutElement.preferredWidth = letterSize.x;
            layoutElement.preferredHeight = letterSize.y;
        }
    }

    private void CreateSpacer(Transform parent, float width)
    {
        GameObject spacer = new GameObject("Space", typeof(RectTransform), typeof(LayoutElement));
        spacer.transform.SetParent(parent, false);

        LayoutElement layout = spacer.GetComponent<LayoutElement>();
        layout.preferredWidth = width;
        layout.preferredHeight = 1f;
    }

    private IEnumerator ShowMenuRoutine()
    {
        float timer = 0f;
        float duration = 1.35f;
        Vector2 startPosition = new Vector2(0f, 420f);
        Vector2 endPosition = new Vector2(0f, 74f);

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / duration);
            float eased = 1f - Mathf.Pow(1f - t, 3f);
            titleRect.anchoredPosition = Vector2.Lerp(startPosition, endPosition, eased);
            yield return null;
        }

        titleRect.anchoredPosition = endPosition;
        yield return new WaitForSeconds(0.25f);
        menuOptions.SetActive(true);
        StartCoroutine(BlinkStartPrompt());
    }

    private IEnumerator BlinkStartPrompt()
    {
        if (startPromptGroup == null)
        {
            yield break;
        }

        while (true)
        {
            float alpha = Mathf.Lerp(0.35f, 1f, (Mathf.Sin(Time.time * 6f) + 1f) * 0.5f);
            startPromptGroup.alpha = alpha;
            yield return null;
        }
    }
}
