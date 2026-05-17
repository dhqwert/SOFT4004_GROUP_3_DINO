using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelixRotator : MonoBehaviour
{
    public float rotationSpeed = 300f;
    public float rotationSpeedAndroid = 50f;

    private void Update () {
        
        #if UNITY_EDITOR
            // this input is for pc its not working in mobile
            if(Input.GetMouseButton (0)) {
                float mouseX = Input.GetAxisRaw ("Mouse X");
                // Đã sửa trục X và Z thành 0f
                transform.Rotate (0f, -mouseX * rotationSpeed * Time.deltaTime, 0f);
            }

        #elif UNITY_ANDROID
            // for mobile
            if(Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved) {
                float xDeltaPos = Input.GetTouch (0).deltaPosition.x;
                // Đã sửa trục X và Z thành 0f
                transform.Rotate (0f, -xDeltaPos * rotationSpeedAndroid * Time.deltaTime, 0f);
            }
        #endif
    }
}