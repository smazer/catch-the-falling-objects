using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Dirt-simple main menu with one button to load gameplay scene
/// </summary>
public class MainMenu : MonoBehaviour
{
    [SerializeField] private Button _startButton;
    
    void Start()
    {
        _startButton.onClick.AddListener(StartGame);
    }
    
    private void StartGame()
    {
        SceneManager.LoadSceneAsync("Gameplay");
    }
}
