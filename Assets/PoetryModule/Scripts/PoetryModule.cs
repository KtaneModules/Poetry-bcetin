﻿using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

public class PoetryModule : MonoBehaviour {

	public KMBombInfo Info;
	public KMBombModule Module;
	public KMAudio Audio;
	public TextMesh stageCounter;
	public TextMesh[] words;
	public KMSelectable[] wordSelectables;
	public Sprite[] girlsDefault;
	public Sprite[] girlsJumpy;
	public SpriteRenderer girlSR,heart;
	public int stageCount;
	private static int _moduleIdCounter = 1;
	private int _moduleId;
	bool solved = false;
	int currentStage=0;
	List<int> winners;
	int pickedGirl;
	float baseY,myAnimTime=-10;
	public float jumpV,jumpTimeSpeed;
	bool isJump = false;
	// Use this for initialization
	int tableH=6, tableW=6;
	public Vector2[] girlLocations;
	string[,] wordTable = new string[,]
	{
		{"Melanie","clarity","flow","fatigue","hollow","Jane"},
		{"energy","sunshine","ocean","reflection","identity","black"},
		{"crowd","heart","weather","words","past","solitary"},
		{"relax","dance","weightless","morality","gaze","failure"},
		{"bunny","lovely","romance","future","focus","search"},
		{"Hana","cookies","compassion","creation","patience","Lacy"}
	};
	KeyValuePair<int, int>[] selection=new KeyValuePair<int, int>[6];

	void Start()
	{
		_moduleId = _moduleIdCounter++;
		Module.OnActivate += ActivateModule;
		Info.OnBombExploded += BombExploded;
		baseY = girlSR.transform.localPosition.z;
		winners = new List<int>();
		RemoveGirl();
		for (int i = 0; i < 6; i++)
		{
			int j = i;
			wordSelectables[i].OnInteract += delegate ()
			{
				OnPress(j);
				return false;
			};
		}
		RemoveWords();
	}

	// Update is called once per frame
	void Update () {
		if (isJump)
		{
			float Y = CalcY();
			myAnimTime += jumpTimeSpeed;
			if (Y > 0)
				girlSR.transform.localPosition = new Vector3(girlSR.transform.localPosition.x, girlSR.transform.localPosition.y, Y + baseY);
			else
			{
				isJump = false;
				if(currentStage != stageCount)
					girlSR.sprite = girlsDefault[pickedGirl];
				girlSR.transform.localPosition = new Vector3(girlSR.transform.localPosition.x, girlSR.transform.localPosition.y, baseY);
			}
		}
	}

	float CalcY() // Calculate Jump Height
	{
		float t = myAnimTime,g=-4,jump1Time=-2*jumpV/g,result=0,jump2mul=0.8f,jump2Time= -2 * jumpV / g * jump2mul;
		if(t> jump1Time + jump2Time)
		{
			isJump = false;
		}
		if (t > jump1Time)
		{
			t -= jump1Time;
			result = jumpV * t * jump2mul + 1 / 2f * g * t * t; ;
		}
		else
			result = jumpV * t + 1 / 2f * g * t * t;
		return result;
	}

	void RemoveWords()
	{
		foreach(TextMesh tm in words)
		{
			tm.text = "";
		}
	}

	void PrintWords()
	{
		for (int i = 0; i < 6; i++)
		{
			words[i].text = KVPToWord(selection[i]);
		}
	}

	void PrintStageCount()
	{
		stageCounter.text = currentStage + "/" + stageCount;
	}
	void SelectRandomWords()
	{
		List<KeyValuePair<int, int>> myList= new List<KeyValuePair<int, int>>();
		for(int i=0; i< tableH; i++)
			for (int j = 0; j < tableW; j++)
				if(!((i==0 || i==tableH-1) && (j==0 || j==tableW-1)))
				{
				myList.Add(new KeyValuePair<int,int>(i,j));
				}
		for(int i=0;i<6;i++)
		{
			int pick = Random.Range(0,myList.Count-1);
			selection[i]=myList[pick];
			myList.RemoveAt(pick);
		}
	}

