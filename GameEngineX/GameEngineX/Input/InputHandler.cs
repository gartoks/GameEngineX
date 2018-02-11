using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Input;
using MouseEventArgs = System.Windows.Forms.MouseEventArgs;

namespace GameEngineX.Input {
    public static class InputHandler {
        static InputHandler() {
            InputHandler.pressedKeys = new HashSet<Key>();
            InputHandler.downKeys = new HashSet<Key>();
            InputHandler.releasedKeys = new HashSet<Key>();

            InputHandler.pressedButtons = new HashSet<MouseButton>();
            InputHandler.downButtons = new HashSet<MouseButton>();
            InputHandler.releasedButtons = new HashSet<MouseButton>();

            //InputHandler.distinctVirtualKeys =
            //    Enumerable.Range(0, 256).Select(KeyInterop.KeyFromVirtualKey).Where(item => item != Key.None).Distinct().Select(item => (byte)KeyInterop.VirtualKeyFromKey(item)).ToArray();

            InputHandler.prevMousePos = (0, 0);
        }

        private static HashSet<Key> pressedKeys;
        private static HashSet<Key> downKeys;
        private static HashSet<Key> releasedKeys;

        private static HashSet<MouseButton> pressedButtons;
        private static HashSet<MouseButton> downButtons;
        private static HashSet<MouseButton> releasedButtons;

        private static (int x, int y) mousePos;
        private static (int x, int y) prevMousePos;
        private static int mouseWheelDelta;

        internal static void Initialize(Control control) {
            control.MouseEnter += (o, e) => control.Focus();

            control.KeyDown += Control_KeyDown;
            control.KeyUp += Control_KeyUp;

            control.MouseMove += Control_MouseMove;
            control.MouseDown += Control_MouseDown;
            control.MouseUp += Control_MouseUp;
            control.MouseWheel += Control_MouseWheel;
        }

        internal static void Update() {
            InputHandler.prevMousePos = MousePosition;

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

            InputHandler.pressedButtons.Clear();
            InputHandler.releasedButtons.Clear();

            foreach (MouseButton mb in InputHandler.internal_downButtons) {
                if (!InputHandler.downButtons.Contains(mb))
                    InputHandler.pressedButtons.Add(mb);

                InputHandler.downButtons.Add(mb);
            }

            foreach (MouseButton mb in InputHandler.internal_upButtons) {
                downButtons.Remove(mb);
                releasedButtons.Add(mb);
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

        public static bool IsMouseButtonPressed(MouseButton button) {
            return InputHandler.pressedButtons.Contains(button);
        }

        public static bool IsMouseButtonDown(MouseButton button) {
            return InputHandler.downButtons.Contains(button);
        }

        public static bool IsMouseButtonReleased(MouseButton button) {
            return InputHandler.releasedButtons.Contains(button);
        }

        public static (int x, int y) MousePosition => (InputHandler.mousePos.x, InputHandler.mousePos.y);

        public static (int dx, int dy) MouseMovement {
            get {
                (int x, int y) mP = MousePosition;

                return (mP.x - InputHandler.prevMousePos.x, mP.y - InputHandler.prevMousePos.y);
            }
        }

        public static int MouseWheelDelta => InputHandler.mouseWheelDelta;

        private static readonly HashSet<Key> internal_downKeys = new HashSet<Key>();
        private static void Control_KeyDown(object sender, KeyEventArgs e) {
            Key key = KeyInterop.KeyFromVirtualKey((int)e.KeyCode);
            InputHandler.internal_downKeys.Add(key);
        }

        private static readonly HashSet<Key> internal_upKeys = new HashSet<Key>();
        private static void Control_KeyUp(object sender, KeyEventArgs e) {
            Key key = KeyInterop.KeyFromVirtualKey((int)e.KeyCode);
            InputHandler.internal_upKeys.Add(key);
        }

        private static void Control_MouseMove(object sender, MouseEventArgs e) {
            InputHandler.mousePos = (e.X, e.Y);
        }
        private static readonly HashSet<MouseButton> internal_downButtons = new HashSet<MouseButton>();
        private static void Control_MouseDown(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Left)
                internal_downButtons.Add(MouseButton.Left);
            else if (e.Button == MouseButtons.Right)
                internal_downButtons.Add(MouseButton.Right);
            else if (e.Button == MouseButtons.Middle)
                internal_downButtons.Add(MouseButton.Middle);
        }

        private static readonly HashSet<MouseButton> internal_upButtons = new HashSet<MouseButton>();
        private static void Control_MouseUp(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Left)
                internal_upButtons.Add(MouseButton.Left);
            else if (e.Button == MouseButtons.Right)
                internal_upButtons.Add(MouseButton.Right);
            else if (e.Button == MouseButtons.Middle)
                internal_upButtons.Add(MouseButton.Middle);
        }

        private static void Control_MouseWheel(object sender, MouseEventArgs e) {
            InputHandler.mouseWheelDelta += e.Delta;
        }

        //[DllImport("user32.dll")]
        //[return: MarshalAs(UnmanagedType.Bool)]
        //private static extern bool GetKeyboardState(byte[] keyState);
    }
}
