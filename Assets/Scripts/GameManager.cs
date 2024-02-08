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

    public static GameManager Instance { get; private set; }
    
    private int _score = 0;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        _timer = _timeLimit;
        _score = 0;
        ChangeGameState(true);
        UpdateUI();
        _replayButton.onClick.AddListener(RestartGame);
        _mainMenuButton.onClick.AddListener(() => SceneManager.LoadScene("MainMenu"));
    }
    
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
            UpdateUI();
        }
    }

    public void AddToScore(int points)
    {
        if (_gameActive)
        {
            _score += points;
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        _scoreText.text = $"Score: {_score}";
        _timerText.text = $"Time: {Mathf.CeilToInt(_timer)}";
    }

    private void EndGame()
    {
        ChangeGameState(false);
        _endScoreText.text = $"Final Score:<br>{_score}";
    }

    public void RestartGame()
    {
        _score = 0;
        _timer = _timeLimit;
        ChangeGameState(true);
        UpdateUI();
    }
}

