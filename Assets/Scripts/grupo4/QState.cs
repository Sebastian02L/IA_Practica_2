using NavigationDJIA.World;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using System;

namespace Assets.Scripts.grupo4
{
    public class QState
    {
        //Atributos//

        //Cadena que representa un estado concreto
        private string state;


        ////////////////////// Variables que definen el estado //////////////////////////////////////////

        private int distanceToOtherX; //Distancia en X hacia el jugador
        private int distanceToOtherY; //Distancia en Y hacia el jugador

        private int otherPositionFromAgent; //Representa si el jugador se encuentra al norte, sur, este, oeste o igual, respecto al agente

        private int walkable; //Representa las situaciones posibles caminables en las que se puede encontrar el agente

        //Métodos//
        //Constructor de la clase QState. Codifica los estados a partir de la posición del agente, del jugador, y de las celdas caminables.
        public QState(CellInfo agentPosition, CellInfo otherPosition, WorldInfo worldInfo)
        {
            //Codificación distancia en el eje X
            int distanceX = Math.Abs(otherPosition.x - agentPosition.x);

            if (0 <= distanceX && distanceX <= 6)
            {
                distanceToOtherX = 0; //CERCA
            }else if(7 <= distanceX && distanceX <= 14)
            {
                distanceToOtherX = 1; //MEDIA
            }else if (15 <= distanceX)
            {
                distanceToOtherX = 2; //LEJOS
            }
            else
            {
                Debug.Log("Hubo un problema a la hora de determinar la distancia en X a other");
            }

            //Codificación distancia en el eje y
            int distanceY = Math.Abs(otherPosition.y - agentPosition.y);

            if (0 <= distanceY && distanceY <= 6)
            {
                distanceToOtherY = 0; //CERCA
            }
            else if (7 <= distanceY && distanceY <= 14)
            {
                distanceToOtherY = 1; //MEDIA
            }
            else if (15 <= distanceY)
            {
                distanceToOtherY = 2; //LEJOS
            }
            else
            {
                Debug.Log("Hubo un problema a la hora de determinar la distancia en Y a other");
            }

            //Codificación posición del jugador respecto del agente
            if (otherPosition.y > agentPosition.y && otherPosition.x == agentPosition.x)
            {
                otherPositionFromAgent = 0; //NORTE
            } 
            else if (otherPosition.y > agentPosition.y && otherPosition.x > agentPosition.x)
            {
                otherPositionFromAgent = 1; //NORESTE
            }
            else if (otherPosition.y == agentPosition.y && otherPosition.x > agentPosition.x)
            {
                otherPositionFromAgent = 2; //ESTE
            }
            else if (otherPosition.y < agentPosition.y && otherPosition.x > agentPosition.x)
            {
                otherPositionFromAgent = 3; //SURESTE
            }
            else if (otherPosition.y < agentPosition.y && otherPosition.x == agentPosition.x)
            {
                otherPositionFromAgent = 4; //SUR
            }
            else if (otherPosition.y < agentPosition.y && otherPosition.x < agentPosition.x)
            {
                otherPositionFromAgent = 5; //SUROESTE
            }
            else if (otherPosition.y == agentPosition.y && otherPosition.x < agentPosition.x)
            {
                otherPositionFromAgent = 6; //OESTE
            }
            else if (otherPosition.y > agentPosition.y && otherPosition.x < agentPosition.x)
            {
                otherPositionFromAgent = 7; //NOROESTE
            }
            else if (otherPosition.y == agentPosition.y && otherPosition.x == agentPosition.x)
            {
                otherPositionFromAgent = 8; //IGUAL
            }
            else
            {
                Debug.Log("Hubo un error a la hora de determinar la posición de other respecto de la del agente");
            }

            //Codificación de la situación caminable
            CellInfo topCell;
            CellInfo bottomCell;
            CellInfo rightCell;
            CellInfo leftCell;

            topCell = worldInfo.NextCell(agentPosition, Directions.Up);
            bottomCell = worldInfo.NextCell(agentPosition, Directions.Down);
            rightCell = worldInfo.NextCell(agentPosition, Directions.Right);
            leftCell = worldInfo.NextCell(agentPosition, Directions.Left);

            if (topCell.Walkable && bottomCell.Walkable && rightCell.Walkable && leftCell.Walkable)
            {
                walkable = 0;
            }
            else if (!topCell.Walkable && bottomCell.Walkable && rightCell.Walkable && leftCell.Walkable)
            {
                walkable = 1;
            }
            else if (topCell.Walkable && bottomCell.Walkable && !rightCell.Walkable && leftCell.Walkable)
            {
                walkable = 2;
            }
            else if (topCell.Walkable && !bottomCell.Walkable && rightCell.Walkable && leftCell.Walkable)
            {
                walkable = 3;
            }
            else if (topCell.Walkable && bottomCell.Walkable && rightCell.Walkable && !leftCell.Walkable)
            {
                walkable = 4;
            }
            else if (!topCell.Walkable && bottomCell.Walkable && !rightCell.Walkable && leftCell.Walkable)
            {
                walkable = 5;
            }
            else if (!topCell.Walkable && !bottomCell.Walkable && rightCell.Walkable && leftCell.Walkable)
            {
                walkable = 6;
            }
            else if (!topCell.Walkable && bottomCell.Walkable && rightCell.Walkable && !leftCell.Walkable)
            {
                walkable = 7;
            }
            else if (topCell.Walkable && !bottomCell.Walkable && !rightCell.Walkable && leftCell.Walkable)
            {
                walkable = 8;
            }
            else if (topCell.Walkable && bottomCell.Walkable && !rightCell.Walkable && !leftCell.Walkable)
            {
                walkable = 9;
            }
            else if (topCell.Walkable && !bottomCell.Walkable && rightCell.Walkable && !leftCell.Walkable)
            {
                walkable = 10;
            }
            else if (!topCell.Walkable && !bottomCell.Walkable && !rightCell.Walkable && leftCell.Walkable)
            {
                walkable = 11;
            }
            else if (topCell.Walkable && !bottomCell.Walkable && !rightCell.Walkable && !leftCell.Walkable)
            {
                walkable = 12;
            }
            else if (!topCell.Walkable && !bottomCell.Walkable && rightCell.Walkable && !leftCell.Walkable)
            {
                walkable = 13;
            }
            else if (!topCell.Walkable && bottomCell.Walkable && !rightCell.Walkable && !leftCell.Walkable)
            {
                walkable = 14;
            }
            else
            {
                Debug.Log("Hubo un problema al determinar la situación caminable");
            }

            //Identificador único de cada estado
            state = distanceToOtherX.ToString() + distanceToOtherY.ToString() + otherPositionFromAgent.ToString() + walkable.ToString();
        }

        //Constructor alternativo para crear un objeto QState a partir de una cadena de identificación del estado
        public QState(string state)
        {
            this.state = state;
        }

        //Getter para el identificador
        public string GetState()
        {
            return state;
        }
    }
}