using UnityEngine;

public class Caller : MonoBehaviour
{
    public Receiver receiver;

    void Start()
    {
        Debug.Log("Hello Friend");

        if (receiver != null)
        {
            receiver.OnCalled();
        }
        else
        {
            Debug.LogError("Receiver reference not assigned!");
        }
    }
}
