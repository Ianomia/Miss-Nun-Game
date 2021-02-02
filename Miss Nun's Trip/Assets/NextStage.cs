using Lightbug.Overseer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NextStage : MonoBehaviour
{
    OverseerController controller = null;

    // Start is called before the first frame update
    void Start()
    {
        controller = OverseerController.Instance;

        if (controller == null)
            throw new System.Exception("Overseer controller not instantiated.");
    }


    public void GoToNextElement()
    {
        controller.GoToNextElement();
    }
}
