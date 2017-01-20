using UnityEngine;
using UnityEngine.UI;
using System;

public class BookManager : MonoBehaviour
{
	public	static BookManager	instance;
	
	public	Camera				m_BookCamera;
	public	GameObject[]		m_Cards;
	public	MegaBook			m_CurrentBook;
	public	Sprite				m_HiddenCardImage;
	private	float				m_Angle;

	private	int					m_CardsPerPage = 12;
	private	int					m_CurrentPage = 0;
	private	int					m_LastPageRendered = -1;
	
	//=====================================================
	
	void Awake()
	{
		instance = this;
	}
	
	//=====================================================

	public void Start()
	{
		m_CurrentPage = 0;
		m_LastPageRendered = -1;
	}

	//=====================================================
	
	public void MarkDirty()
	{
		m_LastPageRendered = -1;
		Update();
	}

	//=====================================================

	public bool IsPageTurning()
	{
		return( m_CurrentBook.pageTurning );
	}

	//=====================================================
	
	public bool IsValidPage()
	{
		return( m_CurrentBook.IsValidPage() );
	}
	
	//=====================================================

	public int GetCurrentPage()
	{
		return( m_CurrentBook.GetCurrentPage() );
	}

	//=====================================================

	public int GetPositionAtMouse()
	{
		Vector3 WorldPos = m_BookCamera.ScreenToWorldPoint( new Vector3( Input.mousePosition.x , Input.mousePosition.y , 1.0f ) );
		WorldPos.x -= 0.44723f;
		WorldPos.z = -WorldPos.z;
		WorldPos.z += 0.34701f;
		
		WorldPos.x /= 0.8097f;
		WorldPos.z /= 0.65805f;
		
		// 1.257,-0.31104
		//Debug.Log( WorldPos.x.ToString("F5") + " " + WorldPos.z.ToString("F5") );
		if( WorldPos.x < 0.0f )
			return( -1 );
		
		int X = (int)( WorldPos.x * 4 );
		int Y = (int)( WorldPos.z * 3 );
		
		if( X < 0 )
			return( -1 );
		if( X > 3 )
			return( -1 );
		
		//Debug.Log( X + " " + Y );
		X = Mathf.Clamp( X , 0 , 3 );
		Y = Mathf.Clamp( Y , 0 , 2 );
		
		X += ( Y * 4 );
		
		//Debug.Log( X );
		
		return( X );
	}

	//=====================================================

	public void TurnPage( int Delta )
	{
		// Allow page turning
		if( m_CurrentBook.pageTurning == false )
		{
			if( m_CurrentBook.IsValidPage( m_CurrentPage + Delta ) )
			{
				m_CurrentPage += Delta;
				StartCoroutine( m_CurrentBook.FlipToPage( m_CurrentPage ) );
			}
		}
	}

	//=====================================================

	public void TurnToPage( int Page )
	{
		// Allow page turning
		if( m_CurrentBook.pageTurning == false )
		{
			m_CurrentPage = Page;
			StartCoroutine( m_CurrentBook.FlipToPage( m_CurrentPage ) );
		}
	}

	//=====================================================

