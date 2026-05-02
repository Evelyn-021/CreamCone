using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [Header("Escena")]
    public string mainMenuSceneName = "MainMenu";

    [Header("Letras bitmap")]
    public Sprite letterASprite;
    public Sprite letterBSprite;
    public Sprite letterCSprite;
    public Sprite letterESprite;
    public Sprite letterGSprite;
    public Sprite letterMSprite;
    public Sprite letterOSprite;
    public Sprite letterPSprite;
    public Sprite letterRSprite;
    public Sprite letterSSprite;
    public Sprite letterTSprite;
    public Sprite letterVSprite;
    public Sprite letterHSprite;
    public Sprite letterUSprite;

    [Header("Numeros bitmap")]
    public Sprite[] numberSprites;

    [Header("Color")]
    public Color backgroundColor = Color.black;

    private readonly Dictionary<char, Sprite> fontMap = new Dictionary<char, Sprite>();
    private Controles controls;
    private bool loadingMenu;

    private void Start()
    {
        Build();
    }

    private void Update()
    {
        if (loadingMenu) return;

        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
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
        controls.Player.Jump.performed += OnStartInput;
        controls.Player.Attack.performed += OnStartInput;
    }

    private void OnDisable()
    {
        if (controls == null) return;

        controls.Player.Jump.performed -= OnStartInput;
        controls.Player.Attack.performed -= OnStartInput;
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
        CreateBitmapText("GAME OVER", transform, new Vector2(0.5f, 0.62f), 4.5f, 480f);
        CreateBitmapText("SCORE", transform, new Vector2(0.5f, 0.48f), 3.8f, 320f);
        CreateBitmapText(GameSession.CurrentScore.ToString().PadLeft(6, '0'), transform, new Vector2(0.5f, 0.4f), 3.8f, 320f);
        CreateBitmapText("PUSH START", transform, new Vector2(0.5f, 0.25f), 3.8f, 520f);
    }

    private void BuildFontMap()
    {
        fontMap.Clear();
        fontMap['A'] = letterASprite;
        fontMap['B'] = letterBSprite;
        fontMap['C'] = letterCSprite;
        fontMap['E'] = letterESprite;
        fontMap['G'] = letterGSprite;
        fontMap['H'] = letterHSprite;
        fontMap['M'] = letterMSprite;
        fontMap['O'] = letterOSprite;
        fontMap['P'] = letterPSprite;
        fontMap['R'] = letterRSprite;
        fontMap['S'] = letterSSprite;
        fontMap['T'] = letterTSprite;
        fontMap['U'] = letterUSprite;
        fontMap['V'] = letterVSprite;
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

    private void CreateBitmapText(string text, Transform parent, Vector2 anchor, float scale, float width)
    {
        GameObject container = new GameObject("BitmapText", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        container.transform.SetParent(parent, false);

        RectTransform rect = container.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(width, 56f);

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
                CreateSpacer(container.transform, 22f);
                continue;
            }

            Sprite sprite = GetSprite(character);
            if (sprite == null) continue;

            GameObject letterObject = new GameObject(character.ToString(), typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            letterObject.transform.SetParent(container.transform, false);

            Image image = letterObject.GetComponent<Image>();
            image.sprite = sprite;
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

    private void CreateSpacer(Transform parent, float width)
    {
        GameObject spacer = new GameObject("Space", typeof(RectTransform), typeof(LayoutElement));
        spacer.transform.SetParent(parent, false);

        LayoutElement layout = spacer.GetComponent<LayoutElement>();
        layout.preferredWidth = width;
        layout.preferredHeight = 1f;
    }

    private void OnStartInput(InputAction.CallbackContext context)
    {
        LoadMainMenu();
    }

    private void LoadMainMenu()
    {
        if (loadingMenu) return;

        loadingMenu = true;
        StartCoroutine(LoadMainMenuNextFrame());
    }

    private IEnumerator LoadMainMenuNextFrame()
    {
        yield return null;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
