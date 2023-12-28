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
        private int counter = 0;

        private int saveEpisode = 0;

        //El diccionario QTable representa la tabla Q utilizada para el aprendizaje por refuerzo:
        //La clave string representa un estado concreto, y el valor Action, la recompensa obtenida por cada una de las 4 acciones en ese estado.
        private Dictionary<string, Action> QTable;

        System.Random random = new System.Random();

        private int lastAction;
        private string lastStringedState;

        public void Initialize(QMind.QMindTrainerParams qMindTrainerParams, WorldInfo worldInfo, INavigationAlgorithm navigationAlgorithm)
        {
            Debug.Log("QMindMyTrainer: initialized");

            QTable = new Dictionary<string, Action>();

            _navigationAlgorithm = QMind.Utils.InitializeNavigationAlgo(navigationAlgorithm, worldInfo);
            _worldInfo = worldInfo; 
            _qMindTrainerParams = qMindTrainerParams;

            AgentPosition = worldInfo.RandomCell();
            OtherPosition = worldInfo.RandomCell();
            OnEpisodeStarted?.Invoke(this, EventArgs.Empty);
        }

        public void DoStep(bool train)
        {
            QState currentState = new QState(AgentPosition, OtherPosition, _worldInfo);
            string stringedState = currentState.GetState();

            if (!QTable.ContainsKey(stringedState))
            {
                QTable.Add(stringedState, new Action());
            }

            bool terminalState = false;

            if (lastStringedState != null)
            {
                if (CheckTerminal(lastStringedState, lastAction, stringedState))
                {
                    terminalState = true;
                }

                if (terminalState)
                {
                    return;
                }

                if (!terminalState)
                {
                    UpdateWithRewards(lastStringedState, lastAction, stringedState, 0);
                }

                if(saveEpisode == _qMindTrainerParams.episodesBetweenSaves)
                {
                    saveEpisode = 0;
                    SaveAsCSV(QTable, "Datos.csv");
                }
            }

            if((CurrentEpisode == 500 && CurrentStep == 0))
            {
                _qMindTrainerParams.epsilon = 0.4f;
            }

            if ((CurrentEpisode == 1000 && CurrentStep == 0))
            {
                _qMindTrainerParams.epsilon = 0.2f;
            }

            if ((CurrentEpisode == 1500 && CurrentStep == 0))
            {
                _qMindTrainerParams.epsilon = 0.0f;
            }

            lastStringedState = stringedState;

            int action = SelectAction(stringedState);
            lastAction = action;

            CellInfo agentCell = QMind.Utils.MoveAgent(action, AgentPosition, _worldInfo);
            AgentPosition = agentCell;

            CellInfo otherCell = QMind.Utils.MoveOther(_navigationAlgorithm, OtherPosition, AgentPosition);
            if (otherCell != null) 
            {
                OtherPosition = otherCell;
            }

            CurrentStep = counter;
            counter += 1;
            Debug.Log("QMindTrainerDummy: DoStep");
        }

        public int SelectAction(string stringedState)
        {
            int action;

            float randomNumber = (float) random.NextDouble(); //Obtiene un número aleatorio entre 0 y 1, excluyendo el 1.

            if(randomNumber > _qMindTrainerParams.epsilon)
            {
                action = QTable[stringedState].GetBestAction();
            }
            else
            {
                action = random.Next(0, 4); //Obtiene un número aleatorio entre 0 y 3, ambos incluidos.
            }

            return action;
        }

        public void UpdateWithRewards(string lastStringedState, int lastAction, string stringedState, float reward)
        {
            switch (lastAction)
            {
                case 0:
                    QTable[lastStringedState].northValue = (1 - _qMindTrainerParams.alpha) * QTable[lastStringedState].northValue + _qMindTrainerParams.alpha * (reward + _qMindTrainerParams.gamma * QTable[stringedState].GetBestActionValue());
                    break;
                case 1:
                    QTable[lastStringedState].eastValue = (1 - _qMindTrainerParams.alpha) * QTable[lastStringedState].eastValue + _qMindTrainerParams.alpha * (reward + _qMindTrainerParams.gamma * QTable[stringedState].GetBestActionValue());
                    break;
                case 2:
                    QTable[lastStringedState].southValue = (1 - _qMindTrainerParams.alpha) * QTable[lastStringedState].southValue + _qMindTrainerParams.alpha * (reward + _qMindTrainerParams.gamma * QTable[stringedState].GetBestActionValue());
                    break;
                case 3:
                    QTable[lastStringedState].westValue = (1 - _qMindTrainerParams.alpha) * QTable[lastStringedState].westValue + _qMindTrainerParams.alpha * (reward + _qMindTrainerParams.gamma * QTable[stringedState].GetBestActionValue());
                    break;
            }
        }

        public bool CheckTerminal(string lastStringedState, int lastAction, string stringedState)
        {
            //Comprobamos si se trata de un estado terminal
            if (AgentPosition.Equals(OtherPosition) || !AgentPosition.Walkable)
            {
                UpdateWithRewards(lastStringedState, lastAction, stringedState, -100);
                ResetEpisode();
                return true;
            }

            return false;
        }

        public void ResetEpisode()
        {
            CurrentEpisode += 1;
            saveEpisode += 1;
            CurrentStep = 0;
            counter = 0;
            lastStringedState = null;
            lastAction = -1;
            AgentPosition = _worldInfo.RandomCell();
            OtherPosition = _worldInfo.RandomCell();

            Debug.Log("Posiciones asignadas");

            OnEpisodeFinished?.Invoke(this, EventArgs.Empty);
            OnEpisodeStarted?.Invoke(this, EventArgs.Empty);
        }

        public void SaveAsCSV(Dictionary<string, Action> data, string path)
        {
            using (StreamWriter sw = new StreamWriter(path))
            {
                sw.WriteLine("StateKey, North, East, South, West");

                foreach(var keyValue in data)
                {
                    string line = $"{keyValue.Key};{string.Join(";", keyValue.Value.northValue)};{string.Join(";", keyValue.Value.eastValue)};{string.Join(";", keyValue.Value.southValue)};{string.Join(";", keyValue.Value.westValue)}";
                    sw.WriteLine(line);
                }
            }

            Debug.Log($"Datos guardados en la ruta: {path}");
        }
    }
}

