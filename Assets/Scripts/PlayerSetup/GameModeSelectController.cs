using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace BomBomLemon.PlayerSetup
{
    public class GameModeSelectController : MonoBehaviour
    {
        [SerializeField] Button          localPlayButton;
        [SerializeField] Button          createRoomButton;
        [SerializeField] Button          joinRoomButton;
        [SerializeField] Button          backButton;
        [SerializeField] TextMeshProUGUI headerLabel;
        [SerializeField] TextMeshProUGUI localPlayLabel;
        [SerializeField] TextMeshProUGUI createRoomLabel;
        [SerializeField] TextMeshProUGUI joinRoomLabel;
        [SerializeField] TextMeshProUGUI orLabel;
        [SerializeField] TextMeshProUGUI guideLabel;
        [SerializeField] TextMeshProUGUI backLabel;
        [SerializeField] CanvasGroup     panelGroup;

        [SerializeField] string localSceneName  = "Game";
        [SerializeField] string titleSceneName  = "Title";

        void Start()
        {
            localPlayButton?.onClick.AddListener(OnLocalPlay);
            createRoomButton?.onClick.AddListener(OnCreateRoom);
            joinRoomButton?.onClick.AddListener(OnJoinRoom);
            backButton?.onClick.AddListener(OnBack);
            LanguageSettings.OnLanguageChanged += ApplyLanguage;
            ApplyLanguage();
            if (panelGroup != null)
                StartCoroutine(FadeIn());
        }

        void OnDestroy() => LanguageSettings.OnLanguageChanged -= ApplyLanguage;

        void ApplyLanguage()
        {
            bool en = LanguageSettings.IsEnglish;
            if (headerLabel)      headerLabel.text      = en ? "How to Play?"                  : "どうやって遊ぶ？";
            if (localPlayLabel)   localPlayLabel.text   = en ? "Play on this device"           : "このスマホで遊ぶ";
            if (createRoomLabel)  createRoomLabel.text  = en ? "Create Room"                   : "部屋を立てる";
            if (joinRoomLabel)    joinRoomLabel.text    = en ? "Join Room"                     : "部屋に入る";
            if (orLabel)          orLabel.text          = en ? "— or —"                        : "― または ―";
            if (guideLabel)       guideLabel.text       = en
                ? "For multi-device play, all devices need BomBom Lemon installed."
                : "部屋に集まって遊ぶ場合は\nそれぞれのスマホにボムボムレモンが\nインストールされている必要があります";
            if (backLabel)        backLabel.text        = en ? "← Back"                        : "← 戻る";
        }

        void OnLocalPlay()  => SceneManager.LoadScene(localSceneName);
        void OnBack()       => SceneManager.LoadScene(titleSceneName);

        void OnCreateRoom()
        {
            Debug.Log("[GameModeSelectController] Create Room (not implemented yet)");
        }

        void OnJoinRoom()
        {
            Debug.Log("[GameModeSelectController] Join Room (not implemented yet)");
        }

        IEnumerator FadeIn()
        {
            panelGroup.alpha = 0f;
            float dur = 0.30f, t = 0f;
            while (t < dur)
            {
                t += Time.deltaTime;
                panelGroup.alpha = Mathf.SmoothStep(0f, 1f, t / dur);
                yield return null;
            }
            panelGroup.alpha = 1f;
        }
    }
}
