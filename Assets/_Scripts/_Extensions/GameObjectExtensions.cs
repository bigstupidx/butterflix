using UnityEngine;
using System;
using System.Collections;

public static class GameObjectExtensions {
	
	//=============================================================================
	
	public static IEnumerator FadeOut(this GameObject gameObj, float duration = 1.0f, Action onComplete = null)
	{
		// Avoid divisions by zero
		if(duration <= 0)
			duration = 1.0f;
		
		MeshRenderer renderer = gameObj.GetComponent<MeshRenderer>();
		if(null != renderer)
		{
			if(null != renderer.material)
			{
				Color color = renderer.material.color;
				float startingAlpha = renderer.material.color.a;
				float alpha = 1.0f;
				
				while(renderer.material.color.a > 0.0f)
				{	
					alpha = color.a;
					alpha -= Time.deltaTime * startingAlpha / duration;
					if(alpha < 0.0f)
						alpha = 0.0f;
					color = new Color(color.r, color.g, color.b, alpha);
					renderer.material.color = color;
					
					yield return null;
				} 
			}
		}
		
		// Fade complete
		if(onComplete != null)
			onComplete();
	}
	
	//=============================================================================
	
	public static IEnumerator FadeIn(this GameObject gameObj, float duration = 1.0f, Action onComplete = null)
	{
		// Avoid divisions by zero
		if(duration <= 0)
			duration = 1.0f;
		
		MeshRenderer renderer = gameObj.GetComponent<MeshRenderer>();
		if(null != renderer)
		{
			if(null != renderer.material)
			{
				Color color = renderer.material.color;
				float endAlpha = 1.0f;
				float alpha = 0.0f;
				
				while(renderer.material.color.a < 1.0f)
				{
					alpha = color.a;
					alpha += Time.deltaTime * endAlpha / duration;
					if(alpha > 1.0f)
						alpha = 1.0f;
					color = new Color(color.r, color.g, color.b, alpha);
					renderer.material.color = color;
					
					yield return null;
				} 
			}
		}
		
		// Fade complete
		if(onComplete != null)
			onComplete();
	}
	
	//=============================================================================
	
	public static IEnumerator SetAlpha(this GameObject gameObj, float alpha = 1.0f, Action onComplete = null)
	{
		if(alpha < 0.0f)
			alpha = 0.0f;
		else if(alpha > 1.0f)
			alpha = 1.0f;
		
		MeshRenderer renderer = gameObj.GetComponent<MeshRenderer>();
		if(null != renderer)
		{
			if(null != renderer.material)
			{
				Color color = renderer.material.color;
				color.a = alpha;
				renderer.material.color = color;
			}
		}
		
		// Fade complete
		if(onComplete != null)
			onComplete();
		else
			yield return null;
	}
	
	//=============================================================================
}
