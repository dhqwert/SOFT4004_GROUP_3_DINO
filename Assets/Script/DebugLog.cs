using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems; // Cần thư viện này để bắt click UI

public class DebugLog : MonoBehaviour
{
    void Update()
    {
        // Khi bạn nhấn chuột trái
        if (Input.GetMouseButtonDown(0))
        {
            // 1. Dò xem có bấm vào UI (Canvases, Buttons, Images...) không
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                PointerEventData pointerData = new PointerEventData(EventSystem.current)
                {
                    position = Input.mousePosition
                };

                List<RaycastResult> results = new List<RaycastResult>();
                EventSystem.current.RaycastAll(pointerData, results);

                if (results.Count > 0)
                {
                    string uiNames = "";
                    for(int i=0; i < results.Count; i++)
                    {
                        uiNames += results[i].gameObject.name;
                        if(i < results.Count - 1) uiNames += " -> ";
                    }

                    Debug.Log($"<color=cyan>[UI CLICK]</color> Chỗ bạn bấm đâm xuyên qua {results.Count} lớp UI: <b>{uiNames}</b>\n<i>(Thằng nằm đầu tiên <b>{results[0].gameObject.name}</b> là thằng đang che các thằng dưới)</i>");
                    return; // Nếu trúng giao diện UI thì thoát luôn, không dò 3D nữa
                }
            }

            // 2. Dò xem có bấm vào không gian 3D (Bắt buộc Object phải có Collider)
            if (Camera.main != null)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    Debug.Log($"<color=yellow>[3D CLICK]</color> Bạn vừa bấm vào vật thể 3D: <b>{hit.collider.gameObject.name}</b>");
                }
                else
                {
                    Debug.Log("<color=gray>[CLICK]</color> Bấm ra khoảng không");
                }
            }
        }
    }
}
