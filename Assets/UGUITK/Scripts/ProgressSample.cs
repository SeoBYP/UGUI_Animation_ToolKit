using UGUIAnimationToolkit.ProgressBar;
using UnityEngine;

public class ProgressSample : MonoBehaviour
{
    [SerializeField] private UIProgressBar progressBar_1;
    [SerializeField] private UIProgressBar progressBar_2;
    [SerializeField] private UIProgressBar progressBar_3;

    public void Add()
    {
        progressBar_1.AddValue(10);
        progressBar_2.AddValue(10);
        progressBar_3.AddValue(10);
    }

    public void Minus()
    {
        progressBar_1.MinusValue(10);
        progressBar_2.MinusValue(10);
        progressBar_3.MinusValue(10);
    }
}
