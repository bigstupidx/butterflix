using UnityEngine;
using System;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class RackRenderer : MonoBehaviour
{
	public	static RackRenderer	instance;
	
	public	Vector3				m_RackBasePosition;
	public	float				m_RackSpacing = 1.0f;
	public	GameObject[]		m_RackItems;
	public	float				m_CurItem;
	private	int					m_LastItemOffsetRendered;
	private	eFairy				m_CurrentFairy;

	//=====================================================
	
	void Awake()
	{
		instance = this;
		m_CurrentFairy = eFairy.BLOOM;
	}
	
	//=====================================================

	public void Start()
	{
		m_CurItem = 0.0f;
		m_LastItemOffsetRendered = -1;
	}

	//=====================================================
	
	public void MarkDirty()
	{
		m_LastItemOffsetRendered = -1;
	}

	//=====================================================

	public void SetCurrentFairy( eFairy Fairy )
	{
		m_CurrentFairy = Fairy;
	}
	
	//=====================================================

	public float GetCurrentItem()
	{
		return( m_CurItem );
	}

	//=====================================================

	public void Update()
	{
		// Render if page has changed
		bool bUpdateResources = false;
		
		// Position the rack with the correct offset
		float Offset = m_CurItem % 1.0f;
		Offset *= m_RackSpacing;
		this.transform.localPosition = new Vector3( -Offset + m_RackBasePosition.x , m_RackBasePosition.y , m_RackBasePosition.z );
		
		// Load the correct sprites for each piece of clothing into the render slots we have
		int ItemOffset = (int)m_CurItem;
		if( ItemOffset != m_LastItemOffsetRendered )
		{
			//Debug.Log("RES");
			bUpdateResources = true;
		}
		
		m_LastItemOffsetRendered = ItemOffset;
		
		List< ClothingItemData > CurClothingList = ClothingItemsManager.GetClothingItems( m_CurrentFairy );
		int MaxItems = CurClothingList.Count;
		
		for( int Idx = 0 ; Idx < 8 ; Idx++ )
		{
			// Valid outfit?
			int OutfitIndex = Idx + ItemOffset;
			
			bool bValidOutfit = false;
			
			ClothingItemData CurOutfit = null;
			if( OutfitIndex < 0 )
			{
				bValidOutfit = false;
			}
			else
			{
				if( OutfitIndex < MaxItems )
				{
					CurOutfit = CurClothingList[ OutfitIndex ];
					if( CurOutfit == null )
						bValidOutfit = false;
					else
						bValidOutfit = true;
				}
			}
			
			if( bValidOutfit )
			{
				m_RackItems[ Idx ].SetActive( true );
				
				// Update sprite for this outfit
				UnityEngine.SpriteRenderer RenderOutfitImage = m_RackItems[ Idx ].GetComponent( typeof( UnityEngine.SpriteRenderer ) ) as UnityEngine.SpriteRenderer;
				CurOutfit.currentRotation += Time.deltaTime * 1.4f;
				
				if( bUpdateResources )
				{
					string Filename = "Clothing/GUITextures/" + CurOutfit.guiTexture2D;
					Sprite RenderSprite = (Sprite)Resources.Load( Filename , typeof( Sprite ) );
					RenderOutfitImage.sprite = RenderSprite;
					RenderOutfitImage.enabled = true;
				}
					
				float outfitRotation = Mathf.Sin( CurOutfit.currentRotation	) * 7.0f;
				m_RackItems[ Idx ].transform.eulerAngles = new Vector3( 0.0f , 344.0f + outfitRotation , 0.0f );
			}
			else
			{
				m_RackItems[ Idx ].SetActive( false );
			}
		}
		
	}

	//=====================================================
}
