using System.Collections;
using UnityEngine;
using UnityEngine.UI;
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

        [Header("ガイド・回答プレイヤー")]
        [SerializeField] TextMeshProUGUI guideNameLabel;
        [SerializeField] TextMeshProUGUI answerNameLabel;

        [Header("HUD")]
        [SerializeField] TextMeshProUGUI lifeCountLabel;
        [SerializeField] TextMeshProUGUI helpCardCountLabel;

        [Header("ボタン")]
        [SerializeField] Button confirmButton;

        [Header("フェード")]
        [SerializeField] CanvasGroup screenFade;
        [SerializeField] CanvasGroup panelGroup;

        void Start()
        {
            if (screenFade) { screenFade.alpha = 1f; screenFade.blocksRaycasts = true; }
            if (panelGroup) panelGroup.alpha = 0f;

            ApplyHUD();
            ApplyTopic();
            ApplyPlayers();

            confirmButton?.onClick.AddListener(OnConfirm);

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

        void ApplyTopic()
        {
            Topic topic = null;
            var db = TopicRuntimeDatabase.Instance;
            if (db != null && db.Topics.Count > 0)
                topic = db.GetRandom();

            if (topic == null)
                topic = FallbackTopic();

            bool en = LanguageSettings.IsEnglish;
            if (topicLabel)
                topicLabel.text = en ? topic.TextEN : topic.Text;
            if (topicLowLabel)
                topicLowLabel.text = en
                    ? $"1 = {topic.LowLabelEN}"
                    : $"1 = {topic.LowLabel}";
            if (topicHighLabel)
                topicHighLabel.text = en
                    ? $"99 = {topic.HighLabelEN}"
                    : $"99 = {topic.HighLabel}";
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

        void OnConfirm()
        {
            // TODO: 数字確認フェーズへ
            Debug.Log("[GameTopicController] 数字確認ボタン押下");
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
