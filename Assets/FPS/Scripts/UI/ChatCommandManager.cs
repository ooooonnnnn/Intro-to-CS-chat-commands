using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.FPS.Game;
using System.Linq;

public class ChatCommandManager : MonoBehaviour
{
    private readonly CommandSymbol rootSymbol, heal, getWeapon, getBlaster, getCharger, getShotgun;

    [SerializeField] private string input = "";
    [SerializeField] private WeaponController blaster, charger, shotgun;

    private ChatCommandParser commandParser = new ChatCommandParser();

    private void Update()
    {
        CommandSymbol chosen = commandParser.ParseCommand(input, rootSymbol, out List<CommandSymbol> recommanded);
        string message = chosen?.phrase ?? "None Chosen";
        print(message);
        print(string.Join(", ", recommanded.Select(sym => sym.phrase)));
    }

    public ChatCommandManager()
    {
        rootSymbol = new("/");
        heal = new("heal", () => Heal());
        getWeapon = new("get weapon");
        getBlaster = new("blaster", () => GetWeapon(blaster));
        getCharger = new("charger", () => GetWeapon(charger));
        getShotgun = new("shotgun", () => GetWeapon(shotgun));

        rootSymbol.children = new CommandSymbol[]{ heal, getWeapon};
        getWeapon.children = new CommandSymbol[]{ getBlaster, getCharger, getShotgun };
    }

    private void Heal() => print("Heal");

    private void GetWeapon(WeaponController weapon) => print($"Get weapon {weapon}");
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
        string rawInput, string targetString, CommandSymbol currentNode, List<CommandSymbol> recommend)
    {
        if (rawInput == "")
            return null;

        string normalizedInput = string.Concat(rawInput.TrimEnd(), " ");

        //Construct target string
        targetString = string.Concat(targetString, currentNode.phrase);
        if (currentNode != rootSymbol && currentNode.command == null)
            targetString = string.Concat(targetString, " ");

        //Check if input fits the target string
        if (normalizedInput == targetString || rawInput == targetString)
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
        if (targetString.StartsWith(rawInput))
        {
            recommend.Add(currentNode);
            return null;
        }
        //If input CONTAINS target, check children
        if (normalizedInput.StartsWith(targetString))
        {
            foreach (var child in currentNode.children)
            {
                if (ParseCommandRecursive(rawInput, targetString, child, recommend) != null)
                    return child;
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
