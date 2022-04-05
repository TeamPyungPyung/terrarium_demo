using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public enum CreatureType
{
    Herbivore,
    Carnivore
}

public class CreatureAgent : Agent
{
    [Header("Creature Type")]
    public CreatureType CreatureType;

    [Header("Creature Points (100 Max)")]
    public float MaxEnergy;
    public float MatureSize;
    public float GrowthRate;
    public float EatingSpeed;
    public float MaxSpeed;
    public float AttackDamage;
    public float DefendDamage;
    public float Eyesight;

    [Header("Monitoring")]
    public float Energy;
    public float Size;
    public float Age;
    public string currentAction;

    [Header("Child")]
    public GameObject ChildSpawn;

    [Header("Species Parameters")]
    public float AgeRate = 0.001f;

    [Header("envController")]
    public GameObject env;

    private GameObject Environment;
    private EnvController EnvController;
    private Rigidbody agentRB;
    private float nextAction;
    private bool died;

    //need?
    //private RayPerceptionSensor rayPer;
    //academy
    private int count;  // for child numbering.
    private Vector2 bounds;

    // Do i need Awake? what's diff with OnEpisodeBegin?
    //private void Awake()
    //{
    //   OnEpisodeBegin();
    //}

    public override void OnEpisodeBegin()
    {
        Size = 1;
        Energy = 1;
        Age = 0;
        //implement need.
        bounds = GetEnvironmentBounds();
        var x = Random.Range(-bounds.x, bounds.x);
        var z = Random.Range(-bounds.y, bounds.y);
        transform.position = new Vector3(x, 1, z);
        // implement need.
        TransformSize();

        //need?
        Initialize();

    }

    public override void Initialize()
    {
        //base.Initialize();
        //rayPer = GetComponent<RayPerceptionSensor>();
        //EnvController = GetComponentInParent<EnvController>();

        agentRB = GetComponent<Rigidbody>();
        currentAction = "Idle";
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        Vector3 localVelocity = transform.InverseTransformDirection(agentRB.velocity);
        sensor.AddObservation(localVelocity.x);
        sensor.AddObservation(localVelocity.z);
        sensor.AddObservation(Energy);
        sensor.AddObservation(Size);
        sensor.AddObservation(Age);
        //implem. need.
        sensor.AddObservation(CanEat);
        sensor.AddObservation(CanReproduce);
    }
    // Start is called before the first frame update

    public override void OnActionReceived(ActionBuffers actions)
    {

        //0 Move, 1 Eat, 2 Reproduce, 3 Attack, 4 Defend, 5 moveforward?, 6 rotate

        if (actions.DiscreteActions[0] > .5f)
        {
            // move
            float forwardAmount = actions.DiscreteActions[5];
            if (forwardAmount > .5f)
            {
                transform.position += transform.forward;
            }

            float turnAmount = 0f;
            if (actions.DiscreteActions[6] == 1f)
            {
                turnAmount = -1f;   //left
            }
            else if (actions.DiscreteActions[6] == 2f)
            {
                turnAmount = 1f;    //right
            }
            transform.Rotate(transform.up * turnAmount * Time.fixedDeltaTime * MaxSpeed * 100);
            currentAction = "Moving";
            nextAction = Time.timeSinceLevelLoad + (25 / MaxSpeed);


        }
        else if (actions.DiscreteActions[1] > .5f)
        {
            Eat();
        }
        else if (actions.DiscreteActions[2] > .5f)
        {
            Reproduce();
        }
        // attack, defend later;


    }
    void Start()
    {
        EnvController = env.GetComponent<EnvController>();

        EnvController.ReproduceHerbivore();
        OnEpisodeBegin();
    }

    // Update is called once per frame
    void Update()
    {
        if (OutOfBounds)
        {
            //Debug.Log("Out");
            AddReward(-1f);
            EnvController.DeadHerbivore();
            if (EnvController.isNoHerbivore())
            {
                // end episode
                EndEpisode();
            }
            gameObject.SetActive(false);
            return;
        }
        if (false && Buried)
        {
            //Debug.Log("buried");
            died = true;
            EnvController.DeadHerbivore();
            if (EnvController.isNoHerbivore())
            {
                // end episode
                EndEpisode();
            }
            gameObject.SetActive(false);
            return;
        }
        if (Dead)
        {
            EnvController.DeadHerbivore();
            if (EnvController.isNoHerbivore())
            {
                // end episode
                EndEpisode();
            }
            gameObject.SetActive(false);
            return;
        }

        if (CanGrow) Grow();
        // Age += AgeRate;

    }

