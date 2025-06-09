using System;
using UnityEngine;

namespace easycodegenunity.Editor.Samples.GameDataExample
{
    public partial struct PlayerData
    {
        public event Action<int> OnPlayerHealthChanged;
        public event Action<int> OnPlayerScoreChanged;
        public event Action<string> OnPlayerNameChanged;
        public event Action<Vector3> OnPlayerPositionChanged;
        public void SetPlayerHealth(int newValue)
        {
            playerHealth = newValue; OnPlayerHealthChanged ? . Invoke ( newValue ) ; 
        }

        public int GetPlayerHealth()
        {
            return playerHealth;
        }

        public void SetPlayerScore(int newValue)
        {
            playerScore = newValue; OnPlayerScoreChanged ? . Invoke ( newValue ) ; 
        }

        public int GetPlayerScore()
        {
            return playerScore;
        }

        public void SetPlayerName(string newValue)
        {
            playerName = newValue; OnPlayerNameChanged ? . Invoke ( newValue ) ; 
        }

        public string GetPlayerName()
        {
            return playerName;
        }

        public void SetPlayerPosition(Vector3 newValue)
        {
            playerPosition = newValue; OnPlayerPositionChanged ? . Invoke ( newValue ) ; 
        }

        public Vector3 GetPlayerPosition()
        {
            return playerPosition;
        }
    }
}