using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class GameSuperviser : MonoBehaviour
{
    public Text fpsDisplay;

    private int fpsCurrent;
    private int fpsNbFrames = 5;
    private float[] fpsTimeDeltas;
    // Start is called before the first frame update
    void Start()
    {
        fpsCurrent = 0;
        fpsTimeDeltas = new float[fpsNbFrames];
    }

    // Update is called once per frame
    void Update()
    {
        fpsCurrent = (fpsCurrent + 1) % fpsNbFrames;
        fpsTimeDeltas[fpsCurrent] = Time.deltaTime;
        fpsDisplay.text = "FPS: " + (fpsNbFrames/fpsTimeDeltas.Sum()).ToString("####");
    }
}
