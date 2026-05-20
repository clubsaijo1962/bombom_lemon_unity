using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using BomBomLemon.PlayerSetup;
using BomBomLemon.Game.Topics;

namespace BomBomLemon.Game
{
    public class GameTopicController : MonoBehaviour
    {
        [Header("お題")]
        [SerializeField] TextMeshProUGUI topicLabel;
        [SerializeField] TextMeshProUGUI topicLowLabel;
        [SerializeField] TextMeshProUGUI topicHighLabel;

        [Header("回答プレイヤー")]
        [SerializeField] TextMeshProUGUI guideNameLabel;
        [SerializeField] TextMeshProUGUI answerNameLabel;

        [Header("HUD")]
        [SerializeField] TextMeshProUGUI lifeCountLabel;
        [SerializeField] TextMeshProUGUI helpCardCountLabel;

        [Header("ボタン")]
        [SerializeField] Button confirmButton;
        [SerializeField] Button topicChangeButton;
        [SerializeField] Button homeButton;

        [Header("フェード")]
        [SerializeField] CanvasGroup screenFade;
        [SerializeField] CanvasGroup panelGroup;

        readonly HashSet<int> _usedIndices = new();

        void Start()
        {
            if (screenFade) { screenFade.alpha = 1f; screenFade.blocksRaycasts = true; }
            if (panelGroup) panelGroup.alpha = 0f;

            ApplyHUD();
            ApplyTopic();
            ApplyPlayers();

            confirmButton?.onClick.AddListener(OnConfirm);
            topicChangeButton?.onClick.AddListener(OnTopicChange);
            homeButton?.onClick.AddListener(OnHome);

            StartCoroutine(FadeOverlayOut());
            StartCoroutine(FadeContentIn());
        }

        void ApplyHUD()
        {
            if (lifeCountLabel)
            {
                lifeCountLabel.text = $"×{SinglePlayConfig.LifeCount}";
                lifeCountLabel.enableWordWrapping = false;
                lifeCountLabel.enableAutoSizing = true;
                lifeCountLabel.fontSizeMin = 24f;
                lifeCountLabel.fontSizeMax = 42f;
            }
            if (helpCardCountLabel)
            {
                helpCardCountLabel.text = $"×{SinglePlayConfig.HelpCardCount}";
                helpCardCountLabel.enableWordWrapping = false;
            }
        }

        void ApplyTopic() => DisplayTopic(PickNextTopic());

        void OnTopicChange() => DisplayTopic(PickNextTopic());

        Topic PickNextTopic()
        {
            var db = TopicRuntimeDatabase.Instance;
            if (db == null || db.Topics.Count == 0) return FallbackTopic();

            var unused = new List<int>();
            for (int i = 0; i < db.Topics.Count; i++)
                if (!_usedIndices.Contains(i)) unused.Add(i);

            if (unused.Count == 0)
            {
                _usedIndices.Clear();
                for (int i = 0; i < db.Topics.Count; i++) unused.Add(i);
            }

            int idx = unused[Random.Range(0, unused.Count)];
            _usedIndices.Add(idx);
            return db.Topics[idx];
        }

        void DisplayTopic(Topic topic)
        {
            bool en = LanguageSettings.IsEnglish;
            if (topicLabel)
                topicLabel.text = en ? topic.TextEN : topic.Text;
            if (topicLowLabel)
                topicLowLabel.text = en ? $"1 = {topic.LowLabelEN}" : $"1 = {topic.LowLabel}";
            if (topicHighLabel)
                topicHighLabel.text = en ? $"99 = {topic.HighLabelEN}" : $"99 = {topic.HighLabel}";
        }

        void ApplyPlayers()
        {
            int count = SinglePlayConfig.PlayerCount;
            string[] names = SinglePlayConfig.PlayerNames;

            int guideIdx  = Random.Range(0, count);
            int answerIdx = count > 1
                ? (guideIdx + 1 + Random.Range(0, count - 1)) % count
                : guideIdx;

            if (guideNameLabel)
                guideNameLabel.text = names != null && guideIdx < names.Length
                    ? names[guideIdx] : $"プレイヤー{guideIdx + 1}";
            if (answerNameLabel)
                answerNameLabel.text = names != null && answerIdx < names.Length
                    ? names[answerIdx] : $"プレイヤー{answerIdx + 1}";
        }

        void OnConfirm() => Debug.Log("[GameTopicController] 数字確認ボタン押下");

        void OnHome() => StartCoroutine(LoadWithFade("SingleSettings"));

        IEnumerator LoadWithFade(string sceneName)
        {
            if (screenFade != null)
            {
                screenFade.blocksRaycasts = true;
                float dur = 0.28f, t = 0f;
                while (t < dur) { t += Time.deltaTime; screenFade.alpha = Mathf.SmoothStep(0f, 1f, t / dur); yield return null; }
                screenFade.alpha = 1f;
            }
            SceneManager.LoadScene(sceneName);
        }

        static Topic FallbackTopic() => new(
            "誕生日にもらって嬉しいもの",
            "全く嬉しくない", "最高に嬉しい",
            "動物の死骸", "家と高級車のカギ一式",
            "Birthday gifts ranked by happiness",
            "Horrifying", "Life-changingly amazing",
            "The carcass of an animal", "Keys to a house and a luxury car");

        IEnumerator FadeOverlayOut()
        {
            if (screenFade == null) yield break;
            float dur = 0.35f, t = 0f;
            while (t < dur) { t += Time.deltaTime; screenFade.alpha = Mathf.SmoothStep(1f, 0f, t / dur); yield return null; }
            screenFade.alpha = 0f;
            screenFade.blocksRaycasts = false;
        }

        IEnumerator FadeContentIn()
        {
            if (!panelGroup) yield break;
            float dur = 0.30f, t = 0f;
            while (t < dur) { t += Time.deltaTime; panelGroup.alpha = Mathf.SmoothStep(0f, 1f, t / dur); yield return null; }
            panelGroup.alpha = 1f;
        }
    }
}
