using UnityEngine;

namespace easycodegenunity.Editor.Samples.BasicExamples.Generated
{
    public class SampleClass : MonoBehaviour, IInterfaceA, IInterfaceB
    {
        public void DoSomethingA()
        {
            Debug.Log("DoSomethingA!");
        }

        public void DoSomethingB()
        {
            Debug.Log("DoSomethingB!");
        }
    }
}