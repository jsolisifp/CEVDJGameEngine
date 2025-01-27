using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine
{
    internal class GameManager
    {

        private static GameManager instance;
        public static GameManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new GameManager();
                return instance;
            }
        }

        public enum GameState
        {
            Bienvenida,
            Presentando,
            MoverManoLibre,
            MoverManoConBola,
            EsperandoObjetosQuietos,
            FinFracaso,
            FinExito,
            Salir 
        }
        private GameState currentState;
        private int score = 0;
        private int remainingTurns = 3;
        private List<Pin> pins;
        private GameObject ball;
        private string currentMessage;

        public GameState CurrentState => currentState;
        public string CurrentMessage => currentMessage;

        public void InitializeGame(List<Pin> pins, GameObject ball)
        {
            this.pins = pins;
            this.ball = ball;

        }

        public void AddScore(int points)
        {
            score += points;
            Console.WriteLine("Score: " + score);
        }

        public void ResetGame()
        {
            score = 0;
            foreach (var pin in pins)
            {
                pin.Reset();
            }
        }

        public void StartGame()
        {
            SetState(GameState.Bienvenida);
        }

        public void SetState(GameState newState)
        {
            currentState = newState;
            HandleState(newState);
        }

        private void HandleState(GameState state)
        {
            switch (state)
            {
                case GameState.Bienvenida:
                    currentMessage = "¡Bienvenido al juego de bolos!\nSelecciona una opción:\n1. JUGAR\n2. SALIR";
                    Console.WriteLine(currentMessage);
                    break;

                case GameState.Presentando:
                    currentMessage = "Preparando la partida...";
                    Console.WriteLine(currentMessage);
                    StartGamePlay();
                    break;

                case GameState.MoverManoLibre:
                    currentMessage = "Desplaza la mano moviendo el ratón.";
                    Console.WriteLine(currentMessage);
                    break;

                case GameState.MoverManoConBola:
                    currentMessage = "Coge la bola pulsando el botón izquierdo.";
                    Console.WriteLine(currentMessage);
                    break;

                case GameState.EsperandoObjetosQuietos:
                    currentMessage = "Esperando que los objetos se detengan...";
                    Console.WriteLine(currentMessage);
                    WaitForObjectsToSettle();
                    break;

                case GameState.FinFracaso:
                    currentMessage = "¡Se agotaron los turnos y no has tirado todos los bolos!";
                    Console.WriteLine(currentMessage);
                    break;

                case GameState.FinExito:
                    currentMessage = $"¡Felicidades! Has tirado todos los bolos. Puntuación: {score}.";
                    Console.WriteLine(currentMessage);
                    break;

                case GameState.Salir:
                    currentMessage = "¡Gracias por jugar! Cerrando el juego...";
                    Console.WriteLine(currentMessage);
                    ExitGame();
                    break;
            }
        }

        //private void WaitForObjectsToSettle()
        //{
        //    // Simulamos la espera hasta que los objetos estén quietos.
        //    System.Threading.Tasks.Task.Run(() =>
        //    {
        //        while (!AreAllObjectsStill())
        //        {
        //            System.Threading.Thread.Sleep(100); // Espera breve para comprobar el estado.
        //        }

        //        // Una vez que todo esté quieto, procesamos los resultados.
        //        if (pins.All(pin => pin.Isfallen))
        //        {
        //            SetState(GameState.FinExito);
        //        }
        //        else if (remainingTurns > 0)
        //        {
        //            remainingTurns--;
        //            Console.WriteLine($"Turnos restantes: {remainingTurns}");
        //            ResetBall();
        //            SetState(GameState.MoverManoLibre);
        //        }
        //        else
        //        {
        //            SetState(GameState.FinFracaso);
        //        }
        //    });
        //}

        private async void WaitForObjectsToSettle()
        {
            int maxWaitTime = 5000; // Máximo tiempo de espera (5 segundos)
            int elapsedTime = 0;
            int checkInterval = 100; // Intervalo de verificación (100 ms)

            while (!AreAllObjectsStill() && elapsedTime < maxWaitTime)
            {
                await System.Threading.Tasks.Task.Delay(checkInterval);
                elapsedTime += checkInterval;
            }

            if (!AreAllObjectsStill())
            {
                // Si no se detiene dentro del tiempo, forzar la detención de la bola
                ForceStopBall();
            }

            if (pins.All(pin => pin.Isfallen))
            {
                SetState(GameState.FinExito);
            }
            else if (remainingTurns > 0)
            {
                remainingTurns--;
                Console.WriteLine($"Turnos restantes: {remainingTurns}");
                ResetBall();
                SetState(GameState.MoverManoLibre);
            }
            else
            {
                SetState(GameState.FinFracaso);
            }
        }


        private void ResetBall()
        {
            Console.WriteLine("Reiniciando la bola para el siguiente turno...");
            if (ball != null)
            {
                ball.transform.position = new Vector3(0, 0.5f, 0); // Posición inicial
                Rigidbody rb = ball.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.speed = Vector3.Zero;       // Reiniciar velocidad lineal
                    rb.angularSpeed = Vector3.Zero; // Reiniciar velocidad angular
                }
            }
        }


        private bool AreAllObjectsStill()
        {
            // Verificar si todos los pines están quietos
            bool pinsStill = pins.All(pin => pin.IsStationary());

            // Verificar si la bola está quieta
            Rigidbody ballRb = ball.GetComponent<Rigidbody>();
            bool ballStill = ballRb != null
                             && ballRb.speed.Length() < 0.01f
                             && ballRb.angularSpeed.Length() < 0.01f;

            return pinsStill && ballStill;
        }


        private void StartGamePlay()
        {
            Scene scene = SceneManager.GetActiveScene();
            string id = SceneManager.GetActiveSceneAssetId();

            if (id == null)
            {
                Editor.OpenSaveSceneModal(true); 
            }
            else
            {
                SceneSerializer.Serialize(scene, Assets.GetAssetsPath() + "\\" + id); // Guardar la escena actual.
                Console.WriteLine("¡Todo listo! Iniciando el juego...");
                Engine.Play(); // Iniciar el juego.
            }
        }

        private void ForceStopBall()
        {
            Console.WriteLine("Forzando la detención de la bola...");
            Rigidbody rb = ball.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.speed = Vector3.Zero;
                rb.angularSpeed = Vector3.Zero;
            }
        }


        private void ExitGame()
        {
            Console.WriteLine("El juego se cerrará ahora.");
            Environment.Exit(0); // Finaliza la aplicación.
        }
    }
}