    bool Buried
    {
        get
        {
            Energy -= AgeRate;
            return Energy < 0;
        }
    }

    private Vector2 GetEnvironmentBounds()
    {
        Environment = transform.parent.gameObject;
        var xs = Environment.transform.localScale.x;
        var zs = Environment.transform.localScale.z;
        return new Vector2(xs, zs) * 25;
    }
    public bool OutOfBounds
    {
        get
        {
            if (transform.position.y < 0) return true;
            if (transform.position.x > bounds.x || transform.position.x < -bounds.x || transform.position.z > bounds.y || transform.position.z < -bounds.y) return true;
            else
                return false;
        }
    }

    void TransformSize()
    {
        transform.localScale = Vector3.one * Mathf.Pow(Size, 0.2f);
    }

    bool CanGrow
    {
        get
        {
            return Energy > ((MaxEnergy / 2) + 1);
        }
    }

    bool CanEat
    {
        get
        {
            if (CreatureType == CreatureType.Herbivore)
            {
                // firstadjacent
                if (FirstAdjacent("Plant") != null) return true;
            }
            return false;
        }
    }

    private GameObject FirstAdjacent(string tag)
    {
        var colliders = Physics.OverlapSphere(transform.position, 1.2f * Size);
        foreach (var collider in colliders)
        {
            if (collider.gameObject.tag == tag)
            {
                return collider.gameObject;
            }
        }
        return null;
    }

    bool CanReproduce
    {
        get
        {
            if (Size >= MatureSize && CanGrow) return true;
            else return false;
        }
    }

    bool Dead
    {
        get
        {
            if (died) return true;
            if (Age > MatureSize)
            {
                currentAction = "Dead";
                died = true;
                Energy = Size;
                AddReward(0.2f); // ??? Why reward to dead?
                return true;
            }
            return false;
        }
    }

    void Grow()
    {
        if (Size > MatureSize) return;
        Energy = Energy / 2;
        Size += GrowthRate * Random.value;
        nextAction = Time.timeSinceLevelLoad + (25 / MaxSpeed);
        currentAction = "Growing";
        TransformSize();
    }

    void Reproduce()
    {
        if (CanReproduce)
        {
            var vec = Random.insideUnitCircle * 5;
            var go = Instantiate(ChildSpawn, new Vector3(vec.x, 0, vec.y), Quaternion.identity, Environment.transform);
            go.name = go.name + (count++).ToString();
            var ca = go.GetComponent<CreatureAgent>();
            ca.OnEpisodeBegin();    // i hope it work.
            Energy = Energy / 2;
            AddReward(0.2f);
            currentAction = "Reproducing";
            nextAction = Time.timeSinceLevelLoad + (25 / MaxSpeed);
        }
    }

    public void Eat()
    {

        if (CreatureType == CreatureType.Herbivore)
        {

            var adj = FirstAdjacent("Plant");
            if (adj != null)
            {
                Debug.Log("Eat doing");
                var creature = adj.GetComponent<Plant>();
                var consume = Mathf.Min(creature.Energy, 5);
                creature.Energy -= consume;
                if (creature.Energy < .1) Destroy(adj);
                Energy += consume;
                AddReward(0.1f);
                nextAction = Time.timeSinceLevelLoad + (25 / EatingSpeed);
                currentAction = "Eating";
            }
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        int forwardAction = 0;
        int turnAction = 0;

        if (Input.GetKey(KeyCode.UpArrow))
        {
            //Debug.Log("up");
            actionsOut.DiscreteActions.Array[0] = 1;
            forwardAction = 1;
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            actionsOut.DiscreteActions.Array[0] = 1;
            turnAction = 1; // left
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            actionsOut.DiscreteActions.Array[0] = 1;
            turnAction = 2; // right;
        }

        if (Input.GetKey(KeyCode.E))
        {
            Debug.Log("E input");
            actionsOut.DiscreteActions.Array[1] = 1;
        }
        if (Input.GetKey(KeyCode.R))
        {
            actionsOut.DiscreteActions.Array[2] = 1;
        }


        actionsOut.DiscreteActions.Array[5] = forwardAction;
        actionsOut.DiscreteActions.Array[6] = turnAction;
    }

    public void MonitorLog()
    {

    }
}
