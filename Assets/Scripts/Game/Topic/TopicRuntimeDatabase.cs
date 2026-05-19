using System;
using System.Collections.Generic;
using UnityEngine;

namespace BomBomLemon.Game.Topics
{
    [Serializable]
    public class TopicData
    {
        public string Text, LowLabel, HighLabel, HintLow, HintHigh;
        public string TextEN, LowLabelEN, HighLabelEN, HintLowEN, HintHighEN;
    }

    [Serializable]
    class TopicDataList { public List<TopicData> items = new(); }

    public class TopicRuntimeDatabase : MonoBehaviour
    {
        const string PrefsKey = "BomBomLemon_Topics_v1";

        public static TopicRuntimeDatabase Instance { get; private set; }

        [SerializeField] private TopicDatabase defaultDatabase;

        public List<Topic> Topics { get; private set; } = new();

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Load();
        }

        public void Load()
        {
            if (PlayerPrefs.HasKey(PrefsKey))
            {
                try
                {
                    var wrapper = JsonUtility.FromJson<TopicDataList>(PlayerPrefs.GetString(PrefsKey));
                    if (wrapper?.items != null && wrapper.items.Count > 0)
                    {
                        Topics = new List<Topic>();
                        foreach (var d in wrapper.items) Topics.Add(FromData(d));
                        return;
                    }
                }
                catch (Exception e) { Debug.LogWarning($"[TopicRuntimeDatabase] Load failed: {e.Message}"); }
            }
            LoadDefaults();
        }

        public void LoadDefaults()
        {
            Topics = new List<Topic>();
            var db = defaultDatabase != null ? defaultDatabase
                     : Resources.Load<TopicDatabase>("TopicDatabase");
            if (db == null)
            {
                Debug.LogError("[TopicRuntimeDatabase] TopicDatabase が見つかりません。Assets/Resources/TopicDatabase.asset を確認してください。");
                return;
            }
            foreach (var t in db.Topics)
                Topics.Add(new Topic(t.Text, t.LowLabel, t.HighLabel, t.HintLow, t.HintHigh,
                                     t.TextEN, t.LowLabelEN, t.HighLabelEN, t.HintLowEN, t.HintHighEN));
            Debug.Log($"[TopicRuntimeDatabase] {Topics.Count} topics loaded");
        }

        public void Save()
        {
            var list = new TopicDataList();
            foreach (var t in Topics) list.items.Add(ToData(t));
            PlayerPrefs.SetString(PrefsKey, JsonUtility.ToJson(list));
            PlayerPrefs.Save();
        }

        public void ResetToDefaults()
        {
            PlayerPrefs.DeleteKey(PrefsKey);
            LoadDefaults();
        }

        public Topic GetRandom()
        {
            if (Topics.Count == 0) return null;
            return Topics[UnityEngine.Random.Range(0, Topics.Count)];
        }

        static TopicData ToData(Topic t) => new()
        {
            Text = t.Text, LowLabel = t.LowLabel, HighLabel = t.HighLabel,
            HintLow = t.HintLow, HintHigh = t.HintHigh,
            TextEN = t.TextEN, LowLabelEN = t.LowLabelEN, HighLabelEN = t.HighLabelEN,
            HintLowEN = t.HintLowEN, HintHighEN = t.HintHighEN
        };

        static Topic FromData(TopicData d) => new(
            d.Text, d.LowLabel, d.HighLabel, d.HintLow, d.HintHigh,
            d.TextEN, d.LowLabelEN, d.HighLabelEN, d.HintLowEN, d.HintHighEN
        );
    }
}
