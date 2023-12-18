using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Image FillImg;

    public void UpdateHealth(int health, int maxHealth)
    {
        FillImg.fillAmount = (float)health / maxHealth;
    }
}
