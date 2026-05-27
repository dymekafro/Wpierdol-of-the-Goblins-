namespace WPG.Core
{
    public enum GameState
    {
        MainMenu,
        CharacterCreation,
        Playing,
        Paused,
        Dead
    }

    public static class SceneNames
    {
        public const string MainMenu = "MainMenu";
        public const string CharacterCreation = "CharacterCreation";
        public const string World = "World";
    }
}
