using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SlideMenuControl : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    public RectTransform botMenuRectTransform;
    private float height;
    private float startPositionY;
    private float startingAnchoredPositionY;

    public void OnDrag(PointerEventData eventdata) {
        botMenuRectTransform.anchoredPosition = new Vector2(Mathf.Clamp(startingAnchoredPositionY - (startPositionY - eventdata.position.y), GetMinPosition(), GetMaxPosition()), 0);
    }

    public void OnPointerDown(PointerEventData eventdata) {
        StopAllCoroutines();
        startPositionY = eventdata.position.y;
        startingAnchoredPositionY = botMenuRectTransform.anchoredPosition.y;
    }

    public void OnPointerUp(PointerEventData eventdata) {
        StartCoroutine(HandleMenuSlide(.25f, botMenuRectTransform.anchoredPosition.y, IsAfterHalfPoint() ? GetMinPosition() : GetMaxPosition()));
    }

    private bool IsAfterHalfPoint() {
        return botMenuRectTransform.anchoredPosition.y < height;
        //return botMenuRectTransform.anchoredPosition.y < 0;
    }
    // Start is called before the first frame update
    void Start()
    {
        height = Screen.height;
    }

    private float GetMinPosition(){
        return height / 2;
        //return -height * 0.4f;
    }
    private float GetMaxPosition(){
        return height * 1.4f;
        //return height/2;
    }

    private IEnumerator HandleMenuSlide(float slideTime, float startingY, float targetY) {
        for(float i = 0; i<slideTime; i+=0.025f) {
            botMenuRectTransform.anchoredPosition = new Vector2(Mathf.Lerp(startingY, targetY, i/slideTime), 0);
            yield return new WaitForSecondsRealtime(0.025f);
        }
    }
}
