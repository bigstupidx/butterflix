using UnityEngine;
using UnityEditor;

[CustomEditor( typeof( Chest ) )]
public class ChestEditor : Editor
{
	private Chest _myTarget;

	private SerializedObject _serializedTarget;
	private SerializedProperty _rewardSwitchItem;
	private SerializedProperty _cardRarity;
	private SerializedProperty _cardClassification;
	private SerializedProperty _cardCondition;
	private SerializedProperty _clipOpen;
	private SerializedProperty _clipReward;
	
	private string[] _models;

	//=====================================================

	void OnEnable()
	{
		_serializedTarget = new SerializedObject( target );
		_myTarget = (Chest)target;
		_cardRarity = _serializedTarget.FindProperty( "_cardRarity" );
		_cardClassification = _serializedTarget.FindProperty( "_cardClassification" );
		_cardCondition = _serializedTarget.FindProperty( "_cardCondition" );
		_rewardSwitchItem = _serializedTarget.FindProperty( "_rewardSwitchItem" );
		_clipOpen = _serializedTarget.FindProperty( "_clipOpen" );
		_clipReward = _serializedTarget.FindProperty( "_clipReward" );

		// Get list of models for this object-type
		GetModels();
	}

	//=====================================================

	public override void OnInspectorGUI()
	{
		_serializedTarget.Update();

		EditorGUILayout.Space();
		switch( _myTarget.Type )
		{
			case eChestType.SMALL:
				EditorGUILayout.LabelField( "Chest Type: ", "Small" );
				break;
			case eChestType.MEDIUM:
				EditorGUILayout.LabelField( "Chest Type: ", "Medium" );
				break;
			case eChestType.LARGE:
				EditorGUILayout.LabelField( "Chest Type: ", "Large" );
				break;
		}

		// Switch Model
		EditorGUILayout.Space();
		_myTarget.Model = EditorGUILayout.Popup( "Chest Model", _myTarget.Model, _models );

		// AudioClips
		EditorGUILayout.Space();
		EditorGUILayout.PropertyField( _clipOpen, new GUIContent( "Open Chest Clip" ), false );
		EditorGUILayout.PropertyField( _clipReward, new GUIContent( "Collect Reward Clip" ), false );

		// Interactive Level - Player's fairy needs to be at min-level in order to interact with the obstacle
		EditorGUILayout.Space();
		_myTarget.InteractiveLevel = EditorGUILayout.IntSlider( "Min Interactive Level", _myTarget.InteractiveLevel, 0, 7 );

		// Card classification
		if( _rewardSwitchItem.intValue == (int)eSwitchItem.NULL )
			_cardClassification.intValue = (int)(eTradingCardClassification)EditorGUILayout.EnumPopup( "Card Classification", (eTradingCardClassification)_cardClassification.intValue );

		// Card rarity
		if( _rewardSwitchItem.intValue == (int)eSwitchItem.NULL )
			_cardRarity.intValue = (int)(eTradingCardRarity)EditorGUILayout.EnumPopup( "Card Rarity", (eTradingCardRarity)_cardRarity.intValue );

		// Card condition
		if( _rewardSwitchItem.intValue == (int)eSwitchItem.NULL )
			_cardCondition.intValue = (int)(eTradingCardCondition)EditorGUILayout.EnumPopup( "Card Condition", (eTradingCardCondition)_cardCondition.intValue );

		EditorGUILayout.Space();
		if(_myTarget.Type == eChestType.LARGE)
		{
			// Special switch item - overrides any gems or card reward
			_rewardSwitchItem.intValue = (int)(eSwitchItem)EditorGUILayout.EnumPopup( "Switch Item Reward", (eSwitchItem)_rewardSwitchItem.intValue );
		}

		// Pressure switch radius
		//if( _myTarget.Type == eSwitchType.PRESSURE )
		//	_myTarget.Radius = EditorGUILayout.Slider( "Pressure Radius", _myTarget.Radius, 0.1f, 2.5f );
 
		var autoRefresh = false;
		if( GUI.changed )
		{
			EditorUtility.SetDirty( target );
			EditorUtility.SetDirty( _myTarget );

			autoRefresh = true;
		}

		// Refresh serialized parameters
		_serializedTarget.ApplyModifiedProperties();

		if( autoRefresh )
			_myTarget.Refresh();
	}

	//=====================================================

	private void GetModels()
	{
		// Get list of models for this object-type
		switch( _myTarget.Type )
		{
			case eChestType.SMALL:
				_models = ResourcesChests.SmallChests;
				break;
			case eChestType.MEDIUM:
				_models = ResourcesChests.MediumChests;
				break;
			case eChestType.LARGE:
				_models = ResourcesChests.LargeChests;
				break;
		}
	}

	//=====================================================
}
