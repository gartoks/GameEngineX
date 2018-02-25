namespace GameEngineX.Game.GameObjects.GameObjectComponents.UserInterface {
    public class GUIToggleButton : GUIButton {

        public bool ToggleState { get; private set; }

        protected override void AdditionalInitialize() {
            base.AdditionalInitialize();

            OnButtonClick += button => ToggleState = !ToggleState;
        }

    }
}