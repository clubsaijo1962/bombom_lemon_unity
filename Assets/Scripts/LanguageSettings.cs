namespace BomBomLemon
{
    public static class LanguageSettings
    {
        public static bool IsEnglish { get; private set; } = false;
        public static event System.Action OnLanguageChanged;

        public static void Toggle()
        {
            IsEnglish = !IsEnglish;
            OnLanguageChanged?.Invoke();
        }
    }
}
