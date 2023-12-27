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

        //El diccionario QTable representa la tabla Q utilizada para el aprendizaje por refuerzo:
        //La clave String representa un estado concreto, y el valor Action, la recompensa obtenida por cada una de las 4 acciones en ese estado.
        private Dictionary<String, Action> QTable;

        System.Random random = new System.Random();

        private float alpha;
        private float gamma;
        private float epsilon;

        private int lastAction;
        private String lastStringedState;

        public void Initialize(QMind.QMindTrainerParams qMindTrainerParams, WorldInfo worldInfo, INavigationAlgorithm navigationAlgorithm)
        {
            Debug.Log("QMindMyTrainer: initialized");

            QTable = new Dictionary<String, Action>();
            alpha = qMindTrainerParams.alpha; 
            gamma = qMindTrainerParams.gamma;
            epsilon = qMindTrainerParams.epsilon;

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
            String stringedState = currentState.GetState();

            if (!QTable.ContainsKey(stringedState))
            {
                QTable.Add(stringedState, new Action());
            }

            CheckTerminalAndRewards(lastStringedState, lastAction, stringedState);

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

        public int SelectAction(String stringedState)
        {
            int action;

            float randomNumber = (float) random.NextDouble(); //Obtiene un número aleatorio entre 0 y 1, ambos incluidos

            if(randomNumber > _qMindTrainerParams.epsilon)
            {
                action = QTable[stringedState].GetBestAction();
            }
            else
            {
                action = random.Next(0, 4);
            }

            return action;
        }

        public void CheckTerminalAndRewards(String lastStringedState, int lastAction, String stringedState)
        {
            //Comprobamos si se trata de un estado terminal
            if (AgentPosition.Equals(OtherPosition) || !AgentPosition.Walkable)
            {
                switch (lastAction)
                {
                    case 0:
                        QTable[lastStringedState].northValue = (1 - alpha) * QTable[lastStringedState].northValue + alpha * (-100 + gamma * QTable[stringedState].GetBestActionValue());
                        break;
                    case 1:
                        QTable[stringedState].eastValue = (1 - alpha) * QTable[lastStringedState].eastValue + alpha * (-100 + gamma * QTable[stringedState].GetBestActionValue());
                        break;
                    case 2:
                        QTable[stringedState].southValue = (1 - alpha) * QTable[lastStringedState].southValue + alpha * (-100 + gamma * QTable[stringedState].GetBestActionValue());
                        break;
                    case 3:
                        QTable[stringedState].westValue = (1 - alpha) * QTable[lastStringedState].westValue + alpha * (-100 + gamma * QTable[stringedState].GetBestActionValue());
                        break;
                }

                ResetEpisode();
            }
        }

        public void ResetEpisode()
        {
            CurrentEpisode += 1;
            CurrentStep = 0;
            counter = 0;
            AgentPosition = _worldInfo.RandomCell();
            OtherPosition = _worldInfo.RandomCell();

            Debug.Log("Posiciones asignadas");

            OnEpisodeFinished?.Invoke(this, EventArgs.Empty);
            OnEpisodeStarted?.Invoke(this, EventArgs.Empty);
        }
    }
}

