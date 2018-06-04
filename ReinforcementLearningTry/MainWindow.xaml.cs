using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ReinforcementLearningTry
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Random rand = new Random();

        const int anzahlZellenBreit = 5;
        const int anzahlZellenHoch = 5;
        Rectangle[,] felder = new Rectangle[anzahlZellenBreit, anzahlZellenHoch];

        int anzahlDerZellen = anzahlZellenBreit * anzahlZellenHoch;
        float eineEinheit;

        float[,] rMatrix = new float[anzahlZellenHoch * anzahlZellenBreit, 5];
        float[] rewardValuesForStates = new float[25];
        float[,] qMatrix = new float[anzahlZellenHoch * anzahlZellenBreit, 5];

        Ellipse el;
        float posX;
        float posY;
        float gamma = 0.3f;//discount factor
        float alpha = 1f;//learning rate
        //int step = 0;
        //int initialState = 0;
        int currentState = 0;
        //int nextState;
        int winningState = 24;
        List<int> winningStates = new List<int>();
        List<int> losingStates = new List<int>();

        DispatcherTimer timer = new DispatcherTimer();


        public MainWindow()
        {
            InitializeComponent();
                        
            //REWARDS:
            rewardValuesForStates[24] = 10000000;
            //rewardValuesForStates[21] = 0;

            rewardValuesForStates[23] = -10;
            rewardValuesForStates[23 - 5] = -10;
            rewardValuesForStates[23 - 15] = -10;
            //rewardValuesForStates[21] = -100;
            rewardValuesForStates[21 - 5] = -10;
            //rewardValuesForStates[21 - 10] = -10;
            rewardValuesForStates[23 - 10] = -10;
            rewardValuesForStates[1] = -10;
            rewardValuesForStates[6] = -10;
            
            for (int i = 0; i < anzahlDerZellen; i++)
            {
                if (rewardValuesForStates[i] < 0)
                {
                    losingStates.Add(i);
                }
                else if (rewardValuesForStates[i] > 0)
                {
                    winningStates.Add(i);
                }
            }
            

            //TIMER:
            timer.Interval = TimeSpan.FromSeconds(0.0001);
            timer.Tick += Timer_Tick;
        }

        int count = 0;
        bool schonmal, lol = false;

        private void Timer_Tick(object sender, EventArgs e)
        {
            int action = 4;
            if (!schonmal) {
                checkBoxEnableBestAction.IsChecked = true;
                checkBoxEnableMoveAgentOnUi.IsChecked = true;
                schonmal = true;
            }

            action = agentMakeDecision();
            moveAgent(action);
            if (checkBoxEnableMoveAgentOnUi.IsChecked == true)
            {
                moveAgentOnUi(action);
                //System.Threading.Thread.Sleep(100);
            }
            if (winningStates.Contains(currentState))
            {
                resetGame();
                if (!lol)
                {
                    labelFirstWin.Content += count.ToString();
                    lol = true;
                }
                
            }
            else if (losingStates.Contains(currentState))
            {
                resetGame();
            }

            count++;
            labelCount.Content = count.ToString();
            /*if (count > 100000 && currentState == 0)
            {
                checkBoxEnableMoveAgentOnUi.IsChecked = true;
                checkBoxEnableBestAction.IsChecked = true;
            }*/
        }
        
        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < anzahlZellenHoch; i++)
            {
                for (int j = 0; j < anzahlZellenBreit; j++)
                {
                    Rectangle r = new Rectangle();
                    r.Width = zeichenfläche.ActualWidth / anzahlZellenBreit-2.0;
                    r.Height = zeichenfläche.ActualHeight / anzahlZellenHoch-2.0;
                    //r.Fill = (würfel.Next(0, 2) == 1) ? Brushes.Cyan : Brushes.Red;
                    r.Fill = Brushes.Wheat;
                    //r.StrokeThickness =44;
                    zeichenfläche.Children.Add(r);
                    Canvas.SetLeft(r, j * zeichenfläche.ActualWidth / anzahlZellenBreit+1);
                    Canvas.SetTop(r, i * zeichenfläche.ActualHeight / anzahlZellenHoch+1);
                    //r.MouseDown += R_MouseDown;
                    felder[i, j] = r;
                    if(rewardValuesForStates[i * 5 + j] > 0)
                    {
                        r.Fill = Brushes.Green;
                    }
                    else if (rewardValuesForStates[i*5 + j] < 0)
                    {
                        r.Fill = Brushes.Red;
                    }

                    if (i == 0 && j == 0)
                    {
                        el = new Ellipse();
                        el.Width = zeichenfläche.ActualWidth / anzahlZellenBreit - 20.0;
                        el.Height = zeichenfläche.ActualHeight / anzahlZellenHoch - 20.0;
                        el.Fill = Brushes.Black;
                        zeichenfläche.Children.Add(el);
                        Canvas.SetLeft(el, j * zeichenfläche.ActualWidth / anzahlZellenBreit + 10);
                        Canvas.SetTop(el, i * zeichenfläche.ActualHeight / anzahlZellenHoch + 10);
                        posX = (float)(j * zeichenfläche.ActualWidth / anzahlZellenBreit + 10);
                        posY = (float)(i * zeichenfläche.ActualHeight / anzahlZellenHoch + 10);
                        Canvas.SetZIndex(el, Int32.MaxValue);
                    }
                }
            }

            eineEinheit = (float)zeichenfläche.ActualWidth / anzahlZellenBreit;
            timer.IsEnabled = true;
        }

        private void resetGame()
        {
            if (checkBoxEnableMoveAgentOnUi.IsChecked == true)
            {
                Canvas.SetLeft(el, 0 * zeichenfläche.ActualWidth / anzahlZellenBreit + 10);
                Canvas.SetTop(el, 0 * zeichenfläche.ActualHeight / anzahlZellenHoch + 10);
            }
            
            posX = (float)(0 * zeichenfläche.ActualWidth / anzahlZellenBreit + 10);
            posY = (float)(0 * zeichenfläche.ActualHeight / anzahlZellenHoch + 10);
            currentState = 0;
        }

        private int agentMakeDecision()
        {

            int bestAction = -1;
            int nextAction = 0;

            if (checkBoxEnableBestAction.IsChecked == true)
            {
                bestAction = getActionWithHighestQValueForCurrentState();
                nextAction = bestAction;
            }

            if (bestAction == -1)
            {
                //RANDOMZUG
                nextAction = getRandomActionsForCurrentState();
            }

            qMatrix[currentState, nextAction] += alpha*((computeQ(nextAction) - qMatrix[currentState, nextAction])-0.5f);
            
            return nextAction;
        }

        private int getRandomActionsForCurrentState()
        {
            bool notUp = false;
            bool notDown = false;
            bool notLeft = false;
            bool notRight = false;
            
            List<int> possibleActions = new List<int>(); int index = 0;
            
            if (currentState < 5)
            {
                notUp = true;
            }
            if (currentState > 19)
            {
                notDown = true;
            }
            if (currentState % 5 == 0)
            {
                notLeft = true;
            }
            if ((currentState-4) % 5 == 0)
            {
                notRight = true;
            }

            if(notUp == false)
            {
                possibleActions.Add(0);
            }
            if (notDown == false)
            {
                possibleActions.Add(1);
            }
            if (notLeft == false)
            {
                possibleActions.Add(2);
            }
            if (notRight == false)
            {
                possibleActions.Add(3);
            }

            return possibleActions[rand.Next(0, possibleActions.Count)];
        }

        private void moveAgentOnUi(int action)
        {
            if (action == 0)
            {
                posY = posY - eineEinheit;
                Canvas.SetTop(el, posY);
                 
            }
            else if (action == 1)
            {
                posY = posY + eineEinheit;
                Canvas.SetTop(el, posY);
            }
            else if (action == 2)
            {
                posX = posX - eineEinheit;
                Canvas.SetLeft(el, posX);
            }
            else if (action == 3)
            {
                posX = posX + eineEinheit;
                Canvas.SetLeft(el, posX);
            }

            //TODO
        }
        private void moveAgent(int action)
        {
            if (action == 0)
            {
                currentState -= 5;
            }
            else if (action == 1)
            {
                currentState += 5;
            }
            else if (action == 2)
            {
                currentState -= 1;
            }
            else if (action == 3)
            {
                currentState += 1;
            }
        }

        private float computeQ(int _action)//Q(state, action) = R(state, action) + gamma*SUMOF(Q(action, possibleActions)) 
        {
            //return rMatrix[currentState, _action] + gamma * sumOfRewardOfPossibleActions(_action);
            int nextState = getStateFromAction(_action);

            return (rewardValuesForStates[getStateFromAction(_action)] + 
                gamma * getHighestQValueForStateInQMatrix(nextState));
        }

        private float sumOfRewardOfPossibleActions(int _state)
        {
            float sum = 0;
            for (int i = 0; i < 5; i++)
            {
                sum += qMatrix[getStateFromAction(_state), i];
            }
            return sum;
        }//BAD

        private int getActionWithHighestQValueForCurrentState()
        {
            /*float helper = 0;
            int bestAction = -1;
            for (int i = 0; i < 5; i++)
            {
                if (helper < qMatrix[currentState, i])
                {
                    helper = qMatrix[currentState, i];
                    bestAction = i;
                }
            }
            return bestAction; float helper = 0;*/

            bool notUp = false;
            bool notDown = false;
            bool notLeft = false;
            bool notRight = false;

            List<int> unpossibleActions = new List<int>(); int index = 0;
            List<int> possibleActions = new List<int>();
            //possibleActions.Add(4);

            if (currentState < 5)
            {
                notUp = true;
            }
            if (currentState > 19)
            {
                notDown = true;
            }
            if (currentState % 5 == 0)
            {
                notLeft = true;
            }
            if ((currentState - 4) % 5 == 0)
            {
                notRight = true;
            }

            if (notUp == true)
            {
                unpossibleActions.Add(0);
            }
            else
            {
                possibleActions.Add(0);
            }
            if (notDown == true)
            {
                unpossibleActions.Add(1);
            }
            else
            {
                possibleActions.Add(1);
            }
            if (notLeft == true)
            {
                unpossibleActions.Add(2);
            }
            else
            {
                possibleActions.Add(2);
            }
            if (notRight == true)
            {
                unpossibleActions.Add(3);
            }
            else
            {
                possibleActions.Add(3);
            }
            unpossibleActions.Add(4);

            List<int> bestValuedActions = new List<int>();
            float helper = qMatrix[currentState, 0];
            int bestAction = 0;

            List<float> listWithQValuesOfPossibleActions = new List<float>();
            foreach (int item in possibleActions)
            {
                listWithQValuesOfPossibleActions.Add(qMatrix[currentState, item]);
            }
            float maxValue = listWithQValuesOfPossibleActions.Max();
            for (int i = 0; i < possibleActions.Count; i++)
            {
                if (qMatrix[currentState, possibleActions[i]].Equals(maxValue))
                {
                    bestValuedActions.Add(possibleActions[i]);
                }
            }

            return bestValuedActions[rand.Next(0, bestValuedActions.Count)];
        }

        private float getHighestQValueForStateInQMatrix(int state)
        {
            float helper = qMatrix[state, 0];
            
            //int highestQValueInState = -1;
            for (int i = 1; i < 5; i++)
            {
                if (helper < qMatrix[state, i])
                {
                    helper = qMatrix[state, i];
                }
            }
            return helper;
        }

        private int getStateFromAction(int action)
        {
            int state = -1;
            if (action == 0)
            {
                state = currentState - 5;
            }
            else if (action == 1)
            {
                state = currentState + 5;
            }
            else if (action == 2)
            {
                state = currentState - 1;
            }
            else if (action == 3)
            {
                state = currentState + 1;
            }
            else if (action == 4)
            {
                state = currentState;
            }

            //if (state > 24)
                //return currentState;

            return state;
        }

        private void sliderIntervallTimer_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            timer.Interval = TimeSpan.FromSeconds(sliderIntervallTimer.Value);
        }
    }
}
