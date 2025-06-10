namespace easycodegenunity.Editor.Core
{
    /// <summary>
    /// The interface for all code generators. You should implement this interface if you want to create a custom code generator.
    /// </summary>
    public interface IEasyCodeGenerator
    {
        /// <summary>
        /// This method will be called to execute the code generation process by the EasyCodeGen.
        /// </summary>
        void Execute();
    }
}