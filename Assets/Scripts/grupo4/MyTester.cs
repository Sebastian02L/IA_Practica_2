#region Copyright
// MIT License
// 
// Copyright (c) 2023 David María Arribas
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
#endregion

using Assets.Scripts.grupo4;
using NavigationDJIA.World;
using QMind.Interfaces;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace QMind
{
    public class MyTester : IQMind
    {
        System.Random random = new System.Random();

        //Tabla Q leida de fichero
        private Dictionary<string, Action> QTable;

        //Información del mundo
        private WorldInfo _worldInfo;

        public void Initialize(WorldInfo worldInfo)
        {
            QTable = new Dictionary<string, Action>();
            _worldInfo = worldInfo;
            ReadCSV(".\\Assets\\Scripts\\grupo4\\Datos.csv");

            Debug.Log("MyTester: initialized");
        }

        public CellInfo GetNextStep(CellInfo currentPosition, CellInfo otherPosition)
        {
            //El agente comprueba el estado en el que se encuentra
            QState currentState = new QState(currentPosition, otherPosition, _worldInfo);
            string stringedState = currentState.GetState();

            //Se escoge la mejor acción a realizar en el estado actual
            int action = SelectAction(stringedState);

            //Desplazamos al agente en función de la acción escogida.
            CellInfo agentCell = QMind.Utils.MoveAgent(action, currentPosition, _worldInfo);

            Debug.Log("MyTester: GetNextStep");
            return agentCell;
        }

        //Método para leer de formato CSV en la ruta especificada
        public void ReadCSV(string path)
        {
            using (StreamReader reader = new StreamReader(path))
            {
                bool firstLineRead = false;

                while (!reader.EndOfStream)
                {
                    //Para ignorar la primera línea (Nombres de las columnas de la tabla Q)
                    if (!firstLineRead)
                    {
                        string line = reader.ReadLine();
                        firstLineRead = true;
                    }
                    else
                    {
                        string line = reader.ReadLine();
                        string[] values = line.Split(';');

                        string key = values[0];
                        Action action = new Action(float.Parse(values[1]), float.Parse(values[2]), float.Parse(values[3]), float.Parse(values[4]));
                        QTable.Add(key, action);
                    }
                }
            }

            Debug.Log($"Datos leidos de la ruta: {path}");
        }

        //Selecciona la mejor acción de cada estado
        public int SelectAction(string stringedState)
        {
            int action;

            if (QTable.ContainsKey(stringedState)) //Si el estado está en la tabla Q
            {
                action = QTable[stringedState].GetBestAction();
            }
            else //En caso contrario, se escoge una acción aleatoria (No se examinó ese estado durante el entrenamiento)
            {
                Debug.Log("No conozco este estado");
                action = random.Next(0, 4); //Obtiene un número aleatorio entre 0 y 3, ambos incluidos.
            }

            return action; //Devuelve la acción finalmente escogida
        }
    }
}