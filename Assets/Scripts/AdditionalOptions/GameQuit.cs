﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameQuit : MonoBehaviour {
	public void DoGameQuit () {
        Debug.Log("Was quit");
        Application.Quit();
	}
}