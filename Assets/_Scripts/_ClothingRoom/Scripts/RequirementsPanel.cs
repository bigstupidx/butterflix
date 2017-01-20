using UnityEngine;
using UnityEngine.UI;
using System;

public class RequirementsPanel : MonoBehaviour
{
	public	GameObject				m_sprKeyRequirementMet;
	public	GameObject				m_sprKeyRequirementNotMet;

	public	GameObject				m_sprPopRequirementMet;
	public	GameObject				m_sprPopRequirementNotMet;

	public	Text					m_txtKeyRequirementValue;
	public	Text					m_txtPopRequirementValue;

	//=====================================================
	
	void Awake()
	{
	}
	
	//=====================================================

	public void Start()
	{
	}

	//=====================================================
	
	public void Setup( FairyItemData CurFairyInfo , int FairyLevel )
	{
		if( CurFairyInfo != null )
		{
			m_txtKeyRequirementValue.text = String.Format( "{0:n0}", CurFairyInfo.KeysRequired[ FairyLevel ] );
			m_txtPopRequirementValue.text = String.Format( "{0:n0}", CurFairyInfo.PopulationRequired[ FairyLevel ] );
			
			if( GameDataManager.Instance.PlayerPopulation >= CurFairyInfo.PopulationRequired[ FairyLevel ] )
			{
				m_sprPopRequirementMet.SetActive( true );
				m_sprPopRequirementNotMet.SetActive( false );
			}
			else
			{
				m_sprPopRequirementMet.SetActive( false );
				m_sprPopRequirementNotMet.SetActive( true );
			}

			if( GameDataManager.Instance.GetNumKeysCollected() >= CurFairyInfo.KeysRequired[ FairyLevel ] )
			{
				m_sprKeyRequirementMet.SetActive( true );
				m_sprKeyRequirementNotMet.SetActive( false );
			}
			else
			{
				m_sprKeyRequirementMet.SetActive( false );
				m_sprKeyRequirementNotMet.SetActive( true );
			}
		}
	}

	//=====================================================
}
