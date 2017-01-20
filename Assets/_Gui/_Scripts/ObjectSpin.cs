using UnityEngine;
using UnityEngine.UI;
using System.Collections;

//=====================================================

public class ObjectSpin : MonoBehaviour
{
	private	float		m_Rot = 0.0f;
	
	public void Awake()
	{
	}
	
	public void Update()
	{
		m_Rot += Time.deltaTime * 360.0f;
		
		transform.eulerAngles = new Vector3( 0.0f , 0.0f , -m_Rot );
	}
}


//=====================================================
