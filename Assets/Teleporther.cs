using UnityEngine;

public class Teleporther : MonoBehaviour
{
    [SerializeField] private ParticleSystem buzz;
    [SerializeField] private GameObject player;
    public Transform pos1;
    public Transform pos2;
    public Transform pos3;
    public Transform pos4;
    private Transform me;
    private float timer = 0;
    private Transform playtran;
    // Start is called before the first frame update
    void Start()
    {
        buzz.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        me = this.transform;
        playtran = player.transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (playtran.position.x > -6 && playtran.position.z < -7.5 && me.transform.position != pos1.transform.position)
        {
            if(timer==0)
            buzz.Play();
            timer += Time.deltaTime;
            if (timer > 2)
            {
                this.transform.position = pos1.position;
                this.transform.rotation = pos1.rotation;
                buzz.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                timer = 0;
            }
        }
        else if (playtran.position.x > -6 && playtran.position.z > -7.5 && me.transform.position != pos2.transform.position)
        {
            if(timer==0)
            buzz.Play();
            timer += Time.deltaTime;
            if (timer > 2)
            {
                this.transform.position = pos2.position;
                this.transform.rotation = pos2.rotation;
                buzz.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                timer = 0;
            }
        }
        else if (playtran.position.x < -6 && playtran.position.z < -7.5 && me.transform.position != pos3.transform.position)
        {
            if(timer == 0)
            buzz.Play();
            timer += Time.deltaTime;
            if (timer > 2)
            {
                this.transform.position = pos3.position;
                this.transform.rotation = pos3.rotation;
                buzz.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                timer = 0;
            }
        }
        else if (playtran.position.x < -6 && playtran.position.z > -7.5 && me.transform.position != pos4.transform.position)
        {
            if(timer == 0)
            buzz.Play();
            timer += Time.deltaTime;
            if (timer > 2)
            {
                this.transform.position = pos4.position;
                this.transform.rotation = pos4.rotation;
                buzz.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                timer = 0;
            }
        }
    }
}