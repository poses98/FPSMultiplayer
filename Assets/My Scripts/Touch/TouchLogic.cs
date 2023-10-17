using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchLogic : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //  Is there a touch
        if(Input.touches.Length <= 0)
        {
            //  If no touches
        }
        else
        {
            for(int i = 0; i < Input.touchCount; i++)
            {
                if (this.GetComponent<GUITexture>().HitTest(Input.GetTouch(i).position))
                {
                    if(Input.GetTouch(i).phase == TouchPhase.Began)
                    {
                        Debug.Log("Touch down on " + this.name);
                    }
                }
            }
        }
    }
}
