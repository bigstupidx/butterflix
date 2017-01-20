using UnityEngine;
using System;

public static class ColorExtensions {
	
	//=============================================================================
	
	public static Color ColorFromRGB(this Color color, int r, int g, int b, int a)
	{
		float red, green, blue, alpha = 0.0f;
		red = r / 255.0f;
		green = g / 255.0f;
		blue = b / 255.0f;
		alpha = a / 255.0f;
		
		return new Color(red, green, blue, alpha);
	}
	
	//=============================================================================
	
	public static Color ColorFromHSV(this Color color, int h , float s , float v)
	{
		float H, S, V, R, G, B;
		float p1, p2, p3;
		float f;
		int i;
		
		if (s < 0.0f) s = 0.0f;
		if (v < 0.0f) v = 0.0f;
		if (s > 1.0f) s = 1.0f;
		if (v > 1.0f) v = 1.0f;
		
		S = s;
		V = v;
		H = (h % 360) / 60.0f;
		i = (int)H;
		f = H - i;
		
		p1 = V * (1 - S);
		p2 = V * (1 - (S * f));
		p3 = V * (1 - (S * (1 - f)));
		
		if      (i == 0) { R = V;  G = p3; B = p1; }
		else if (i == 1) { R = p2; G = V;  B = p1; }
		else if (i == 2) { R = p1; G = V;  B = p3; }
		else if (i == 3) { R = p1; G = p2; B = V;  }
		else if (i == 4) { R = p3; G = p1; B = V;  }
		else             { R = V;  G = p1; B = p2; }
		
		return new Color(R, G, B, 1.0f);
	}
	
	//=============================================================================
	
	public static Color RandomColor(this Color color)
	{
		float red, green, blue, alpha = 0.0f;
		red = UnityEngine.Random.Range(0, 256);
		green = UnityEngine.Random.Range(0, 256);
		blue = UnityEngine.Random.Range(0, 256);
		alpha = UnityEngine.Random.Range(0, 256);
		
		return new Color(red, green, blue, alpha);
	}
	
	//=============================================================================	
}
