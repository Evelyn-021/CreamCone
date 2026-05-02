using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameWinUI : MonoBehaviour
{
    [Header("Escena")]
    public string mainMenuSceneName = "MainMenu";

    [Header("Letras bitmap")]
    public Sprite letterASprite;
    public Sprite letterBSprite;
    public Sprite letterCSprite;
    public Sprite letterDSprite;
    public Sprite letterESprite;
    public Sprite letterFSprite;
    public Sprite letterGSprite;
    public Sprite letterHSprite;
    public Sprite letterISprite;
    public Sprite letterKSprite;
    public Sprite letterLSprite;
    public Sprite letterMSprite;
    public Sprite letterNSprite;
    public Sprite letterOSprite;
    public Sprite letterPSprite;
    public Sprite letterRSprite;
    public Sprite letterSSprite;
    public Sprite letterTSprite;
    public Sprite letterUSprite;
    public Sprite letterVSprite;
    public Sprite letterWSprite;
    public Sprite letterYSprite;

    [Header("Numeros bitmap")]
    public Sprite[] numberSprites;

    [Header("Colores")]
    public Color backgroundColor = Color.black;
    public Color messageColor = new Color(1f, 0.92f, 0.05f, 1f);
    public Color creditColor = new Color(1f, 0.05f, 0.02f, 1f);
    public float dropDuration = 10f;
    public float dropStartOffsetY = 520f;

    private readonly Dictionary<char, Sprite> fontMap = new Dictionary<char, Sprite>();
    private RectTransform messageRoot;
    private Controles controls;
    private bool loadingMenu;

    private void Start()
    {
        Build();
    }

    private void Update()
    {
        if (loadingMenu) return;

        if ((Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame) ||
            WasAnyGamepadButtonPressed())
        {
            LoadMainMenu();
        }
    }

    private void OnEnable()
    {
        if (controls == null)
        {
            controls = new Controles();
        }

        controls.Enable();
        controls.Player.Jump.performed += OnReturnInput;
        controls.Player.Attack.performed += OnReturnInput;
    }

    private void OnDisable()
    {
        if (controls == null) return;

        controls.Player.Jump.performed -= OnReturnInput;
        controls.Player.Attack.performed -= OnReturnInput;
        controls.Disable();
    }

    private void Build()
    {
        BuildFontMap();
        ClearChildren();

        RectTransform root = GetComponent<RectTransform>();
        root.anchorMin = Vector2.zero;
        root.anchorMax = Vector2.one;
        root.offsetMin = Vector2.zero;
        root.offsetMax = Vector2.zero;

        CreatePanel("Background", backgroundColor);
        CreateBitmapText("SCORE", transform, new Vector2(0f, 1f), new Vector2(28f, -28f), 2.4f, 220f, messageColor, TextAnchor.MiddleLeft);
        CreateBitmapText(GameSession.CurrentScore.ToString().PadLeft(6, '0'), transform, new Vector2(0f, 1f), new Vector2(28f, -58f), 2.4f, 220f, messageColor, TextAnchor.MiddleLeft);

        messageRoot = CreateMessageRoot();

        CreateBitmapText("CONGRATULATIONS", messageRoot, new Vector2(0.5f, 0.72f), 3.2f, 600f, messageColor);
        CreateBitmapText("YOU WON", messageRoot, new Vector2(0.5f, 0.61f), 3.2f, 360f, messageColor);
        CreateBitmapText("THANK YOU", messageRoot, new Vector2(0.5f, 0.5f), 2.95f, 380f, messageColor);
        CreateBitmapText("FOR PLAYING", messageRoot, new Vector2(0.5f, 0.41f), 2.95f, 440f, messageColor);

        CreateBitmapText("GAME INSPIRED BY", messageRoot, new Vector2(0.5f, 0.25f), 2.1f, 520f, creditColor);
        CreateBitmapText("BUBBLE BOBBLE TAITO", messageRoot, new Vector2(0.5f, 0.18f), 2.1f, 580f, creditColor);
        CreateBitmapText("MADE BY EVELYN ALONSO", messageRoot, new Vector2(0.5f, 0.11f), 2.1f, 640f, creditColor);

        StartCoroutine(DropMessageRoutine());
    }

    private void BuildFontMap()
    {
        fontMap.Clear();
        fontMap['A'] = letterASprite;
        fontMap['B'] = letterBSprite;
        fontMap['C'] = letterCSprite;
        fontMap['D'] = letterDSprite;
        fontMap['E'] = letterESprite;
        fontMap['F'] = letterFSprite;
        fontMap['G'] = letterGSprite;
        fontMap['H'] = letterHSprite;
        fontMap['I'] = letterISprite;
        fontMap['K'] = letterKSprite;
        fontMap['L'] = letterLSprite;
        fontMap['M'] = letterMSprite;
        fontMap['N'] = letterNSprite;
        fontMap['O'] = letterOSprite;
        fontMap['P'] = letterPSprite;
        fontMap['R'] = letterRSprite;
        fontMap['S'] = letterSSprite;
        fontMap['T'] = letterTSprite;
        fontMap['U'] = letterUSprite;
        fontMap['V'] = letterVSprite;
        fontMap['W'] = letterWSprite;
        fontMap['Y'] = letterYSprite;
    }

    private void ClearChildren()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }

    private void CreatePanel(string objectName, Color color)
    {
        GameObject panelObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panelObject.transform.SetParent(transform, false);

        RectTransform rect = panelObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = panelObject.GetComponent<Image>();
        image.color = color;
    }

    private RectTransform CreateMessageRoot()
    {
        GameObject rootObject = new GameObject("WinMessageRoot", typeof(RectTransform));
        rootObject.transform.SetParent(transform, false);

        RectTransform rect = rootObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.anchoredPosition = Vector2.up * dropStartOffsetY;

        return rect;
    }

    private IEnumerator DropMessageRoutine()
    {
        if (messageRoot == null) yield break;

        Vector2 start = Vector2.up * dropStartOffsetY;
        Vector2 end = Vector2.zero;
        float timer = 0f;

        while (timer < dropDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / dropDuration);
            float eased = 1f - Mathf.Pow(1f - t, 3f);
            messageRoot.anchoredPosition = Vector2.Lerp(start, end, eased);
            yield return null;
        }

        messageRoot.anchoredPosition = end;
    }

    private void CreateBitmapText(string text, Transform parent, Vector2 anchor, float scale, float width, Color color)
    {
        CreateBitmapText(text, parent, anchor, Vector2.zero, scale, width, color, TextAnchor.MiddleCenter);
    }

    private void CreateBitmapText(
        string text,
        Transform parent,
        Vector2 anchor,
        Vector2 anchoredPosition,
        float scale,
        float width,
        Color color,
        TextAnchor alignment)
    {
        GameObject container = new GameObject("BitmapText", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        container.transform.SetParent(parent, false);

        RectTransform rect = container.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(width, 56f);
        rect.pivot = GetPivotForAlignment(alignment);

        HorizontalLayoutGroup layout = container.GetComponent<HorizontalLayoutGroup>();
        layout.childAlignment = alignment;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        layout.childScaleWidth = false;
        layout.childScaleHeight = false;
        layout.spacing = 2f;

        foreach (char character in text)
        {
            if (character == ' ')
            {
                CreateSpacer(container.transform, 14f);
                continue;
            }

            Sprite sprite = GetSprite(character);
            if (sprite == null)
            {
                continue;
            }

            GameObject letterObject = new GameObject(character.ToString(), typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            letterObject.transform.SetParent(container.transform, false);

            Image image = letterObject.GetComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.preserveAspect = true;
            image.raycastTarget = false;

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

    private Vector2 GetPivotForAlignment(TextAnchor alignment)
    {
        if (alignment == TextAnchor.MiddleLeft)
        {
            return new Vector2(0f, 0.5f);
        }

        if (alignment == TextAnchor.MiddleRight)
        {
            return new Vector2(1f, 0.5f);
        }

        return new Vector2(0.5f, 0.5f);
    }

    private Sprite GetSprite(char character)
    {
        if (char.IsDigit(character))
        {
            int index = character - '0';
            if (numberSprites != null && index >= 0 && index < numberSprites.Length)
            {
                return numberSprites[index];
            }
        }

        fontMap.TryGetValue(character, out Sprite sprite);
        return sprite;
    }

    private void OnReturnInput(InputAction.CallbackContext context)
    {
        LoadMainMenu();
    }

    private bool WasAnyGamepadButtonPressed()
    {
        if (Gamepad.current == null) return false;

        foreach (InputControl control in Gamepad.current.allControls)
        {
            if (control is ButtonControl button && button.wasPressedThisFrame)
            {
                return true;
            }
        }

        return false;
    }

    private void LoadMainMenu()
    {
        if (loadingMenu) return;

        loadingMenu = true;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
