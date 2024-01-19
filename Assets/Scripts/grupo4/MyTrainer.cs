#region Copyright
// MIT License
// 
// Copyright (c) 2023 David Mar�a Arribas
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

using System;
using System.Collections.Generic;
using System.IO;
using Assets.Scripts.grupo4;
using NavigationDJIA.Interfaces;
using NavigationDJIA.World;
using QMind;
using QMind.Interfaces;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

namespace Grupo4
{
    public class MyTrainer : IQMindTrainer
    {
        public int CurrentEpisode { get; private set; }
        public int CurrentStep { get; private set; }
        public CellInfo AgentPosition { get; private set; }
        public CellInfo OtherPosition { get; private set; }
        public float Return { get; }
        public float ReturnAveraged { get; }
        public event EventHandler OnEpisodeStarted;
        public event EventHandler OnEpisodeFinished;

        private INavigationAlgorithm _navigationAlgorithm;
        private WorldInfo _worldInfo;
        private QMindTrainerParams _qMindTrainerParams;

        private int saveEpisode = 0; //Contador de episodios hasta guardar

        //El diccionario QTable representa la tabla Q utilizada para el aprendizaje por refuerzo:
        //La clave string representa un estado concreto, y el valor Action, la recompensa obtenida por cada una de las 4 acciones en ese estado.
        private Dictionary<string, Action> QTable;

        System.Random random = new System.Random();

        private int lastAction; //Entero que almacena la acci�n realizada en el estado anterior.
        private string lastStringedState; //Cadena que almacena el identificador del estado anterior.

        public void Initialize(QMind.QMindTrainerParams qMindTrainerParams, WorldInfo worldInfo, INavigationAlgorithm navigationAlgorithm)
        {
            QTable = new Dictionary<string, Action>();

            _navigationAlgorithm = QMind.Utils.InitializeNavigationAlgo(navigationAlgorithm, worldInfo);
            _worldInfo = worldInfo; 
            _qMindTrainerParams = qMindTrainerParams;

            //Al inicio del entrenamiento (y tambi�n al inicio de cada episodio) se elige una posici�n aleatoria del entorno tanto para el agente como para el jugador.
            AgentPosition = worldInfo.RandomCell();
            OtherPosition = worldInfo.RandomCell();
            OnEpisodeStarted?.Invoke(this, EventArgs.Empty);

            Debug.Log("MyTrainer: initialized");
        }

        //La realizaci�n de un paso
        public void DoStep(bool train)
        {
            //El agente comprueba el estado en el que se encuentra
            QState currentState = new QState(AgentPosition, OtherPosition, _worldInfo);
            string stringedState = currentState.GetState();

            //Si no est� en la tabla Q, lo a�ade
            if (!QTable.ContainsKey(stringedState))
            {
                QTable.Add(stringedState, new Action());
            }

            //Booleano para controlar si se ha llegado a un estado terminal y as� dejar de procesar el paso.
            bool terminalState = false;

            //Si hab�a un estado previo es que ya se ha realizado una acci�n por parte del agente
            if (lastStringedState != null)
            {
                //Se comprueba si el estado actual es terminal
                //CheckTerminal se encarga de actualizar con recompensa negativa en caso de haber llegado a un estado terminal
                if (CheckTerminal(lastStringedState, lastAction, stringedState))
                {
                    terminalState = true;
                }

                //Si es terminal dejamos de procesar el paso
                if (terminalState)
                {
                    return;
                }

                //Si no es terminal actualizamos con una recompensa de 0
                if (!terminalState)
                {
                    UpdateWithRewards(lastStringedState, lastAction, stringedState, 0);
                }

                //Si hemos llegado al n�mero de episodios en el que debemos guardar
                if(saveEpisode >= _qMindTrainerParams.episodesBetweenSaves)
                {
                    saveEpisode = 0; //Reseteamos el contador de episodios hasta guardar
                    SaveAsCSV(".\\Assets\\Scripts\\grupo4\\Datos.csv"); //Guardamos la tabla Q en formato .csv
                }
            }

            //Se realiza una disminuci�n progresiva de la tasa de exploraci�n para favorecer el entrenamiento
            if((CurrentEpisode == 2000 && CurrentStep == 0))
            {
                _qMindTrainerParams.epsilon = 0.4f;
            }

            if ((CurrentEpisode == 4000 && CurrentStep == 0))
            {
                _qMindTrainerParams.epsilon = 0.2f;
            }

            if ((CurrentEpisode == 6000 && CurrentStep == 0))
            {
                _qMindTrainerParams.epsilon = 0.0f;
            }

            //Se almacena en un atributo de clase el �ltimo estado visitado antes de realizar la acci�n
            lastStringedState = stringedState;

            //Se escoge una acci�n a realizar sobre el estado utilizando la estrategia de exploraci�n
            int action = SelectAction(stringedState);

            //Se almacena en un atributo de clase la �ltima acci�n realizada
            lastAction = action;

            //Desplazamos al agente en funci�n de la acci�n escogida.
            CellInfo agentCell = QMind.Utils.MoveAgent(action, AgentPosition, _worldInfo);
            AgentPosition = agentCell;

            //Desplazamos al jugador en funci�n del algoritmo A*.
            CellInfo otherCell = QMind.Utils.MoveOther(_navigationAlgorithm, OtherPosition, AgentPosition);
            if (otherCell != null) //Para no perder la �ltima ubicaci�n del jugador
            {
                OtherPosition = otherCell;
            }

            CurrentStep++; //Incrementamos el paso

            Debug.Log("MyTrainer: DoStep");
        }

