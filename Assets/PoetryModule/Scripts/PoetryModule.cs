using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;

public class PoetryModule : MonoBehaviour {

	public KMBombInfo Info;
	public KMBombModule Module;
	public KMAudio Audio;
	public TextMesh stageCounter;
	public TextMesh[] words;
	public KMSelectable[] wordSelectables;
	public int stageCount;
	private static int _moduleIdCounter = 1;
	private int _moduleId;
	bool solved = false;
	int currentStage=0;
	int winner = -1;
	// Use this for initialization
	int tableH=3, tableW=3;
	string[,] wordTable = new string[,]
	{
		{"word1","word2","word3"},
		{"word4","word5","word6"},
		{"word7","word8","word9"}
	};
	KeyValuePair<int, int>[] selection=new KeyValuePair<int, int>[6];

	void Start()
	{
		_moduleId = _moduleIdCounter++;
		Module.OnActivate += ActivateModule;
		Info.OnBombExploded += BombExploded;
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
		stageCounter.text = currentStage + 1 + "/" + stageCount;
	}
	void SelectRandomWords()
	{
		List<KeyValuePair<int, int>> myList= new List<KeyValuePair<int, int>>();
		for(int i=0; i< tableH; i++)
			for (int j = 0; j < tableW; j++)
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
		winner = 0;
	}

	string KVPToWord(KeyValuePair<int,int> kvp)
	{
		return wordTable[kvp.Key,kvp.Value];
	}

	void ActivateModule()
	{
		NewStage();
	}

	void BombExploded()
	{

	}

	void OnPress(int which)
	{
		PrintDebug("Selected '" + KVPToWord(selection[which]) +"'.");
		if (which == winner)
		{
			Correct();
			NewStage();
		}
		else
			Wrong();
	}

	void Correct()
	{
		PrintDebug("Correct Word!");
		if (currentStage == stageCount-1)
			Module.HandlePass();
		else
			currentStage++;
		//Jump the girl
	}

	void Wrong()
	{
		PrintDebug("Wrong Word!");
		Module.HandleStrike();
	}
	private void PrintDebug(string str)
	{
		Debug.LogFormat("[Poetry #{0}] " + str, _moduleId);
	}
}
