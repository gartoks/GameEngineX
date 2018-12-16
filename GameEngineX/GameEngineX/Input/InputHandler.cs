using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Input;
using GameEngineX.Utility.Math;
using MouseEventArgs = System.Windows.Forms.MouseEventArgs;

namespace GameEngineX.Input {
    public static class InputHandler {
        static InputHandler() {
            InputHandler.keyChars = new Dictionary<Key, char>();
            InputHandler.pressedKeys = new HashSet<Key>();
            InputHandler.downKeys = new HashSet<Key>();
            InputHandler.releasedKeys = new HashSet<Key>();

            InputHandler.pressedButtons = 0;//new HashSet<MouseButton>();
            InputHandler.downButtons = 0;//new HashSet<MouseButton>();
            InputHandler.releasedButtons = 0;//new HashSet<MouseButton>();

            //InputHandler.distinctVirtualKeys =
            //    Enumerable.Range(0, 256).Select(KeyInterop.KeyFromVirtualKey).Where(item => item != Key.None).Distinct().Select(item => (byte)KeyInterop.VirtualKeyFromKey(item)).ToArray();

            InputHandler.prevMousePos = (0, 0);
        }

        private static int ControlHeight;

        private static Dictionary<Key, char> keyChars;

        private static HashSet<Key> pressedKeys;
        private static HashSet<Key> downKeys;
        private static HashSet<Key> releasedKeys;

        private static int pressedButtons;
        private static int downButtons;
        private static int releasedButtons;
        //private static HashSet<MouseButton> pressedButtons;
        //private static HashSet<MouseButton> downButtons;
        //private static HashSet<MouseButton> releasedButtons;

        private static (int x, int y) mousePos;
        private static (int x, int y) prevMousePos;
        private static int mouseWheelDelta;

        public delegate void KeyEvent(Key key, KeyModifiers modifiers);
        public delegate void KeyPressEvent(char keyChar);
        public delegate void MouseMoveEvent(int x, int y);
        public delegate void MouseButtonEvent(MouseButton button, int x, int y);
        public delegate void MouseWheelEvent(int wheelDelta, int x, int y);

        public static event KeyEvent OnKeyDown;
        public static event KeyEvent OnKeyUp;
        public static event KeyPressEvent OnKeyPressed;
        public static event MouseMoveEvent OnMouseMove;
        public static event MouseButtonEvent OnMouseButtonDown;
        public static event MouseButtonEvent OnMouseButtonUp;
        public static event MouseWheelEvent OnMouseWheelMove;


        internal static void Initialize(Control control) {
            ControlHeight = control.Height;

            control.MouseEnter += (o, e) => control.Focus();

            control.KeyDown += Control_KeyDown;
            control.KeyUp += Control_KeyUp;
            control.KeyPress += Control_KeyPress;

            control.MouseMove += Control_MouseMove;
            control.MouseDown += Control_MouseDown;
            control.MouseUp += Control_MouseUp;
            control.MouseWheel += Control_MouseWheel;
        }

        internal static void Update() {
            InputHandler.prevMousePos = mousePos;

            InputHandler.pressedKeys.Clear();
            InputHandler.releasedKeys.Clear();

            foreach (Key key in InputHandler.internal_downKeys) {
                if (!InputHandler.downKeys.Contains(key))
                    InputHandler.pressedKeys.Add(key);
                InputHandler.downKeys.Add(key);
            }

            foreach (Key key in InputHandler.internal_upKeys) {
                downKeys.Remove(key);
                releasedKeys.Add(key);
            }

            InputHandler.internal_downKeys.Clear();
            InputHandler.internal_upKeys.Clear();

            InputHandler.pressedButtons = 0;
            InputHandler.releasedButtons = 0;
            //InputHandler.pressedButtons.Clear();
            //InputHandler.releasedButtons.Clear();

            foreach (MouseButton mb in InputHandler.internal_downButtons) {
                //if (!InputHandler.downButtons.Contains(mb))
                //    InputHandler.pressedButtons.Add(mb);

                //InputHandler.downButtons.Add(mb);

                if ((InputHandler.downButtons & (int)mb) == 0)
                    InputHandler.pressedButtons |= (int)mb;

                InputHandler.downButtons |= (int)mb;
            }

            foreach (MouseButton mb in InputHandler.internal_upButtons) {
                //downButtons.Remove(mb);
                //releasedButtons.Add(mb);

                InputHandler.downButtons &= ~(int)mb;
                InputHandler.releasedButtons |= (int)mb;
            }

            InputHandler.internal_downButtons.Clear();
            InputHandler.internal_upButtons.Clear();
        }

        internal static void LateUpate() {
            InputHandler.mouseWheelDelta = 0;
        }

        public static bool IsKeyPressed(Key key) {
            return InputHandler.pressedKeys.Contains(key);
        }

        public static bool IsKeyDown(Key key) {
            return InputHandler.downKeys.Contains(key);
        }

        public static bool IsKeyReleased(Key key) {
            return InputHandler.releasedKeys.Contains(key);
        }

        public static IEnumerable<Key> PressedKeys => InputHandler.pressedKeys;

        public static IEnumerable<Key> DownKeys => InputHandler.downKeys;

        public static char KeyToChar(Key key) {
            return InputHandler.keyChars.TryGetValue(key, out char c) ? c : '\0';
        }

