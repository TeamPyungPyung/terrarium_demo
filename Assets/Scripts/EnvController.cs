using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;

public class EnvController : MonoBehaviour
{

    private int m_NumberOfHerbivores;

    // Start is called before the first frame update
    void Start()
    {
        m_NumberOfHerbivores = 0;
    }

    private void FixedUpdate()
    {
        
    }

    public void ReproduceHerbivore()
    {
        m_NumberOfHerbivores++;
    }

    public void DeadHerbivore()
    {
        m_NumberOfHerbivores--;
        if(m_NumberOfHerbivores == 0)
        {
            Debug.Log("no chicken now");
            // reset need.
        }
    }

    public bool isNoHerbivore()
    {
        if(m_NumberOfHerbivores == 0)
        {
            return true;
        }
        return false;
    }
}
