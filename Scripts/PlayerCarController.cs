using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerCarController : MonoBehaviour {

    [SerializeField] float forwardSpeed;
    [SerializeField] float backwardSpeed;
    [SerializeField] float turnForceSpeed;
    [SerializeField] float rotateSpeed;
    [SerializeField] ForceMode rotationalForceMode;
    [SerializeField] AnimationCurve torqueCurve;
    [SerializeField] int SpamReverseCount;
    [SerializeField] float reverseTime;
    [SerializeField] TMP_Text reverseText;
    Vector3 startPos;
    Quaternion startRot;
    Rigidbody car;
    float horTime;
    float generalTimer;
    float velocityErrorTime;
    float playerHorz;
    public static bool startCar = false;
    bool respawn = false;
    bool stuckCheck;
    bool reversing;
    int keyCount;
    
    
    // Use this for initialization
	void Start () {
        car = GetComponent<Rigidbody>();
        startPos = transform.position;
        startRot = transform.rotation;
        StartCoroutine(CheckForZeroVelocityCondition());
	}
	
	// Update is called once per frame
	void Update () {
        RespawnCar();
        HorizontalInputTimeConversion();
        //print(car.velocity.magnitude);
	}

    private void FixedUpdate()
    {
        MoveCarForward();
        TurnCar();
    }

    void MoveCarForward()
    {
        if (!respawn && !reversing && startCar)
        {
            transform.Translate(Vector3.forward * forwardSpeed * Time.deltaTime);
        }
    }

    void MoveCarBack()
    {
        if (reversing)
        {
            transform.Translate(Vector3.back * backwardSpeed * Time.deltaTime);
            RotateCarBackwards();
        }
    }

    void RotateCarBackwards()
    {
        if (reversing)
        {
            transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);
        }
    }

    void TurnCar()
    {
        if (!respawn && !reversing && startCar)
        {
            float torAdjust = TorqueAdjuster(playerHorz);
            if (playerHorz != 0)
            {
                car.AddTorque(Vector3.up * turnForceSpeed * torAdjust * Time.deltaTime, rotationalForceMode);
            }
        }
    }

    float TorqueAdjuster(float rotInput)
    {
        if(rotInput < 0)
        {
            return -torqueCurve.Evaluate(Mathf.Abs(rotInput));
        }
        return torqueCurve.Evaluate(rotInput);
    }

    void HorizontalInputTimeConversion()
    {
        float horzInput = Input.GetAxisRaw("Horizontal");
        horTime += Time.deltaTime * horzInput;
        Mathf.Clamp(horTime, -1, 1);
        if(horzInput == 0)
        {
            horTime = 0;
            playerHorz = 0;
        }
        playerHorz = horTime;
    }

    void RespawnCar()
    {
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            RespawnCarVelocityStuck();
        }
    }

    void RespawnCarVelocityStuck()
    {
        StartCoroutine(GoDelay());
        car.velocity = Vector3.zero;
        car.angularVelocity = Vector3.zero;
        transform.position = startPos;
        transform.rotation = startRot;
        stuckCheck = false;
        reversing = false;
    }



    IEnumerator SpamToReverse()
    {
        reversing = true;
        car.velocity = Vector3.zero;
        car.angularVelocity = Vector3.zero;
       
        while(keyCount < SpamReverseCount)
        {
            if (Input.anyKeyDown)
            {
                keyCount++;
            }
            yield return null;
        }
        keyCount = 0;
        reverseText.gameObject.SetActive(false);
        StartCoroutine(ReverseCar());
    }

    IEnumerator ReverseCar()
    {
        generalTimer = 0;
        while(generalTimer < reverseTime)
        {
            MoveCarBack();
            generalTimer += Time.deltaTime;
            yield return null;
        }
        reversing = false;
    }


    IEnumerator CheckForZeroVelocityCondition()
    {
        if (!stuckCheck & !reversing)
        {
            if (car.velocity.magnitude > 0 && !respawn)
            {
                stuckCheck = true;
                StartCoroutine(VelocityTimeCheck());
            }
        }
        yield return new WaitForSeconds(0.33f);
        StartCoroutine(CheckForZeroVelocityCondition());
    }

    IEnumerator VelocityTimeCheck()
    {
        generalTimer = 0;
        while (generalTimer < 2f)
        {
            if (car.velocity.magnitude > 1.4f)
            {
                velocityErrorTime += Time.deltaTime;
            }
            else
            {
                velocityErrorTime = 0;
            }
            generalTimer += Time.deltaTime;
            yield return null;
        }
        
        if (velocityErrorTime >= 2f)
        {
            velocityErrorTime = 0;
            stuckCheck = false;
            reverseText.gameObject.SetActive(true);
            StartCoroutine(SpamToReverse());
        }
        else
        {
            stuckCheck = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "AI_Car")
        {
            RespawnCarVelocityStuck();
        }
        if(other.tag == "Item")
        {
            other.GetComponent<ItemController>().GotItem();
        }
    }

    IEnumerator GoDelay()
    {
        respawn = true;
        yield return new WaitForSeconds(3f);
        respawn = false;
    }
}
