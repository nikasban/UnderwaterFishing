
using UnityEngine;
using UnityEngine.EventSystems;

public class ShopObject : MonoBehaviour
{
    public GameObject shopPanel;
    public GameObject dimmer;

    public Transform player;
    public float interactRange = 2f;

    void Start()
    {
        if (shopPanel != null)
            shopPanel.SetActive(false);

        if (dimmer != null)
            dimmer.SetActive(false);

        if (player == null)
        {
            GameObject p =
                GameObject.FindGameObjectWithTag("Player");

            if (p != null)
                player = p.transform;
        }
    }

    void Update()
    {
        if (shopPanel == null)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current != null &&
                EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            if (IsPlayerNearby() && WasClicked())
                OpenShop();
        }
    }

    void OpenShop()
    {
        if (shopPanel == null)
            return;

        ShopUI shop = shopPanel.GetComponentInChildren<ShopUI>(true);

        if (shop != null)
            shop.OpenShop();
        else
            shopPanel.SetActive(true);

        if (dimmer != null)
            dimmer.SetActive(false);
    }

    bool IsPlayerNearby()
    {
        if (player == null)
            return false;

        return Vector2.Distance(
            player.position,
            transform.position
        ) <= interactRange;
    }

    bool WasClicked()
    {
        if (Camera.main == null)
            return false;

        Vector2 mouse =
            Camera.main.ScreenToWorldPoint(
                Input.mousePosition
            );

        Collider2D hit = Physics2D.OverlapPoint(mouse);

        return hit != null &&
            hit.gameObject == gameObject;
    }
}
