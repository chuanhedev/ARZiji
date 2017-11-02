using UnityEngine;
using System.Collections;
using System;
using Nuke;

public class Angle {
    
	//convert angle from -180 to 180
	public static float ToStardard (float a){
		float temp = Nuke.Utils.ModPositive(a, 360);
		return temp > 180 ? temp - 360 : temp;
	}
}