using FirstPersonPlayer.Tools.Interface;

namespace SharedUI.Interface
{
    public interface IExaminable
    {
        public void StartExamining();
        public void StopExamining();
        
        public void OnFinishExamining();

        public bool ExaminableWithRuntimeTool(IRuntimeTool  tool);
    }
}