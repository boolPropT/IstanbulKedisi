using UnityEngine;
using System;

public class Can : MonoBehaviour
{
    public int canPuani = 3;

    public Action<Can> Oldu;

    public void HasarAl(int miktar)
    {
        canPuani -= miktar; //can azalýyor...
        if (canPuani <= 0) 
        {
            Oldu?.Invoke(this);
            GetComponent<MartiUcuslaYokOl>()?.UcuslaAyril();
        }
    }
}