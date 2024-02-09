using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    
    private float _timeLimit = 60.0f;
    [SerializeField] private GameObject _gameplayScorePanel;
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private TextMeshProUGUI _timerText;
    
    
    [SerializeField] private GameObject _gameplayEndPanel;
    [SerializeField] private TextMeshProUGUI _endScoreText;
    [SerializeField] private Button _replayButton;
    [SerializeField] private Button _mainMenuButton;

    public delegate void GameStateChangedEvent(bool active);
    public event GameStateChangedEvent OnGameStateChanged;
    
    
    private float _timer;
    private bool _gameActive;

    //singleton for a quick exercise, generally would use DI or Service Locator
    public static GameManager Instance { get; private set; } 
    
    private int _score = 0;
    private void Awake() //singleton static reference setup
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        //if this was a duplicate being destroyed in Awake...don't kill the legitimate singleton instance
        if (Instance == this) 
        {
            Instance = null;
        }
    }

    private void Start()
    {
        InitNewGame();
        _replayButton.onClick.AddListener(InitNewGame);
        _mainMenuButton.onClick.AddListener(() => SceneManager.LoadScene("MainMenu"));
    }
    
    /// <summary>
    /// State changed event firing, on restart and game over
    /// </summary>
    /// <param name="active"></param>
    private void ChangeGameState(bool active)
    {
        _gameActive = active;
        OnGameStateChanged?.Invoke(_gameActive);
        
        _gameplayScorePanel.SetActive(active);
        _gameplayEndPanel.SetActive(!active);
    }

    void Update()
    {
        if (_gameActive)
        {
            _timer -= Time.deltaTime;
            if (_timer <= 0)
            {
                EndGame();
            }
            UpdateTime();
        }
    }

    public void AddToScore(int points)
    {
        if (_gameActive)
        {
            _score += points;
            UpdateScore();
        }
    }

    private void UpdateTime()
    {
        _timerText.text = $"Time: {Mathf.CeilToInt(_timer)}";
    }

    private void UpdateScore()
    {
        _scoreText.text = $"Score: {_score}";
    }
    

    private void EndGame()
    {
        ChangeGameState(false);
        _endScoreText.text = $"Final Score:<br>{_score}";
    }

    public void InitNewGame()
    {
        _score = 0;
        _timer = _timeLimit;
        ChangeGameState(true);
        UpdateTime();
        UpdateScore();
    }
}

