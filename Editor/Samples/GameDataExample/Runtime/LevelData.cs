namespace easycodegenunity.Editor.Samples.GameDataExample
{
    [GameData]
    public partial struct LevelData
    {
        [GameDataField] private int levelNumber;
        [GameDataField] private string levelName;
        [GameDataField] private float levelExp;
        [GameDataField] private int levelEnemyCount;
        [GameDataField] private float levelItemDropRate;
    }
}