using UnityEngine;
using UnityEngine.SceneManagement;

namespace RantsMailDelivery
{
    public class GameDirector : MonoBehaviour
    {
        public static GameDirector Instance { get; private set; }
        public RunnerPlayer player;
        public RunnerCamera cameraController;
        public int requiredWhitePart1 = 6;
        public int requiredWhitePart2 = 7;
        public int requiredWhitePart3 = 8;

        private enum GameState { Menu, Selection, Intro, Playing, Paused, GameOver, PartClear, Finished, Options, Shop, Credits }
        private GameState state = GameState.Menu;
        private int currentPart;
        private int white;
        private int bankBlue;
        private int unlockedPart;
        private bool blueHatOwned;
        private bool blueHatEquipped;
        private bool muted;
        private float darkOverlay = .02f;
        private float recoveringUntil;
        private float invulnerableUntil;
        private float checkpointX;
        private int checkpointWhite;
        private bool checkpointActivated;
        private float messageUntil;
        private string toast = "";

        private GUIStyle titleStyle;
        private GUIStyle subTitleStyle;
        private GUIStyle labelStyle;
        private GUIStyle smallStyle;
        private GUIStyle buttonStyle;
        private GUIStyle panelStyle;
        private Texture2D panelTex;
        private Texture2D darkTex;

        public bool IsPlaying => state == GameState.Playing;
        public int CurrentPart => currentPart;
        public bool IsTired => IsPlaying && Time.time < recoveringUntil;
        public bool CanGlide => currentPart >= 1;
        public bool BlueHatEquipped => blueHatEquipped;
        private int RequiredWhite => currentPart == 0 ? requiredWhitePart1 : currentPart == 1 ? requiredWhitePart2 : requiredWhitePart3;
        private float PartStart => currentPart == 0 ? 2f : currentPart == 1 ? 70.5f : 144.5f;
        private float PartEnd => currentPart == 0 ? 65f : currentPart == 1 ? 139f : 212f;
        private string PartName => currentPart == 0 ? "Parte 1 • Vizinhança" : currentPart == 1 ? "Parte 2 • Língua da Tera" : "Parte 3 • A Grande Floresta";

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            Time.timeScale = 1f;
            bankBlue = PlayerPrefs.GetInt("Rant_BlueBank", 0);
            unlockedPart = Mathf.Clamp(PlayerPrefs.GetInt("Rant_UnlockedPart", 0), 0, 2);
            blueHatOwned = PlayerPrefs.GetInt("Rant_BlueHatOwned", 0) > 0;
            blueHatEquipped = PlayerPrefs.GetInt("Rant_BlueHatEquipped", 0) > 0 && blueHatOwned;
        }

        private void Start()
        {
            if (player != null) player.ResetTo(PartStart);
            if (cameraController != null) cameraController.Snap();
        }

