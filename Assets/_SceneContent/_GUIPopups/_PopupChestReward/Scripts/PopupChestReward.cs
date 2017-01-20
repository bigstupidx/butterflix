using UnityEngine;
using System.Collections;

public class PopupChestReward : MonoBehaviour
{
	public 	static PopupChestReward			instance					= null;

	public	GameObject				m_GUICamera;
	public	UnityEngine.UI.Image 	m_RenderCardImage;
	public	UnityEngine.UI.Image 	m_RenderCardOverlayImage;
	public	UnityEngine.UI.Text 	m_RenderCardPosition;

	private TradingCardHeldItem		_currentCard;
	
	//=====================================================

	void Awake()
	{
		instance = this;
	}
	
	//=====================================================
	
	public void Show( TradingCardHeldItem CurHeldCard, float delay = 0.0f )
	{
		_currentCard = CurHeldCard;

		if( InputManager.Instance != null )
			InputManager.Instance.OnBlockInput( true );

		StartCoroutine( ShowPopup( delay ) );
	}

	//=====================================================

	public void OnButtonPressed_OK()
	{
		m_GUICamera.SetActive( false );

		if( InputManager.Instance != null )
			InputManager.Instance.OnBlockInput( false );
	}

	//=====================================================

	private IEnumerator ShowPopup( float delay )
	{
		yield return new WaitForSeconds( delay );

		// Setup card GUI texture
		var CurSpreadsheetCard = TradingCardItemsManager.GetTradingCardItem( _currentCard.id );
		var RenderSprite = (Sprite)Resources.Load( "Cards/SmallGUITextures/" + CurSpreadsheetCard.smallGuiTexture2D, typeof( Sprite ) );
		m_RenderCardImage.sprite = RenderSprite;
		m_RenderCardPosition.text = ( CurSpreadsheetCard.globalPosition + 1 ).ToString();

		if( _currentCard.condition == eTradingCardCondition.MINT )
			m_RenderCardOverlayImage.enabled = false;
		else
			m_RenderCardOverlayImage.enabled = true;

		m_GUICamera.SetActive( true );
	}

	//=====================================================
}
