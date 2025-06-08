using UnityEngine;

namespace easycodegenunity.Editor.Samples.GameDataExample
{
    [GameData]
    public partial struct PlayerData
    {
        [GameDataField] private int playerHealth;
        [GameDataField] private int playerScore;
        [GameDataField] private string playerName;
        [GameDataField] private Vector3 playerPosition;

     
    }
}