using System;

public sealed class ChestReward {

	public int Gems { get; set; }
	public TradingCardSpreadsheetItem Card { get; set; }
	public eTradingCardCondition CardCondition { get; set; }
	public eSwitchItem SwitchItem { get; set; }

	//=====================================================

	public ChestReward()
	{
		Gems = 0;
		Card = new TradingCardSpreadsheetItem();
		CardCondition = eTradingCardCondition.MINT;
		SwitchItem = eSwitchItem.NULL;
	}

	//=====================================================

	public override string ToString()
	{
		if(Card == null)
			Card = new TradingCardSpreadsheetItem();
		
		return String.Format( "Reward:: Gems: {0} : Card Id: {1} : SwitchItem: {2}", Gems, Card.id, SwitchItem );
	}

	//=====================================================
}
