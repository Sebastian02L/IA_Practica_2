using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Action 
{
    //Atributos//
    //Representan las 4 acciones posibles.
    public float northValue; //0
    public float eastValue; //1
    public float southValue; //2
    public float westValue; //3

    //Métodos//
    public Action() //El constructor inicializa a 0 la recompensa por cada acción en un nuevo estado.
    {
        this.northValue = 0;
        this.eastValue = 0;
        this.southValue = 0;
        this.westValue = 0;
    }

    public int GetBestAction()
    {
        float maxValue = float.MinValue;
        int bestAction = -1;

        if(northValue > maxValue)
        {
            maxValue = northValue;
            bestAction = 0;
        }

        if (eastValue > maxValue)
        {
            maxValue = eastValue;
            bestAction = 1;
        }

        if (southValue > maxValue)
        {
            maxValue = southValue;
            bestAction = 2;
        }

        if (westValue > maxValue)
        {
            maxValue = westValue;
            bestAction = 3;
        }

        return bestAction;
    }

    public float GetBestActionValue()
    {
        switch (GetBestAction())
        {
            case 0:
                return northValue;
            case 1:
                return eastValue;  
            case 2: 
                return southValue;
            case 3: 
                return westValue;
        }

        return -1.0f;
    }
}
