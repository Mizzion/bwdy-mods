using System;
using System.Collections.Generic;
using bwdyworks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;

namespace PeopleSeeds;

internal class People
{
	public static readonly string[] PersonNames = new string[28]
	{
		"Haley", "Abigail", "Penny", "Emily", "Maru", "Leah", "Harvey", "Elliott", "Sebastian", "Shane",
		"Alex", "Sam", "Evelyn", "George", "Gus", "Jodi", "Lewis", "Linus", "Marnie", "Pam",
		"Robin", "Demetrius", "Clint", "Caroline", "Pierre", "Sandy", "Willy", "Wizard"
	};

	private static readonly int[] SeedIds = new int[People.PersonNames.Length];

	public static HashSet<string> Banished = new HashSet<string>();

	internal static Dictionary<string, NPC> CloneTemplates { get; set; }

	internal static Dictionary<string, int> CloneGenerations { get; set; }

	public static string GetAngryFace(int personId)
	{
		string person = People.GetPerson(personId);
		if (personId < 12)
		{
			return "$5";
		}
		return person switch
		{
			"Caroline" => "$3", 
			"Clint" => "$3", 
			"Demetrius" => "$4", 
			"Evelyn" => "$2", 
			"George" => "$3", 
			"Gus" => "$3", 
			"Jodi" => "$3", 
			"Lewis" => "$4", 
			"Linus" => "$3", 
			"Marnie" => "$3", 
			"Pam" => "$4", 
			"Pierre" => "$3", 
			"Robin" => "$3", 
			"Sandy" => "$2", 
			"Willy" => "$2", 
			"Wizard" => "$1", 
			_ => "$0", 
		};
	}

	public static string GetScaredFace(int personId)
	{
		string person = People.GetPerson(personId);
		switch (person)
		{
		case "Abigail":
		case "Alex":
			return "$7";
		case "Emily":
			return "$6";
		case "Haley":
			return "$3";
		case "Maru":
			return "$9";
		case "Sam":
			return "$8";
		case "Shane":
			return "$10";
		default:
			if (personId < 12)
			{
				return "$2";
			}
			return person switch
			{
				"Caroline" => "$2", 
				"Clint" => "$4", 
				"Demetrius" => "$6", 
				"Evelyn" => "$2", 
				"George" => "$2", 
				"Gus" => "$2", 
				"Jodi" => "$4", 
				"Lewis" => "$3", 
				"Linus" => "$3", 
				"Marnie" => "$4", 
				"Pam" => "$3", 
				"Pierre" => "$4", 
				"Robin" => "$5", 
				"Sandy" => "$2", 
				"Willy" => "$1", 
				"Wizard" => "$1", 
				_ => "$0", 
			};
		}
	}

	public static string GetStrangeFace(int personId)
	{
		string person = People.GetPerson(personId);
		switch (person)
		{
		case "Alex":
			return "$8";
		case "Emily":
			return "$7";
		case "Haley":
			return "$8";
		default:
			if (personId < 12)
			{
				return "$4";
			}
			return person switch
			{
				"Caroline" => "$1", 
				"Clint" => "$5", 
				"Demetrius" => "$1", 
				"Evelyn" => "$1", 
				"George" => "$1", 
				"Gus" => "$3", 
				"Jodi" => "$1", 
				"Lewis" => "$2", 
				"Linus" => "$1", 
				"Marnie" => "$2", 
				"Pam" => "$1", 
				"Pierre" => "$2", 
				"Robin" => "$4", 
				"Sandy" => "$3", 
				"Willy" => "$3", 
				"Wizard" => "$1", 
				_ => "$0", 
			};
		}
	}

	internal static void OnLoad()
	{
		People.CloneTemplates = new Dictionary<string, NPC>();
		People.CloneGenerations = new Dictionary<string, int>();
		string[] personNames = People.PersonNames;
		foreach (string text in personNames)
		{
			NPC characterFromName = Game1.getCharacterFromName(text, false);
			People.CloneTemplates.Add(text, new NPC(new AnimatedSprite("Characters\\" + text, 0, 16, 32), characterFromName.position.Value, characterFromName.getHome().Name, 2, text, characterFromName.datable.Value, (Dictionary<int, int[]>)null, Game1.content.Load<Texture2D>("Portraits\\" + text)));
			People.CloneGenerations.Add(text, 0);
		}
		personNames = People.PersonNames;
		foreach (string text2 in personNames)
		{
			People.SeedIds[Array.IndexOf(People.PersonNames, text2)] = Modworks.Items.GetModItemId(Mod.Module, text2 + "Seeds").Value;
		}
	}

	internal static bool IsSeed(int itemIndex)
	{
		return Array.IndexOf(People.SeedIds, itemIndex) >= 0;
	}

	internal static void OnStartup()
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Expected O, but got Unknown
		string[] personNames = People.PersonNames;
		foreach (string text in personNames)
		{
			string text2 = text + "Seeds";
			Modworks.Items.AddItem(Mod.Module, new BasicItemEntry((Mod)(object)Mod.Instance, text2, 30, -300, "Basic", -74, text + " Seeds", "Plant in spring, summer, or fall."));
		}
	}

	internal static int GetSeedItemId(NPC npc)
	{
		return People.GetSeedItemId(npc.Name);
	}

	internal static int GetSeedItemId(int personId)
	{
		return People.GetSeedItemId(People.GetPerson(personId));
	}

	internal static int GetSeedItemId(string npc)
	{
		string text = npc + "Seeds";
		int? modItemId = Modworks.Items.GetModItemId(Mod.Module, text);
		if (!modItemId.HasValue)
		{
			return -1;
		}
		return modItemId.Value;
	}

	internal static int GetPersonId(NPC npc)
	{
		return People.GetPersonId(npc.Name);
	}

	internal static int GetPersonId(int seedId)
	{
		return Array.IndexOf(People.SeedIds, seedId);
	}

	internal static int GetPersonId(string name)
	{
		if (name.ToLower().Contains("clone"))
		{
			name = name.Substring(0, name.LastIndexOf('(') - 1);
		}
		return Array.IndexOf(People.PersonNames, name);
	}

	internal static string GetPerson(int index)
	{
		return People.PersonNames[index];
	}

	internal static NPC ClonePerson(int index)
	{
		return People.ClonePerson(People.GetPerson(index));
	}

	internal static NPC ClonePerson(NPC npc)
	{
		return People.ClonePerson(npc.Name);
	}

	internal static NPC ClonePerson(string name)
	{
		NPC nPC = People.CloneTemplates[name];
		People.CloneGenerations[name]++;
		NPC nPC2 = new NPC(new AnimatedSprite("Characters\\" + nPC.Name, 0, 16, 32), new Vector2(-5000f, -5000f), "BugLand", nPC.DirectionIndex, nPC.Name, nPC.datable.Value, (Dictionary<int, int[]>)null, Game1.content.Load<Texture2D>("Portraits\\" + nPC.Name));
		nPC2.FacingDirection = Modworks.RNG.Next(4);
		nPC2.id = 100 + (People.CloneGenerations[name] * 100 + nPC.id);
		string displayName = (nPC2.Name = nPC.Name);
		nPC2.displayName = displayName;
		Game1.player.friendshipData[nPC2.displayName] = new Friendship(0);
		return nPC2;
	}
}
