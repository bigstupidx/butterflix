[System.Serializable]
public class WildMagicItemData
{
	public string		Id;
	public float		Value;
	public int			PopulationBonus;

	public WildMagicItemData()
	{
		Id = string.Empty;
		Value = 0.0f;
		PopulationBonus = 0;
	}

	public WildMagicItemData Copy()
	{
		var obj = new WildMagicItemData { Id = Id, Value = Value, PopulationBonus = PopulationBonus };

		return obj;
	}
}
