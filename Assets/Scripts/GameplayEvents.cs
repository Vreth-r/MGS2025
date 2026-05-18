using System;

public class GameplayEvents
{
    // Player Health events
    public event Action<int, int> OnPlayerHealthGained;

    public void PlayerHealthGained(int playerID, int healthGained)
    { 
        OnPlayerHealthGained?.Invoke(playerID, healthGained);
    }

    public event Action<int, int> OnPlayerHealthLost;

    public void PlayerHealthLost(int playerID, int healthLost)
    {
        OnPlayerHealthLost?.Invoke(playerID, healthLost);
    }

    // Game Over Event
    public event Action OnGameOver;
    public void GameOver()
    {
        OnGameOver?.Invoke();
    }

    // Event for Combo processing, used for combo display for UI but can be considered a gameplay event so it will stay here
    public event Action<int, Judgement, float> OnPlayerCombo;

    public void ResolvePlayerCombo(int playerID, Judgement judgement, float currentComboCount)
    {
        OnPlayerCombo?.Invoke(playerID, judgement, currentComboCount);
    }


    public void ClearAll()
    { 
        OnGameOver = null;
        OnPlayerHealthGained = null;
        OnPlayerHealthLost = null;
        OnPlayerCombo = null;
    }
}
