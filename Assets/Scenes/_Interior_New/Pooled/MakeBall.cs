
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MakeBall : MonoBehaviour {
	//[Header("The ball was already present in the object pooler")]
	public int ballIndex = 0;
    public float spawnInterval = 0.1f;
    float spawnProgress;
    private int ballzero;
    private float scales;
   // public float scale = 2f;
	//[Header("This ball will be added to the object pooler when the game begins")]
	//public GameObject differentBall;
	//private int differentIndex;
	private ObjectPooler OP;
	public Transform SpawnPoint;
	private void Start()
	{
		OP = ObjectPooler.SharedInstance;
		//differentIndex = OP.AddObject(differentBall);
		Random.InitState((int)System.DateTime.Now.Ticks);
        InvokeRepeating("Generate", 1.0f, spawnInterval);
    }
	// Update is called once per frame


   void Generate()
    {
        ballzero = Random.Range(0, ballIndex);
       // scales = Random.Range(0.2f, scale);
        GameObject ball = OP.GetPooledObject(ballzero);
        ball.tag = "Ball";
        //ball.transform.rotation = SpawnPoint.transform.rotation;
        float xPos = Random.Range(-10f, 10f);
        float zPos = Random.Range(-10f, 10f);
        float yPos = Random.Range(-10f, 10f);
        ball.transform.position = SpawnPoint.transform.position + xPos * Vector3.right + zPos * Vector3.forward + yPos * Vector3.left;
        ball.transform.rotation = Random.rotation;
        //ball.transform.localScale = scales * ball.transform.localScale;
        ball.SetActive(true);
        
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            StartCoroutine(WaitaMoment());
            var freeze = GameObject.FindGameObjectsWithTag("Ball");
            foreach (var obj in freeze)
            {
                obj.GetComponent<DisableAfterTime>().enabled = false;
                obj.SetActive(true);
            }
            //CancelInvoke();
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            InvokeRepeating("Generate", 1.0f, spawnInterval);
        }

    }

    public IEnumerator WaitaMoment()
    {
        yield return new WaitForSeconds(3);
        CancelInvoke();
    }


}