        private void Update()
        {
#if UNITY_EDITOR
            // Atalhos de avaliação do Level Design: F1/F2/F3 iniciam as 3 partes
            // sem mudar o desbloqueio do jogador. Não existem no build final.
            if (Input.GetKeyDown(KeyCode.F1)) BeginPart(0);
            if (Input.GetKeyDown(KeyCode.F2)) BeginPart(1);
            if (Input.GetKeyDown(KeyCode.F3)) BeginPart(2);
#endif
            if (state == GameState.Playing)
            {
                if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P)) Pause();
                if (player == null) return;
                float x = player.transform.position.x;
                if (!checkpointActivated && x >= CheckpointLocation())
                {
                    checkpointActivated = true;
                    checkpointX = CheckpointLocation();
                    checkpointWhite = white;
                    Notify("Checkpoint salvo!");
                }
                if (player.transform.position.y < -7.2f) GameOver("Rant caiu no abismo!");
                if (x >= PartEnd) FinishPart();
            }
            else if (state == GameState.Paused && (Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Escape))) Resume();
        }

        private float CheckpointLocation() => currentPart == 0 ? 23f : currentPart == 1 ? 90f : 169f;
        private void Notify(string text) { toast = text; messageUntil = Time.unscaledTime + 2.6f; }

        public void AddCrystal(bool blue)
        {
            if (!IsPlaying) return;
            if (blue)
            {
                bankBlue++;
                PlayerPrefs.SetInt("Rant_BlueBank", bankBlue);
                PlayerPrefs.Save();
                Notify("Cristal azul! Pode comprar roupas na loja.");
            }
            else white++;
        }

        public void OnHazardHit(bool fatal)
        {
            if (!IsPlaying || Time.time < invulnerableUntil) return;
            if (fatal || IsTired)
            {
                GameOver(fatal ? "Obstáculo intransponível!" : "Rant levou dois danos seguidos!");
                return;
            }
            recoveringUntil = Time.time + 2.5f;
            invulnerableUntil = Time.time + .4f;
            Notify("Rant ficou cansado! Evite outro impacto!");
        }

        private void Pause() { if (state == GameState.Playing) { state = GameState.Paused; Time.timeScale = 0f; } }
        private void Resume() { if (state == GameState.Paused) { state = GameState.Playing; Time.timeScale = 1f; } }

        private void BeginPart(int idx)
        {
            Time.timeScale = 1f;
            currentPart = Mathf.Clamp(idx, 0, 2);
            white = 0;
            checkpointX = PartStart;
            checkpointWhite = 0;
            checkpointActivated = false;
            recoveringUntil = -1f;
            invulnerableUntil = -1f;
            ResetWorldFrom(PartStart);
            if (player != null) player.ResetTo(PartStart);
            if (cameraController != null) cameraController.Snap();
            state = GameState.Playing;
            Notify(currentPart == 0 ? "Espaço / clique: pular • S: rolar" : "Segure espaço / clique para PLANAR!");
        }

        private void ResetWorldFrom(float atX)
        {
            foreach (var item in FindObjectsByType<PickupItem>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                item.RestoreAfterCheckpoint(atX);
            foreach (var hazard in FindObjectsByType<HazardObject>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                hazard.RestoreAfterCheckpoint(atX);
        }

        private void GameOver(string reason)
        {
            state = GameState.GameOver;
            Time.timeScale = 0f;
            toast = reason;
        }

        private void RestartCheckpoint()
        {
            Time.timeScale = 1f;
            white = checkpointWhite;
            recoveringUntil = -1f;
            invulnerableUntil = -1f;
            ResetWorldFrom(checkpointX);
            player.ResetTo(checkpointX);
            if (cameraController != null) cameraController.Snap();
            state = GameState.Playing;
            Notify("Retomando do último checkpoint!");
        }

        private void FinishPart()
        {
            Time.timeScale = 0f;
            if (white < RequiredWhite)
            {
                state = GameState.GameOver;
                toast = "Cristais brancos insuficientes para avançar!";
                return;
            }
            if (currentPart == 2)
            {
                state = GameState.Finished;
            }
            else
            {
                unlockedPart = Mathf.Max(unlockedPart, currentPart + 1);
                PlayerPrefs.SetInt("Rant_UnlockedPart", unlockedPart);
                PlayerPrefs.Save();
                state = GameState.PartClear;
            }
        }

        private void OnGUI()
        {
            float xscale = Screen.width / 1600f;
            float yscale = Screen.height / 900f;
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(xscale, yscale, 1f));
            InitStyles();
            if (darkOverlay > 0f)
            {
                Color old = GUI.color;
                GUI.color = new Color(1f, 1f, 1f, darkOverlay);
                GUI.DrawTexture(new Rect(0, 0, 1600, 900), darkTex);
                GUI.color = old;
            }

            if (state == GameState.Playing || state == GameState.Paused)
            {
                GUI.Box(new Rect(25, 18, 570, 98), GUIContent.none, panelStyle);
                GUI.Label(new Rect(48, 28, 530, 43), PartName, subTitleStyle);
                GUI.Label(new Rect(48, 71, 530, 30), "Açúcar branco: " + white + " / " + RequiredWhite + "     Azul: " + bankBlue, labelStyle);
                GUI.Box(new Rect(1170, 18, 400, 84), GUIContent.none, panelStyle);
                GUI.Label(new Rect(1192, 31, 350, 25), CanGlide ? "PULAR • PLANAR • ROLAR" : "PULAR • ROLAR", smallStyle);
                GUI.Label(new Rect(1192, 63, 350, 25), IsTired ? "CANSADO — PERIGO!" : "P / ESC: pausa", smallStyle);
                if (state == GameState.Playing && Time.unscaledTime < messageUntil)
                    GUI.Label(new Rect(375, 745, 850, 45), toast, subTitleStyle);
            }

            if (state == GameState.Menu)
            {
                Panel(415, 92, 770, 682);
                GUI.Label(new Rect(470, 132, 670, 90), "RANT'S MAIL DELIVERY", titleStyle);
                GUI.Label(new Rect(485, 240, 645, 58), "O carteiro mais atrasado da floresta!", subTitleStyle);
                if (Button(580, 345, 440, 66, "JOGAR")) state = GameState.Selection;
                if (Button(580, 422, 440, 66, "OPÇÕES")) state = GameState.Options;
                if (Button(580, 499, 440, 66, "LOJA DE ROUPAS")) state = GameState.Shop;
                if (Button(580, 576, 440, 66, "CRÉDITOS")) state = GameState.Credits;
                GUI.Label(new Rect(580, 690, 500, 45), "Protótipo Unity 2D • GDD 1.2.0", smallStyle);
            }
            else if (state == GameState.Selection)
            {
                Panel(395, 125, 810, 650);
                GUI.Label(new Rect(465, 151, 680, 70), "SELECIONE A PARTE", titleStyle);
                for (int i = 0; i < 3; i++)
                {
                    string txt = "PARTE " + (i + 1) + (i <= unlockedPart ? " — DISPONÍVEL" : " — BLOQUEADA");
                    if (Button(515, 300 + 99 * i, 570, 78, txt) && i <= unlockedPart)
                    {
                        currentPart = i;
                        state = GameState.Intro;
                    }
                }
                if (Button(580, 651, 440, 62, "VOLTAR")) state = GameState.Menu;
            }
            else if (state == GameState.Intro)
            {
                Panel(296, 176, 1008, 550);
                GUI.Label(new Rect(390, 211, 830, 85), currentPart == 0 ? "RANT ACORDOU ATRASADO!" : "NOVA ENTREGA!", titleStyle);
                string text = currentPart == 0
                    ? "Rant põe seu chapéu de carteiro e corre para entregar as cartas.\nDesvie das formigas, pule obstáculos e recolha açúcar branco."
                    : currentPart == 1
                        ? "Uma língua perigosa surgiu pelo caminho...\nAgora Rant pode usar uma FOLHA para planar!"
                        : "Tera está logo atrás! Atravesse a floresta,\nrole por troncos e plane sobre os abismos.";
                GUI.Label(new Rect(400, 329, 800, 155), text, subTitleStyle);
                if (Button(540, 544, 520, 78, "COMEÇAR ENTREGA")) BeginPart(currentPart);
            }
            else if (state == GameState.GameOver)
            {
                Panel(387, 186, 825, 510);
                GUI.Label(new Rect(475, 220, 650, 80), "GAME OVER", titleStyle);
                GUI.Label(new Rect(478, 310, 640, 102), toast, subTitleStyle);
                GUI.Label(new Rect(478, 390, 640, 45), "Cristais brancos: " + white + " / " + RequiredWhite, labelStyle);
                if (Button(535, 477, 530, 72, "VOLTAR AO CHECKPOINT")) RestartCheckpoint();
                if (Button(535, 563, 530, 65, "MENU PRINCIPAL")) ReturnMenu();
            }
            else if (state == GameState.PartClear)
            {
                Panel(380, 165, 840, 560);
                GUI.Label(new Rect(466, 205, 675, 80), "ENTREGA CONCLUÍDA!", titleStyle);
                string text = currentPart == 0 ? "Rant chega ao correio... mas ainda precisa correr!" : "A floresta ficou mais perigosa. Tera se aproxima!";
                GUI.Label(new Rect(470, 320, 668, 135), text, subTitleStyle);
                if (Button(530, 484, 540, 70, "PRÓXIMA PARTE")) { currentPart++; state = GameState.Intro; Time.timeScale = 1f; }
                if (Button(530, 571, 540, 70, "MENU PRINCIPAL")) ReturnMenu();
            }
            else if (state == GameState.Finished)
            {
                Panel(338, 144, 920, 605);
                GUI.Label(new Rect(405, 187, 790, 84), "RANT'S MAIL DELIVERY", titleStyle);
                GUI.Label(new Rect(401, 314, 805, 156), "Rant tropeça num tronco... Tera se aproxima!\nMas ela só queria comer a FOLHA da encomenda.\nFim da demonstração!", subTitleStyle);
                if (Button(544, 560, 512, 76, "MENU PRINCIPAL")) ReturnMenu();
            }
            else if (state == GameState.Paused)
            {
                Panel(445, 190, 710, 525);
                GUI.Label(new Rect(545, 225, 535, 90), "PAUSADO", titleStyle);
                if (Button(545, 363, 512, 75, "CONTINUAR")) Resume();
                if (Button(545, 454, 512, 75, "REINICIAR CHECKPOINT")) RestartCheckpoint();
                if (Button(545, 547, 512, 75, "MENU")) ReturnMenu();
            }
            else if (state == GameState.Options)
            {
                Panel(430, 175, 740, 548);
                GUI.Label(new Rect(535, 212, 580, 80), "OPÇÕES", titleStyle);
                if (Button(535, 310, 530, 64, muted ? "SOM: DESLIGADO" : "SOM: LIGADO"))
                {
                    muted = !muted; AudioListener.volume = muted ? 0f : 1f;
                }
                if (Button(535, 393, 530, 64, "BRILHO: " + Mathf.RoundToInt((1f - darkOverlay) * 100f) + "%"))
                    darkOverlay = darkOverlay >= .32f ? .02f : darkOverlay + .1f;
                if (Button(535, 476, 530, 64, Screen.fullScreen ? "JANELA" : "TELA CHEIA"))
                    Screen.fullScreen = !Screen.fullScreen;
                if (Button(535, 576, 530, 64, "VOLTAR")) state = GameState.Menu;
            }
            else if (state == GameState.Shop)
            {
                Panel(422, 156, 756, 606);
                GUI.Label(new Rect(490, 194, 620, 75), "LOJA DE ROUPAS", titleStyle);
                GUI.Label(new Rect(519, 295, 590, 57), "Cristais azuis: " + bankBlue, subTitleStyle);
                string action = !blueHatOwned ? "COMPRAR CHAPÉU AZUL — 1 CRISTAL" : blueHatEquipped ? "CHAPÉU AZUL EQUIPADO" : "EQUIPAR CHAPÉU AZUL";
                if (Button(522, 387, 557, 82, action))
                {
                    if (!blueHatOwned && bankBlue > 0)
                    {
                        bankBlue--; blueHatOwned = true;
                        PlayerPrefs.SetInt("Rant_BlueBank", bankBlue);
                        PlayerPrefs.SetInt("Rant_BlueHatOwned", 1);
                    }
                    if (blueHatOwned)
                    {
                        blueHatEquipped = true;
                        PlayerPrefs.SetInt("Rant_BlueHatEquipped", 1);
                    }
                    PlayerPrefs.Save();
                }
                if (Button(522, 495, 557, 72, "USAR CHAPÉU ORIGINAL"))
                {
                    blueHatEquipped = false;
                    PlayerPrefs.SetInt("Rant_BlueHatEquipped", 0);
                }
                if (Button(522, 604, 557, 70, "VOLTAR")) state = GameState.Menu;
            }
            else if (state == GameState.Credits)
            {
                Panel(410, 183, 780, 545);
                GUI.Label(new Rect(505, 211, 600, 75), "CRÉDITOS", titleStyle);
                GUI.Label(new Rect(479, 317, 650, 189), "Conceito e GDD: Sarah D'Oliveira e Gabriel Ribeiro\nPersonagens: Rant, Tera e Bool\nProtótipo demonstrativo em Unity e C#", subTitleStyle);
                if (Button(538, 580, 524, 67, "VOLTAR")) state = GameState.Menu;
            }
        }

        private void ReturnMenu()
        {
            Time.timeScale = 1f;
            state = GameState.Menu;
            white = 0;
            recoveringUntil = -1f;
            if (player != null) player.ResetTo(2f);
            if (cameraController != null) cameraController.Snap();
        }

        private void InitStyles()
        {
            if (titleStyle != null) return;
            panelTex = MakeTexture(new Color(.035f, .11f, .14f, .93f));
            darkTex = MakeTexture(Color.black);
            titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 48, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, wordWrap = true };
            subTitleStyle = new GUIStyle(GUI.skin.label) { fontSize = 30, alignment = TextAnchor.MiddleCenter, wordWrap = true };
            labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 23, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft };
            smallStyle = new GUIStyle(GUI.skin.label) { fontSize = 22, alignment = TextAnchor.MiddleLeft };
            buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 24, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, wordWrap = true };
            panelStyle = new GUIStyle(GUI.skin.box);
            panelStyle.normal.background = panelTex;
            panelStyle.border = new RectOffset(5, 5, 5, 5);
            foreach (var style in new[] { titleStyle, subTitleStyle, labelStyle, smallStyle, buttonStyle })
                style.normal.textColor = Color.white;
        }
        private Texture2D MakeTexture(Color c)
        {
            Texture2D t = new Texture2D(2, 2);
            t.SetPixels(new[] { c, c, c, c }); t.Apply(); return t;
        }
        private void Panel(float x, float y, float w, float h) => GUI.Box(new Rect(x, y, w, h), GUIContent.none, panelStyle);
        private bool Button(float x, float y, float w, float h, string label) => GUI.Button(new Rect(x, y, w, h), label, buttonStyle);
        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            if (panelTex != null) Destroy(panelTex);
            if (darkTex != null) Destroy(darkTex);
        }
    }
}
