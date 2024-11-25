using Silk.NET.Input;
using Silk.NET.Windowing;
using System.Numerics;



namespace GameEngine
{

    internal class Input
    {
        public static Action<Vector2> onMouseMove;
        public static Action<ScrollWheel> onMouseWheel;
        public static Action<Key, int> onKeyDown;

        static IKeyboard keyboard;
        static IMouse mouse;        
        static IInputContext context;


        public static void Init(IWindow window)
        {
            context = window.CreateInput();
            keyboard = context.Keyboards.FirstOrDefault();
            mouse = context.Mice.FirstOrDefault();
            mouse.Cursor.CursorMode = CursorMode.Raw;

            keyboard.KeyDown += OnKeyDown;
            mouse.MouseMove += OnMouseMove;
            mouse.Scroll += OnMouseWheel;
        }

        public static IInputContext GetContext()
        {
            return context;
        }

        public static void Finish()
        {
            context.Dispose();
        }

        public static void SetCursorVisible(bool visible)
        {
            if(visible) { mouse.Cursor.CursorMode = CursorMode.Normal; }
            else { mouse.Cursor.CursorMode = CursorMode.Raw; }
        }

        public static bool IsKeyPressed(Key k)
        {
            return keyboard.IsKeyPressed(k);
        }

        public static Vector2 GetMousePosition()
        {
            return mouse.Position;
        }

        public static bool IsMouseButtonPressed(int button)
        {
            MouseButton mouseButton;
            if (button == 0) { mouseButton = MouseButton.Left; }
            else if (button == 1) { mouseButton = MouseButton.Right; }
            else // button == 2
            { mouseButton = MouseButton.Middle; }

            return mouse.IsButtonPressed(mouseButton);
        }

        private static void OnMouseMove(IMouse mouse, Vector2 position)
        {
            onMouseMove?.Invoke(position);
        }

        private static void OnMouseWheel(IMouse mouse, ScrollWheel scrollWheel)
        {
            onMouseWheel?.Invoke(scrollWheel);
        }

        static void OnKeyDown(IKeyboard keyboard, Key key, int arg3)
        {
            onKeyDown?.Invoke(key, arg3);
        }

        public static void Update(float deltaTime)
        {

        }

    }


}
