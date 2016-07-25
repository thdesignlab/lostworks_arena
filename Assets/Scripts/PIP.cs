using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class PIP : MonoBehaviour 
{
#if UNITY_EDITOR
	[HideInInspector]
	public GameObject basePrefab;

	void Update()
	{
		UpdateComponents();
	}

	[ContextMenu("Reset Prefab")]
	void UpdateComponents()
	{
		UpdateComponent(basePrefab, gameObject);
	}

	[ContextMenu("Apply Prefab")]
	void ApplyComponents()
	{
		UpdateComponent(gameObject, basePrefab);
	}


	void UpdateComponent(GameObject baseObject, GameObject postObject)
	{
		var baseComponents = new List<Component>(baseObject.GetComponents(typeof(Component)));
		var postComponents = new List<Component>( postObject.GetComponents(typeof(Component)) );
		
		foreach( var component in baseComponents)
		{
			if( component is PIP || component is AttatchedPrefab )
			{
				continue;
			}
			
			var targetComponent =  postComponents.Find( (item )=> item.GetType() == component.GetType ());
			
			UnityEditorInternal.ComponentUtility.CopyComponent(component);
			
			if( targetComponent != null ){
				UnityEditorInternal.ComponentUtility.PasteComponentValues(targetComponent);
			}else{
				UnityEditorInternal.ComponentUtility.PasteComponentAsNew(gameObject);
			}
		}
		
		
		foreach( var component in postComponents )
		{
			if(  component is PIP || component is AttatchedPrefab )
			{
				continue;
			}
			
			var targetComponent =  baseComponents.Find( (item )=> item.GetType() == component.GetType ());
			if( targetComponent == null )
			{
				DestroyImmediate (component);
			}
		}
	}
#endif
}
