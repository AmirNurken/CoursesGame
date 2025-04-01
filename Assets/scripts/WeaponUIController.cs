using UnityEngine;
using UnityEngine.UI;

public class WeaponUIController : MonoBehaviour
{
    public Image meleeImage;
    public Image gunImage;

    void Start()
    {
        UpdateWeaponUI(1);
    }

    public void UpdateWeaponUI(int mode)
    {
        if (mode == 1)
        {
            if (meleeImage != null)
            {
                meleeImage.color = Color.green;
            }
            if (gunImage != null)
            {
                gunImage.color = Color.white;
            }
        }
        else if (mode == 2)
        {
            if (meleeImage != null)
            {
                meleeImage.color = Color.white;
            }
            if (gunImage != null)
            {
                gunImage.color = Color.green;
            }
        }
    }
}