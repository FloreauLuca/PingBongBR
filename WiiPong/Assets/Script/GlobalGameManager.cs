using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalGameManager : MonoBehaviour
{
    public static Color GetColor(int colorChoice)
    {
        if (colorChoice > 12)
        {
            colorChoice = colorChoice % 12;
        }
        switch (colorChoice)
        {
            case -1: return Color.white;
            case 0: return Color.white;
            case 1: return Color.red;
            case 2: return Color.green;
            case 3: return Color.blue;
            case 4: return Color.yellow;
            case 5: return Color.cyan;
            case 7: return Color.magenta;
            case 8: return new Color(255, 184, 0);
            case 9: return new Color(255, 62, 0);
            case 10: return new Color(0, 192, 255);
            case 11: return new Color(155, 0, 255);
            case 12: return new Color(172, 255, 0);
        }

        return Color.white;
    }
}
