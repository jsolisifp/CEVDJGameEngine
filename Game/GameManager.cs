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
            Console.WriteLine($"Intentando transicionar al estado: {newState}");
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

        private async void WaitForObjectsToSettle()
        {
            int maxWaitTime = 5000; // Tiempo máximo de espera (5 segundos)
            int elapsedTime = 0;
            int checkInterval = 100; // Intervalo de verificación (100 ms)

            Console.WriteLine("Esperando que los objetos se detengan...");

            while (!AreAllObjectsStill() && elapsedTime < maxWaitTime)
            {
                await Task.Delay(checkInterval);
                elapsedTime += checkInterval;
            }

            // Solo reiniciar si los objetos realmente se han detenido
            if (!AreAllObjectsStill())
            {
                Console.WriteLine("Los objetos siguen en movimiento después del tiempo límite. Reiniciando...");
                ResetBall();
            }

            // Evaluar el resultado del turno
            if (pins.All(pin => pin.Isfallen))
            {
                Console.WriteLine("¡Todos los pines han caído! Cambiando a FinExito...");
                SetState(GameState.FinExito);
            }
            else if (remainingTurns > 0)
            {
                remainingTurns--;
                Console.WriteLine($"Turnos restantes: {remainingTurns}. Preparando siguiente turno...");
                ResetBall();
                SetState(GameState.MoverManoLibre);
            }
            else
            {
                Console.WriteLine("No quedan turnos. Cambiando a FinFracaso...");
                SetState(GameState.FinFracaso);
            }
        }




        private void ResetBall()
        {
            Console.WriteLine("Reiniciando la bola para el siguiente turno...");

            if (ball != null)
            {
                Console.WriteLine("Reiniciando posición y velocidades de la bola...");

                // Establecer la posición inicial de la bola
                ball.transform.position = new Vector3(0, 0.5f, 0); // Cambiar a la posición inicial deseada
                Console.WriteLine($"Nueva posición de la bola: {ball.transform.position}");

                // Reiniciar la velocidad lineal y rotacional
                Rigidbody rb = ball.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = false;       // Asegurarse de que no esté en un estado cinemático
                    rb.speed = Vector3.Zero;      // Detener la velocidad lineal
                    rb.angularSpeed = Vector3.Zero; // Detener la velocidad angular

                    // Actualizar el estado en el motor físico
                    Physics.SetKinematicBodyState(rb.Handle, ball.transform.position, ball.transform.rotation, rb.speed, rb.angularSpeed);
                    Console.WriteLine("Estado de física actualizado correctamente.");
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
                             && ballRb.speed.Length() < 0.01f // Velocidad lineal casi cero
                             && ballRb.angularSpeed.Length() < 0.01f; // Velocidad angular casi cero

            Console.WriteLine($"Estado de la bola - Quieto: {ballStill}, Velocidad: {ballRb?.speed}, Rotación: {ballRb?.angularSpeed}");
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



        private void ExitGame()
        {
            Console.WriteLine("El juego se cerrará ahora.");
            Environment.Exit(0); // Finaliza la aplicación.
        }
    }
}
