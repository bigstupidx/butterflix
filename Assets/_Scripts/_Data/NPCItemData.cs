[System.Serializable]
public class NPCItemData
{
	public string		Id;
	public string		CardId;
	public int			PopulationUnlock;
	public int			GemReward;

	public NPCItemData()
	{
		Id = string.Empty;
		CardId = string.Empty;
		PopulationUnlock = 0;
		GemReward = 0;
	}

	public NPCItemData Copy()
	{
		var obj = new NPCItemData { Id = Id,
									CardId = CardId,
									PopulationUnlock = PopulationUnlock,
									GemReward = GemReward
		};

		return obj;
	}
}