	public void Update()
	{
		
		/*
		// Randomly move cards around to show it's rendering
		m_Angle += Time.deltaTime * 90.0f;
		
		int Idx = 0;
		foreach( GameObject Obj in m_Cards )
		{
			float Angle = m_Angle + ( Idx * 40 );
			float XOff = Mathf.Sin( Angle * Mathf.Deg2Rad ) * 30.0f;
			float YOff = Mathf.Cos( Angle * Mathf.Deg2Rad ) * 30.0f;

			Vector3 NewVec = m_CardStartPositions[ Idx ];
			NewVec.x += XOff;
			NewVec.y += YOff;
			//Obj.transform.localPosition = NewVec;
			Idx++;
		}
		*/
		
		// Find out which page we're on and render the appropriate cards on that page
		int CurrentPage = m_CurrentBook.GetCurrentPage();
		
		//Debug.Log( m_CurrentBook.GetCurrentPage() );
		
		// Retrieve cards 
		//int NumCards = GameDataManager.Instance.TradingCardDataList.Count;
		
		// Render if page has changed
		bool bUpdateResources = false;
		if( CurrentPage != m_LastPageRendered )
			bUpdateResources = true;
		
		{
			m_LastPageRendered = CurrentPage;
		
			for( int RenderTexturePage = 0 ; RenderTexturePage < 2 ; RenderTexturePage++ )
			{
				int CurRenderTexturePage = RenderTexturePage;
				if( ( CurrentPage & 1 ) == 1 )
					CurRenderTexturePage++;
				
				if( CurRenderTexturePage > 1 )
					CurRenderTexturePage = 0;
				
				// Setup cards for this render texture page
				for( int CurCardOffset = 0 ; CurCardOffset < m_CardsPerPage ; CurCardOffset++ )
				{
					int ListCardOffset = CurCardOffset;
					ListCardOffset += RenderTexturePage * m_CardsPerPage;
					ListCardOffset += CurrentPage * m_CardsPerPage;
					
					// Valid card?
					bool bValidCard = true;
					TradingCardSpreadsheetItem CurSpreadsheetCard = TradingCardItemsManager.GetTradingCardItem( CurrentPage + RenderTexturePage , CurCardOffset );
					if( CurSpreadsheetCard == null )
						bValidCard = false;

					GameObject RenderCard = m_Cards[ CurCardOffset + ( CurRenderTexturePage * m_CardsPerPage ) ];
					GameObject RenderCardScuffedOverlay = RenderCard.transform.parent.gameObject.transform.GetChild( 1 ).gameObject;
					UnityEngine.UI.Image RenderCardImage = RenderCard.GetComponent( typeof( UnityEngine.UI.Image ) ) as UnityEngine.UI.Image;
					UnityEngine.UI.Image RenderCardScuffedOverlayImage = RenderCardScuffedOverlay.GetComponent( typeof( UnityEngine.UI.Image ) ) as UnityEngine.UI.Image;
		
					if( bValidCard )
					{
						//Debug.Log( CurSpreadsheetCard.page + " " + CurSpreadsheetCard.position );
						RenderCard.transform.parent.gameObject.SetActive( true );

						//TradingCardHeldItem CurHeldCard = GameDataManager.Instance.TradingCardDataList[ ListCardOffset ];
						//TradingCardSpreadsheetItem CurSpreadsheetCard = TradingCardItemsManager.GetTradingCardItem( CurHeldCard.id );
						
						// Does the player have this card?
						TradingCardHeldItem CurHeldCard = null;
						
						int NumMint = 0;
						int NumScuffed = 0;
						
						if( GameDataManager.Instance != null )
							CurHeldCard = GameDataManager.Instance.GetHeldTradingCard( CurSpreadsheetCard.id , ref NumMint , ref NumScuffed );
						
						if( CurHeldCard != null )
						{
							CurHeldCard.notifyTimer -= Time.deltaTime;
							CurHeldCard.notifyTimer = Mathf.Clamp( CurHeldCard.notifyTimer , 0.0f , 100.0f );
							
							float fScale = ( Mathf.Sin( CurHeldCard.notifyTimer * 5.0256f ) * 0.1f ) + 1.0f;
							RenderCardImage.gameObject.transform.localScale = new Vector3( fScale , fScale , 1.0f );
							RenderCardScuffedOverlayImage.gameObject.transform.localScale = new Vector3( fScale , fScale , 1.0f );
							
							if( bUpdateResources )
							{
								Sprite RenderSprite = (Sprite)Resources.Load( "Cards/SmallGUITextures/" + CurSpreadsheetCard.smallGuiTexture2D , typeof( Sprite ) );
								RenderCardImage.sprite = RenderSprite;
								RenderCardImage.enabled = true;
								RenderCardScuffedOverlayImage.enabled = true;
							}
						}
						else
						{
							RenderCardImage.sprite = m_HiddenCardImage;
							if( bUpdateResources )
							{
								RenderCardImage.enabled = true;
								RenderCardScuffedOverlayImage.enabled = true;
							}
						}
						
						// Show/hide number of cards
						GameObject sprNumCards = RenderCard.transform.parent.gameObject.transform.GetChild( 2 ).gameObject;
						GameObject sprScuffedOverlay = RenderCard.transform.parent.gameObject.transform.GetChild( 1 ).gameObject;
						GameObject sprNumCardsText = sprNumCards.transform.GetChild( 0 ).gameObject;
						Text txtNumCards = sprNumCardsText.GetComponent( typeof( Text ) ) as Text;
						
						int TotalCards = NumMint + NumScuffed;
						
						// For now use the card number instead of 'number of cards'
						TotalCards = CurSpreadsheetCard.globalPosition + 1; //( ListCardOffset + 1 ); 
						
						if( 1 == 1 ) //TotalCards > 1 )
						{
							sprNumCardsText.SetActive( true );
							txtNumCards.text = TotalCards.ToString();
						}
						else
						{
							sprNumCardsText.SetActive( false );
						}
						
						// Show scuffed overlay?
						if( ( NumMint <= 0 ) && ( NumScuffed > 0 ) )
						{
							sprScuffedOverlay.SetActive( true );
						}
						else
						{
							sprScuffedOverlay.SetActive( false );
						}
					}
					else
					{
						RenderCard.transform.parent.gameObject.SetActive( false );
						//RenderCardImage.enabled = false;
					}
				}
			}
		}
	}

	//=====================================================
	
	public void HideCard( int Page , int Position )
	{
		int CurRenderTexturePage = 0;
		if( ( Page & 1 ) == 1 )
			CurRenderTexturePage++;
		
		GameObject RenderCard = m_Cards[ Position + ( CurRenderTexturePage * m_CardsPerPage ) ];
		GameObject RenderCardScuffedOverlay = RenderCard.transform.parent.gameObject.transform.GetChild( 1 ).gameObject;
		
		UnityEngine.UI.Image RenderCardImage = RenderCard.GetComponent( typeof( UnityEngine.UI.Image ) ) as UnityEngine.UI.Image;
		UnityEngine.UI.Image RenderCardScuffedOverlayImage = RenderCardScuffedOverlay.GetComponent( typeof( UnityEngine.UI.Image ) ) as UnityEngine.UI.Image;
		
		RenderCardImage.enabled = false;
		RenderCardScuffedOverlayImage.enabled = false;
	}

	//=====================================================
}
