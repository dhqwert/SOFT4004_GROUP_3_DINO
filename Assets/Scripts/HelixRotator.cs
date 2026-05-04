using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelixRotator : MonoBehaviour
{
    public float rotationSpeed = 8f;
    public float rotationSpeedAndroid = 0.25f;

    private void Update () {
        if (GameManager.gameOver || GameManager.levelWin || Time.timeScale == 0f) {
            return;
        }
        
        #if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
            if(Input.GetMouseButton (0)) {
                float mouseX = Input.GetAxisRaw ("Mouse X");
                transform.Rotate (0f, -mouseX * rotationSpeed, 0f);
            }

        #elif UNITY_ANDROID
            if(Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved) {
                float xDeltaPos = Input.GetTouch (0).deltaPosition.x;
                transform.Rotate (0f, -xDeltaPos * rotationSpeedAndroid, 0f);
            }
        #endif
    }
}
