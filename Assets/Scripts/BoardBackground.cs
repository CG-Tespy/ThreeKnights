using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardBackground : MonoBehaviour
{
	public Transform parent, background;
	private Transform firstChild, lastChild;
	
    // As per Unity's order of execution, this should start after the board is initialized
    void Start()
    {
        StartCoroutine(ExpandBoard());
    }
    
    IEnumerator ExpandBoard() {
		firstChild = parent.GetChild(0);
		lastChild = parent.GetChild(parent.childCount - 1);

		background.position = (firstChild.position + lastChild.position) / 2 + Vector3.forward;
		background.localScale = lastChild.position - firstChild.position + Vector3.forward/10f + firstChild.localScale + lastChild.localScale;
		
		yield return null;
    }
}