        //Se selecciona una acci�n (aleatoria o la mejor de un estado dado en funci�n de la tasa de exploraci�n)
        public int SelectAction(string stringedState)
        {
            int action;

            float randomNumber = (float) random.NextDouble(); //Obtiene un n�mero aleatorio flotante entre 0 y 1, excluyendo el 1.

            if(randomNumber > _qMindTrainerParams.epsilon) //Si el n�mero random es mayor que la tasa de exploraci�n se escoge la mejor acci�n a realizar en ese estado
            {
                action = QTable[stringedState].GetBestAction();
            }
            else //En caso contrario, se escoge una acci�n aleatoria.
            {
                action = random.Next(0, 4); //Obtiene un n�mero aleatorio entre 0 y 3, ambos incluidos.
            }

            return action; //Devuelve la acci�n finalmente escogida
        }

        //M�todo para actualizar con una recompensa la tabla Q en un estado en base a la acci�n realizada y el estado alcanzado.
        public void UpdateWithRewards(string lastStringedState, int lastAction, string stringedState, float reward)
        {
            //En funci�n de la acci�n realizada se actualiza el estado en base al valor previo (experiencia), una recompensa dada, y lo buena que es la transicci�n al estado siguiente.
            switch (lastAction)
            {
                case 0: //N
                    QTable[lastStringedState].northValue = (1 - _qMindTrainerParams.alpha) * QTable[lastStringedState].northValue + _qMindTrainerParams.alpha * (reward + _qMindTrainerParams.gamma * QTable[stringedState].GetBestActionValue());
                    break;
                case 1: //E
                    QTable[lastStringedState].eastValue = (1 - _qMindTrainerParams.alpha) * QTable[lastStringedState].eastValue + _qMindTrainerParams.alpha * (reward + _qMindTrainerParams.gamma * QTable[stringedState].GetBestActionValue());
                    break;
                case 2: //S
                    QTable[lastStringedState].southValue = (1 - _qMindTrainerParams.alpha) * QTable[lastStringedState].southValue + _qMindTrainerParams.alpha * (reward + _qMindTrainerParams.gamma * QTable[stringedState].GetBestActionValue());
                    break;
                case 3: //W
                    QTable[lastStringedState].westValue = (1 - _qMindTrainerParams.alpha) * QTable[lastStringedState].westValue + _qMindTrainerParams.alpha * (reward + _qMindTrainerParams.gamma * QTable[stringedState].GetBestActionValue());
                    break;
            }
        }

        //M�todo que comprueba si hemos llegado a un estado terminal
        public bool CheckTerminal(string lastStringedState, int lastAction, string stringedState)
        {
            //Si se trata de un estado terminal
            if (AgentPosition.Equals(OtherPosition) || !AgentPosition.Walkable)
            {
                //Actualizamos con una recompensa negativa de -100 (hemos llegado a la condici�n de derrota o a una situaci�n indeseable)
                UpdateWithRewards(lastStringedState, lastAction, stringedState, -100);
                ResetEpisode(); //Se llama al reseteo de episodio
                return true;
            }

            return false;
        }

        //M�todo para resetear el episodio
        public void ResetEpisode()
        {
            CurrentEpisode += 1; //Incrementamos el episodio actual
            saveEpisode += 1; //Incrementamos el contador de episodios hasta guardar
            CurrentStep = 0; //Reseteamos el paso a 0
            lastStringedState = null; //Se limpia la variable utilizada para identificar el estado anterior
            lastAction = -1; //Se limpia la variable utilizada para identificar la �ltima acci�n realizada
            
            //Se calcula una nueva posici�n de inicio random para el nuevo episodio tanto para el agente como para el jugador
            AgentPosition = _worldInfo.RandomCell();
            OtherPosition = _worldInfo.RandomCell();

            Debug.Log("Posiciones asignadas");

            //Se notifica a los observadores pertinentes de que termin� un episodio y comienza otro
            OnEpisodeFinished?.Invoke(this, EventArgs.Empty);
            OnEpisodeStarted?.Invoke(this, EventArgs.Empty);
        }

        //M�todo para guardar el diccionario de la tabla Q en formato CSV en una ruta especificada.
        public void SaveAsCSV(string path)
        {
            //Utilizamos un StreamWriter en una ruta
            using (StreamWriter sw = new StreamWriter(path))
            {
                //El writer escribe una primera l�nea para los t�tulos de la tabla en formato CSV
                sw.WriteLine("StateKey; North; East; South; West");

                foreach(var keyValue in QTable) //Para cada clave del diccionario
                {
                    //Creamos la l�nea en formato CSV y la escribimos en el fichero. Una por fila
                    string line = $"{keyValue.Key};{keyValue.Value.northValue};{keyValue.Value.eastValue};{keyValue.Value.southValue};{keyValue.Value.westValue}";
                    sw.WriteLine(line);
                }
            }

            Debug.Log($"Datos guardados en la ruta: {path}");
        }
    }
}