        public static bool IsMouseButtonPressed(MouseButton button) {
            //return InputHandler.pressedButtons.Contains(button);
            return (InputHandler.pressedButtons & (int)button) > 0;
        }

        public static bool IsMouseButtonDown(MouseButton button) {
            //return InputHandler.downButtons.Contains(button);
            return (InputHandler.downButtons & (int)button) > 0;
        }

        public static bool IsMouseButtonReleased(MouseButton button) {
            //return InputHandler.releasedButtons.Contains(button);
            return (InputHandler.releasedButtons & (int)button) > 0;
        }

        public static IEnumerable<MouseButton> PressedMouseButtons {
            get {
                foreach (object mouseButton in Enum.GetValues(typeof(MouseButton))) {
                    MouseButton mB = (MouseButton)mouseButton;
                    if (IsMouseButtonPressed(mB))
                        yield return mB;
                }
            }
        }

        public static IEnumerable<MouseButton> DownMouseButtons {
            get {
                foreach (object mouseButton in Enum.GetValues(typeof(MouseButton))) {
                    MouseButton mB = (MouseButton)mouseButton;
                    if (IsMouseButtonDown(mB))
                        yield return mB;
                }
            }
        }

        public static IEnumerable<MouseButton> ReleasedMouseButtons {
            get {
                foreach (object mouseButton in Enum.GetValues(typeof(MouseButton))) {
                    MouseButton mB = (MouseButton)mouseButton;
                    if (IsMouseButtonReleased(mB))
                        yield return mB;
                }
            }
        }

        public static Vector2 MousePosition => new Vector2(InputHandler.mousePos.x, InputHandler.mousePos.y);

        public static Vector2 MouseMovement => new Vector2(mousePos.x - InputHandler.prevMousePos.x, mousePos.y - InputHandler.prevMousePos.y);

        public static int MouseWheelDelta => InputHandler.mouseWheelDelta;

        private static readonly HashSet<Key> internal_downKeys = new HashSet<Key>();
        private static void Control_KeyDown(object sender, KeyEventArgs e) {
            Key key = KeyInterop.KeyFromVirtualKey((int)e.KeyCode);
            InputHandler.internal_downKeys.Add(key);

            lastKeyPressed = key;

            int keyModifierFlags = (e.Alt ? (int)KeyModifiers.Alt : 0) | (e.Alt ? (int)KeyModifiers.Control : 0) | (e.Alt ? (int)KeyModifiers.Shift : 0);
            OnKeyDown?.Invoke(key, (KeyModifiers)keyModifierFlags);
        }

        private static readonly HashSet<Key> internal_upKeys = new HashSet<Key>();
        private static void Control_KeyUp(object sender, KeyEventArgs e) {
            Key key = KeyInterop.KeyFromVirtualKey((int)e.KeyCode);
            InputHandler.internal_upKeys.Add(key);

            int keyModifierFlags = (e.Alt ? (int)KeyModifiers.Alt : 0) | (e.Alt ? (int)KeyModifiers.Control : 0) | (e.Alt ? (int)KeyModifiers.Shift : 0);
            OnKeyUp?.Invoke(key, (KeyModifiers)keyModifierFlags);
        }

        private static Key? lastKeyPressed;
        private static void Control_KeyPress(object sender, KeyPressEventArgs e) {
            if (lastKeyPressed != null) {
                if (!InputHandler.keyChars.ContainsKey((Key)lastKeyPressed)) {
                    InputHandler.keyChars[(Key)lastKeyPressed] = e.KeyChar;
                }
                lastKeyPressed = null;
            }

            OnKeyPressed?.Invoke(e.KeyChar);
        }

        private static void Control_MouseMove(object sender, MouseEventArgs e) {
            InputHandler.mousePos = (e.X, ControlHeight - e.Y);

            OnMouseMove?.Invoke(e.X, ControlHeight - e.Y);
        }

        private static readonly HashSet<MouseButton> internal_downButtons = new HashSet<MouseButton>();
        private static void Control_MouseDown(object sender, MouseEventArgs e) {
            MouseButton button;
            if (e.Button == MouseButtons.Left)
                button = MouseButton.Left;
            else if (e.Button == MouseButtons.Right)
                button = MouseButton.Right;
            else if (e.Button == MouseButtons.Middle)
                button = MouseButton.Middle;
            else
                throw new Exception();

            internal_downButtons.Add(button);

            OnMouseButtonDown?.Invoke(button, e.X, ControlHeight - e.Y);
        }

        private static readonly HashSet<MouseButton> internal_upButtons = new HashSet<MouseButton>();
        private static void Control_MouseUp(object sender, MouseEventArgs e) {
            MouseButton button;
            if (e.Button == MouseButtons.Left)
                button = MouseButton.Left;
            else if (e.Button == MouseButtons.Right)
                button = MouseButton.Right;
            else if (e.Button == MouseButtons.Middle)
                button = MouseButton.Middle;
            else
                throw new Exception();

            internal_upButtons.Add(button);

            OnMouseButtonUp?.Invoke(button, e.X, ControlHeight - e.Y);
        }

        private static void Control_MouseWheel(object sender, MouseEventArgs e) {
            InputHandler.mouseWheelDelta += e.Delta;

            OnMouseWheelMove?.Invoke(e.Delta, e.X, ControlHeight - e.Y);
        }

        //[DllImport("user32.dll")]
        //[return: MarshalAs(UnmanagedType.Bool)]
        //private static extern bool GetKeyboardState(byte[] keyState);
    }
}