	void NewStage()
	{
		PrintStageCount();
		SelectRandomWords();
		PrintWords();
		CalculateWinner();
	}

	void CalculateWinner()
	{
		int value = 100;
		winners.Clear();
		for(int i=0;i<6;i++)
		{
			int dist = (int)Mathf.Abs(girlLocations[pickedGirl].x - selection[i].Key) + (int)Mathf.Abs(girlLocations[pickedGirl].y - selection[i].Value);
			PrintDebug("'" + wordTable[selection[i].Key, selection[i].Value] + "'s location on table: " + (selection[i].Value + 1) + " " + (selection[i].Key + 1) +" Distance: " + dist);
			if(dist == value)
			{
				winners.Add(i);
			}
			if(dist < value)
			{
				value = dist;
				winners.Clear();
				winners.Add(i);
			}
		}
		string dbg = "Correct Word(s): ";
		foreach (int i in winners)
			dbg = dbg + " " + wordTable[selection[i].Key, selection[i].Value];
		PrintDebug(dbg);
	}

	string KVPToWord(KeyValuePair<int,int> kvp)
	{
		return wordTable[kvp.Key,kvp.Value];
	}

	void PickGirl()
	{
		 pickedGirl = Random.Range(0, 4);
		PrintDebug("Picked girl " + wordTable[(int)girlLocations[pickedGirl].x, (int)girlLocations[pickedGirl].y] + "'s location on table: " + ((int)girlLocations[pickedGirl].y + 1) + " " + ((int)girlLocations[pickedGirl].x + 1));
	}

	void PrintGirl()
	{
		girlSR.sprite = girlsDefault[pickedGirl];
	}
	void RemoveGirl()
	{
		girlSR.sprite = null;
	}
	void ActivateModule()
	{
		PickGirl();
		PrintGirl();
		NewStage();
	}

	void BombExploded()
	{

	}

	void OnPress(int which)
	{
		PrintDebug("Selected '" + KVPToWord(selection[which]) +"'.");
		if (winners.Contains(which))
		{
			Correct();
		}
		else
			Wrong();
	}
	void Jump()
	{
		isJump = true;
		myAnimTime = 0.001f;
		girlSR.sprite = girlsJumpy[pickedGirl];
	}
	void TurnOffInteraction()
	{
		foreach(KMSelectable sel in wordSelectables)
		{
			sel.gameObject.SetActive(false);
		}
	}
	void PlayPenSound()
	{
		Audio.PlaySoundAtTransform("pen" + Random.Range(1,4), transform);
	}
	void Correct()
	{
		Jump();
		PlayPenSound();
		PrintDebug("Correct Word!");
		currentStage++;
		if (currentStage == stageCount)
		{
			Module.HandlePass();
			heart.enabled = true;
			RemoveWords();
			PrintStageCount();
			TurnOffInteraction();
		}
		else
		{
			NewStage();
		}
		//Jump the girl
	}

    private string TwitchHelpMessage = "Submit your answer with !{0} press bunny, or !{0} bunny.";
    public KMSelectable[] ProcessTwitchCommand(string command)
    {
        string[] split = command.ToLowerInvariant().Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries);
        if (split.Length > 2) return null;
        if (split.Length == 2)
        {
            if (split[0] == "press" || split[0] == "submit")
                split = split.Skip(1).ToArray();
            else
                return null;
        }
        for (int i = 0; i < 6; i++)
        {
            if (words[i].text.ToLowerInvariant().Equals(split[0]))
                return new[] {wordSelectables[i]};
        }
        return null;
    }


    void Wrong()
	{
		PrintDebug("Wrong Word!");
		Module.HandleStrike();
		NewStage();
	}
	private void PrintDebug(string str)
	{
		Debug.LogFormat("[Poetry #{0}] " + str, _moduleId);
	}
}
