using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.FPS.Gameplay;
using System.Linq;
using TMPro;
using System.Collections;

public class ChatCommandManager : MonoBehaviour
{
    [SerializeField] private GameObject chatPanel;
    [SerializeField] private ChatCommands chatCommands;
    [SerializeField] private TMP_InputField textInput;
    [SerializeField] private TMP_Text sugggestionsText;
    [SerializeField] private PlayerCharacterController playerController;
    private string input
    {
        get => _input;
        set
        {
            _input = value;
            ParseNewInput();
        }
    }
    private string _input;
    private CommandSymbol parsedSymbol;

    //Command tree
    private CommandSymbol rootSymbol;

    private ChatCommandParser commandParser = new ChatCommandParser();

    private void Start()
    {
        rootSymbol = chatCommands.rootSymbol;

        textInput.onValueChanged.AddListener(UpdateInput);
        textInput.text = "";
        chatPanel.SetActive(false);
        sugggestionsText.text = "";
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            HandleEnterKey();
        }
    }

    private void UpdateInput(string newInput) => input = newInput;

    private void ParseNewInput()
    {
        parsedSymbol = commandParser.ParseCommand(input, rootSymbol, out List<CommandSymbol> suggested);

        sugggestionsText.text = string.Join("\n", suggested.Select(sym => sym.phrase));

        string message = parsedSymbol?.phrase ?? "None Chosen";
        print(message);
        print(string.Join(", ", suggested.Select(sym => sym.phrase)));
    }

    private void HandleEnterKey()
    {
        if (!chatPanel.activeInHierarchy)
        {
            chatPanel.SetActive(true);
            StartCoroutine(FocusInputNextFrame());
            playerController.enabled = false;
            return;
        }

        if (parsedSymbol != null && parsedSymbol.command != null)
        {
            parsedSymbol.command();
        }

        textInput.Select();
        chatPanel.SetActive(false);
        textInput.text = "";
        playerController.enabled = true;
    }

    private IEnumerator FocusInputNextFrame()
    {
        yield return null; // wait one frame
        textInput.Select();
        textInput.ActivateInputField();
    }

    private void OnDestroy()
    {
        textInput.onValueChanged.RemoveListener(UpdateInput);
    }
}

public class ChatCommandParser
{
    public CommandSymbol ParseCommand(string input, CommandSymbol rootNode, out List<CommandSymbol> suggest)
    {
        suggest = new List<CommandSymbol>();
        rootSymbol = rootNode;
        return ParseCommandRecursive(input, "", rootNode, suggest);
    }

    private CommandSymbol rootSymbol;

    private CommandSymbol ParseCommandRecursive(
        string rawInput, string commandString, CommandSymbol currentNode, List<CommandSymbol> suggest)
    {
        if (rawInput == "")
            return null;

        string normalizedInput = string.Concat(rawInput.TrimEnd(), " ");

        //Construct target string
        commandString = string.Concat(commandString, currentNode.phrase);
        if (currentNode != rootSymbol && currentNode.command == null)
            commandString = string.Concat(commandString, " ");

        //Check if input fits the target string
        if (normalizedInput == commandString || rawInput == commandString)
        {
            if (currentNode.children != null)
            {
                foreach (CommandSymbol child in currentNode.children)
                {
                    suggest.Add(child);
                }
            }
            return currentNode;
        }
        //If input contained in target, recommend currentNode
        if (commandString.StartsWith(rawInput))
        {
            suggest.Add(currentNode);
            return null;
        }
        //If input CONTAINS target, check children
        if (normalizedInput.StartsWith(commandString))
        {
            foreach (var child in currentNode.children)
            {
                CommandSymbol chosen = ParseCommandRecursive(rawInput, commandString, child, suggest);
                if (chosen != null)
                    return chosen;
                //null => no fit, not null => perfect fit
            }
        }

        //Non of the above => wrong input
        return null;
    }
}

public class CommandSymbol
{
    public string phrase { get; private set; }
    public CommandSymbol[] children;
    public Action command {  get; private set; }

    public CommandSymbol(string phrase, Action command = null)
    {
        this.phrase = phrase;
        this.command = command;
    }
}
