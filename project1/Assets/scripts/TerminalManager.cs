using TMPro;
using UnityEngine;

public class TerminalManager : MonoBehaviour
{
    public TMP_Text outputText;          // text object inside scroll view
    public TMP_InputField inputField;    // input field for typing

    void Start()
    {
        inputField.onSubmit.AddListener(HandleCommand);
    }

    void HandleCommand(string command)
    {
        // Show what player typed
        outputText.text += "\n> " + command;

        // Example response
        if (command.ToLower() == "help")
            outputText.text += "\nAvailable commands: help, exit, ping, nmap";
        else if (command.ToLower() == "ping")
            outputText.text += "\nPONG!";
        else if (command.ToLower() == "nmap")
            outputText.text += "\nScanning 192.168.1.23...\r\nPORT     STATE SERVICE\r\n22/tcp   open  ssh\r\n80/tcp   open  http";
        else
            outputText.text += "\nUnknown command.";

        // Clear input for next command
        inputField.text = "";

        // Refocus so player can keep typing
        inputField.ActivateInputField();
    }

    public void BruteForce()
    {

    }

}
