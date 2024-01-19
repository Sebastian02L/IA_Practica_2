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

    //Los enteros de 0-3 codifican la acción para su fácil utilización en el trainer.

    //Métodos//
    public Action() //El constructor inicializa a 0 la recompensa por cada acción en un nuevo estado.
    {
        this.northValue = 0;
        this.eastValue = 0;
        this.southValue = 0;
        this.westValue = 0;
    }

    //Constructor alternativo para crear un objeto Action en base a 4 valores de calidad
    public Action(float northValue, float eastValue, float southValue, float westValue)
    {
        this.northValue = northValue;
        this.eastValue = eastValue;
        this.southValue = southValue;
        this.westValue = westValue;
    }

    //Comprueba cual es la mejor acción del estado y la devuelve.
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

    //Devuelve el valor de recompensa de la mejor acción.
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
