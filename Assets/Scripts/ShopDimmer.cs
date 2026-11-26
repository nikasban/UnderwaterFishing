using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ShopDimmer : MonoBehaviour, IPointerClickHandler
{
    public GameObject shopPanel;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
            gameObject.SetActive(false);
        }
    }
}