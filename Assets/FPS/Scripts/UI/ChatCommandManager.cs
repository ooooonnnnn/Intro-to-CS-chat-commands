using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.FPS.Game;
using System.Linq;
using TMPro;
using Unity.FPS.Gameplay;

public class ChatCommandManager : MonoBehaviour
{
    [SerializeField] private WeaponController blaster, charger, shotgun;
    [SerializeField] private GameObject chatPanel;
    [SerializeField] private TMP_InputField textInput;
    [SerializeField] private TMP_Text recommendations;
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
    private CommandSymbol rootSymbol, heal, getWeapon, getBlaster, getCharger, getShotgun;

    private ChatCommandParser commandParser = new ChatCommandParser();
    private ChatCommands commands = new ChatCommands();

    private void Awake()
    {
        ConstructTree();
        textInput.onValueChanged.AddListener(UpdateInput);
        textInput.text = "";
        chatPanel.SetActive(false);
        recommendations.text = "";
    }

    private void ConstructTree()
    {
        rootSymbol = new("/");
        heal = new("heal", () => commands.Heal());
        getWeapon = new("get weapon");
        getBlaster = new("blaster", () => commands.GetWeapon(blaster));
        getCharger = new("charger", () => commands.GetWeapon(charger));
        getShotgun = new("shotgun", () => commands.GetWeapon(shotgun));

        rootSymbol.children = new CommandSymbol[] { heal, getWeapon };
        getWeapon.children = new CommandSymbol[] { getBlaster, getCharger, getShotgun };
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
        parsedSymbol = commandParser.ParseCommand(input, rootSymbol, out List<CommandSymbol> recommanded);

        recommendations.text = string.Join("\n", recommanded.Select(sym => sym.phrase));

        string message = parsedSymbol?.phrase ?? "None Chosen";
        print(message);
        print(string.Join(", ", recommanded.Select(sym => sym.phrase)));
    }

    private void HandleEnterKey()
    {
        if (!chatPanel.activeInHierarchy)
        {
            chatPanel.SetActive(true);
            textInput.Select();
            playerController.enabled = false;
            return;
        }

        if (parsedSymbol != null && parsedSymbol.command != null)
        {
            parsedSymbol.command();
        }

        chatPanel.SetActive(false);
        textInput.text = "";
        playerController.enabled = true;
    }

    private void OnDestroy()
    {
        textInput.onValueChanged.RemoveListener(UpdateInput);
    }
}

public class ChatCommandParser
{
    public CommandSymbol ParseCommand(string input, CommandSymbol rootNode, out List<CommandSymbol> recommend)
    {
        recommend = new List<CommandSymbol>();
        rootSymbol = rootNode;
        return ParseCommandRecursive(input, "", rootNode, recommend);
    }

    private CommandSymbol rootSymbol;

    private CommandSymbol ParseCommandRecursive(
        string rawInput, string commandString, CommandSymbol currentNode, List<CommandSymbol> recommend)
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
                    recommend.Add(child);
                }
            }
            return currentNode;
        }
        //If input contained in target, recommend currentNode
        if (commandString.StartsWith(rawInput))
        {
            recommend.Add(currentNode);
            return null;
        }
        //If input CONTAINS target, check children
        if (normalizedInput.StartsWith(commandString))
        {
            foreach (var child in currentNode.children)
            {
                CommandSymbol chosen = ParseCommandRecursive(rawInput, commandString, child, recommend);
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

public class ChatCommands
{
    public void Heal() => Debug.Log("Heal");

    public void GetWeapon(WeaponController weapon) => Debug.Log($"Get weapon {weapon}");
}
