using System.Collections;
using UnityEngine;
using Photon.Bolt;

public class GameOver : MonoBehaviour
{
    public GameObject panel;
    public GameObject resultText;
    public GameObject messageText;

    bool isGameOver = false;
    
    public bool IsGameOver { get => isGameOver; }

    public void Lose(string playerDeadName, CharacterColor playerDeadCharacter)
    {
        string htmlColor = ColorUtility.ToHtmlStringRGBA(playerDeadCharacter.UnityColor()).ToLower();
        Show("Game Over", "<color=#" + htmlColor + ">" + playerDeadName + "</color> died");
    }

    public void Win()
    {
        Show("Victory", "Congratulations!");
    }

    void Show(string result, string message)
    {
        isGameOver = true;

        messageText.GetComponent<TMPro.TextMeshProUGUI>().text = result;
        messageText.GetComponent<TMPro.TextMeshProUGUI>().text = message;

        panel.SetActive(true);

        if (BoltNetwork.IsServer)
            StartCoroutine(Countdown());
    }

    IEnumerator Countdown()
    {
        yield return new WaitForSeconds(5);
        BoltNetwork.LoadScene("Lobby");
    }
}
