using UnityEngine;
using System;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class RackScroller : MonoBehaviour
{
	public	static RackScroller	instance;
	
	public	Camera								m_GUI2DCamera;
	
	private	float								m_CurrentItemIndex = -2.0f;
	private	float								m_DestinationItemIndex = -2.0f;

	private	bool								m_bSwiping = false;
	private	float								m_SwipingStartX = 0.0f;
	private	float								m_SwipingAccel = 0.0f;
	private	float								m_SwipingStartItemIndex = 0.0f;
	private	Vector3[]							m_LastWorldPos = new Vector3[ 4 ];
	private	eFairy								m_CurrentFairy;

	//private	int					m_LastItemOffsetRendered;

	//=====================================================
	
	void Awake()
	{
		instance = this;
		m_CurrentFairy = eFairy.BLOOM;
	}
	
	//=====================================================

	public void SetCurrentFairy( eFairy Fairy )
	{
		if( m_CurrentFairy != Fairy )
		{
			m_CurrentFairy = Fairy;
			m_DestinationItemIndex = m_CurrentItemIndex = -2.0f;
		}
	}
	
	//=====================================================

	public void Start()
	{
	}

	//=====================================================
	
	public void Update()
	{
		if( m_bSwiping )
		{
			Vector3 WorldPos = m_GUI2DCamera.ScreenToWorldPoint( new Vector3( Input.mousePosition.x , Input.mousePosition.y , 1.0f ) );
			WorldPos *= 800.0f;
			float SwipingDeltaX = WorldPos.x - m_SwipingStartX;
			
			m_SwipingAccel = ( WorldPos - m_LastWorldPos[ 3 ] ).x;
			for( int MIdx = 3 ; MIdx > 0 ; MIdx-- )
			{
				m_LastWorldPos[ MIdx ] = m_LastWorldPos[ MIdx - 1 ]; 
			}
			m_LastWorldPos[ 0 ] = WorldPos;
			
			m_DestinationItemIndex = m_SwipingStartItemIndex - ( SwipingDeltaX * 0.0079f );
			
			List< ClothingItemData > CurClothingList = ClothingItemsManager.GetClothingItems( m_CurrentFairy );
			int MaxItems = CurClothingList.Count;
			
			float MinSwipePos = (float)-2.0f;
			float MaxSwipePos = (float)( MaxItems - 4 );
			if( MaxSwipePos < 0.0f )
				MaxSwipePos = 0.0f;
			//MinSwipePos -= 0.4f;
			MaxSwipePos += 0.4f;
			
			if( m_DestinationItemIndex < MinSwipePos )
				m_DestinationItemIndex = MinSwipePos;
			if( m_DestinationItemIndex > MaxSwipePos )
				m_DestinationItemIndex = MaxSwipePos;

			if( Input.GetMouseButtonUp( 0 ) )
			{
				m_bSwiping = false;
				//m_DestinationItemIndex = (float)((int)(m_DestinationItemIndex + 0.5f));
				
				m_SwipingAccel = Mathf.Clamp( m_SwipingAccel , -25.0f , 25.0f );
			}
		}
		else
		{
			// Not swiping
			Vector3 WorldPos = m_GUI2DCamera.ScreenToWorldPoint( new Vector3( Input.mousePosition.x , Input.mousePosition.y , 1.0f ) );
			WorldPos *= 800.0f;
			for( int MIdx = 3 ; MIdx > 0 ; MIdx-- )
			{
				m_LastWorldPos[ MIdx ] = WorldPos;
			}
			
			m_SwipingStartX = WorldPos.x;
			m_SwipingStartItemIndex = m_DestinationItemIndex;

			//Debug.Log( WorldPos );
			if( Input.GetMouseButtonDown( 0 ) )
			{
				if( ( WorldPos.y > 1004.0f ) && ( WorldPos.y < 1345.0f ) )
				{
					if( ( WorldPos.x > -497.0f ) && ( WorldPos.x < 314.0f ) )
					{
						m_bSwiping = true;
						m_SwipingAccel = 0.0f;
					}
				}
			}
			else
			{
				//m_DestinationItemIndex -= m_SwipingAccel * Time.deltaTime * 0.35f;
				m_SwipingAccel *= 0.92f;
			}
		}
		
		PreHelpers.DeltaTend( ref m_CurrentItemIndex , m_DestinationItemIndex , 7.5f , Time.deltaTime );
		
		// Update renderer
		if( RackRenderer.instance != null )
			RackRenderer.instance.m_CurItem = m_CurrentItemIndex;
	}

	//=====================================================
	
	public void OnOutfitSelected( int Index )
	{
		//Debug.Log( "OOS: " + Index );
		// Calculate visible index
		int VisibleIndex = Index + (int)m_CurrentItemIndex;
		ClothingRoomManager.instance.OnOutfitSelected( VisibleIndex );
	}
	
	//=====================================================
}
