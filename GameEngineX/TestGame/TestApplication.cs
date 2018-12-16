using System.Windows.Forms;
using GameEngineX.Application;

namespace TestGame {
    public class TestApplication : ApplicationBase {

        public override string Name => "TestApplication";

        public override Form Form => MainForm.Instance;
    }
}
