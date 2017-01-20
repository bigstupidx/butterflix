using UnityEngine;
using UnityEngine.UI;

public class LoadingScreenManager : MonoBehaviour
{
	//=====================================================
	
	public	GameObject	m_LoadingSprite;

	private	AsyncOperation	m_LevelSync = null;
	private	int				m_Stage = 0;
	private	float			m_Timer = 0.0f;
	private	float			m_Rotation = 0.0f;
	private	string			m_LoadingScene = "";

	void Awake()
	{
		m_Timer = 0.0f;
		m_Stage = 0;
	}

	//=====================================================
	
	void Start()
	{
		m_Stage = 0;
		m_Timer = 0.0f;
		m_LoadingScene = PlayerPrefsWrapper.GetString("LoadingScreenScene" , "CommonRoom");
	}

	//=====================================================
	
	void Update()
	{
		m_Timer += Time.deltaTime;
		m_Rotation += Time.deltaTime;
		
		Image sprImage = m_LoadingSprite.GetComponent<Image>();
		switch( m_Stage )
		{
			case 0:
				// Load level
				{
					// Load level
					m_LevelSync = Application.LoadLevelAsync( m_LoadingScene );
					m_LevelSync.allowSceneActivation = false;
					m_Stage = 1;
					m_Timer = -0.2f;
				}
				break;
			
			case 1:
				// Fade in
				{
					float fAlpha = Mathf.Clamp( m_Timer , 0.0f , 0.25f ) * 4.0f;
					sprImage.color = new Color( 1.0f , 1.0f , 1.0f , fAlpha );
					if( m_Timer > 0.25f )
					{
						m_Stage = 2;
						m_Timer = 0.0f;
					}
				}
				break;
				
			case 2:
				// Loading
				if( ( m_LevelSync.progress > 0.85f ) && ( m_Timer > 0.5f ) )
				{
					m_Stage = 3;
					m_Timer = 0.0f;
				}
				break;
				
			case 3:
				// Fade out
				{
					float fAlpha = Mathf.Clamp( m_Timer , 0.0f , 0.25f ) * 4.0f;
					sprImage.color = new Color( 1.0f , 1.0f , 1.0f , 1.0f - fAlpha );
					if( m_Timer > 0.25f )
					{
						m_Stage = 4;
						
						// Activate level
						m_LevelSync.allowSceneActivation = true;
					}
				}
				break;
		}
		
		m_LoadingSprite.transform.eulerAngles = new Vector3( 0.0f , 0.0f , -m_Rotation * 180.0f );
	}
	
	//=====================================================
}
